using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using MEPTools.Util;

namespace MEPTools.FireHyrantLink
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class FireHyrantLinkCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            FireHydrantLinkForm form = new FireHydrantLinkForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    FamilyInstance fireHydrant = PickFireHyrant(uiDoc, "请选择消火栓");
                    MEPCurve mep = MEPUtil.PickMEPCurve(uiDoc, "请选择立管");
                    LinkFireHyrant(doc, fireHydrant, mep, form.Offset / 304.8, form.IsBottom);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    throw;
                }
            }

            return Result.Succeeded;
        }

        private void LinkFireHyrant(Document doc, FamilyInstance fireHydrant, MEPCurve mep, double height, bool isBottom)
        {
            using (Transaction trans = new Transaction(doc, "连接消火栓！"))
            {
                trans.Start();
                CreateMiddlePipe(doc, mep, fireHydrant, height / 304.8, isBottom);
                trans.Commit();
            }
        }

        private Connector[] FindConnector(FamilyInstance fireHydrant, MEPCurve mep, bool isBottom)
        {
            Connector fireConnector = null;
            foreach (Connector con in fireHydrant.MEPModel.ConnectorManager.Connectors)
            {
                if (fireConnector == null)
                {
                    fireConnector = con;
                }
                else
                {
                    if (isBottom)
                    {
                        if (con.Origin.Z < fireConnector.Origin.Z)
                            fireConnector = con;
                    }
                    else
                    {
                        if (con.Origin.Z > fireConnector.Origin.Z)
                            fireConnector = con;
                    }
                }
            }
            return new Connector[2] {
                fireConnector, GetNearestConnector(fireConnector, mep)
            };
        }

        private Connector GetNearestConnector(Connector fireConnector, MEPCurve mep)
        {
            double minDistance = double.MaxValue;
            Connector pipeConnector = null;
            foreach (Connector con in mep.ConnectorManager.Connectors)
            {
                var dis = fireConnector.Origin.DistanceTo(con.Origin);
                if (dis < minDistance)
                {
                    minDistance = dis;
                    pipeConnector = con;
                }
            }
            return pipeConnector;
        }

        private void CreateMiddlePipe(Document doc, MEPCurve mep, FamilyInstance fireHydrant, double offSet, bool isBottom)
        {
            Connector[] connectors = FindConnector(fireHydrant, mep, isBottom);
            XYZ startPoint = connectors[1].Origin;
            XYZ endPoint = connectors[0].Origin;

            if (isBottom)
            {
                double Z = connectors[0].Origin.Z - offSet;
                startPoint = new XYZ(connectors[1].Origin.X, connectors[1].Origin.Y, Z);
                endPoint = new XYZ(connectors[0].Origin.X, connectors[0].Origin.Y, Z);
                MEPCurve newVerticalMep = MEPFactory.CopyTo(doc, mep, connectors[0].Origin, endPoint);
                connectors[0].ConnectTo(GetNearestConnector(connectors[0], newVerticalMep));
                MEPCurve newMep = MEPFactory.CopyTo(doc, mep, startPoint, endPoint);
                connectors[1].ConnectNearConnector(doc, newMep);
                GetConnectorInPoint(newVerticalMep, ((LocationCurve)newVerticalMep.Location).Curve.GetEndPoint(1)).ConnectNearConnector(doc, newMep);
            }
            else
            {
                XYZ pipeConnectPt = new XYZ(connectors[1].Origin.X, connectors[1].Origin.Y, connectors[0].Origin.Z);
                XYZ Out = fireHydrant.HandOrientation;
                XYZ outToPipe = (pipeConnectPt - connectors[0].Origin).Normalize();
                if (1 - Math.Abs(Out.DotProduct(outToPipe)) < 0.01)// 消火栓与水管处于同一竖直平面
                {
                    XYZ vector = pipeConnectPt - connectors[0].Origin;
                    MEPCurve newMep = MEPFactory.CopyTo(doc, mep, connectors[0].Origin + vector / 4, connectors[0].Origin + vector * 3 / 4);
                    GetConnectorInPoint(newMep, connectors[0].Origin + vector / 4).ConnectTo(connectors[0]);
                    doc.Create.NewElbowFitting(GetConnectorInPoint(newMep, connectors[0].Origin + vector * 3 / 4), connectors[1]);
                }
                else// 消火栓与水管不处于同一竖直平面
                {
                    if (Out.DotProduct(outToPipe) < 0)
                        Out = -Out;
                    double length = (pipeConnectPt - connectors[0].Origin).DotProduct(Out) / 2;
                    MEPCurve newMep1 = MEPFactory.CopyTo(doc, mep, connectors[0].Origin, connectors[0].Origin + Out * length);
                    GetConnectorInPoint(newMep1, connectors[0].Origin).ConnectTo(connectors[0]);
                    XYZ ptStart = (newMep1.Location as LocationCurve).Curve.GetEndPoint(1);
                    XYZ vector = pipeConnectPt - ptStart;
                    MEPCurve newMep2 = MEPFactory.CopyTo(doc, mep, ptStart + vector / 4, ptStart + vector * 3 / 4);
                    GetConnectorInPoint(newMep1, ptStart).ConnectNearConnector(doc, newMep2);
                    connectors[1].ConnectNearConnector(doc, newMep2);
                }
            }
        }

        private Connector GetConnectorInPoint(MEPCurve mep, XYZ point)
        {
            foreach (Connector con in mep.ConnectorManager.Connectors)
            {
                if (con.Origin.IsAlmostEqualTo(point))
                    return con;
            }
            return null;
        }

        public FamilyInstance PickFireHyrant(UIDocument uiDoc, string prompt)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, new FireHyrantFilter(), prompt);
            return uiDoc.Document.GetElement(refer) as FamilyInstance;
        }
    }
}
