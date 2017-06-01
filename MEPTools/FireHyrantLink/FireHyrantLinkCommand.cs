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
            FamilyInstance fireHyrant = PickFireHyrant(uiDoc, "请选择消火栓");
            MEPCurve mep = MEPUtil.PickMEPCurve(uiDoc, "请选择立管");
            double height = 200;
            bool isBottom = true;
            LinkFireHyrant(doc, fireHyrant, mep, height, isBottom);

            // 查找最近的两个connector
            ConnectorManager fireHyrantCM = fireHyrant.MEPModel.ConnectorManager;

            Connector fireHyrantConnector = null; //消火栓
            Connector pipeConnector = null;

            if (fireHyrantCM.Connectors.Size >= 2)
            {
                // 如果有两个connector 就有根据用户的选择决定消火栓的进水口（侧进水口，底进水口）           
                ConnectorSetIterator temp = fireHyrantCM.Connectors.ForwardIterator();
                temp.MoveNext();
                Connector tempcon = temp.Current as Connector;

                foreach (Connector con in fireHyrantCM.Connectors)
                {
                    if (isBottom)
                    {
                        if (con.Origin.Z < tempcon.Origin.Z)
                        {
                            fireHyrantConnector = con;
                        }
                    }
                    else
                    {
                        if (con.Origin.Z > tempcon.Origin.Z)
                        {
                            fireHyrantConnector = con;
                        }
                    }
                }
            }

            else
            {
                // 否则默认底进水口
                ConnectorSetIterator temp = fireHyrantCM.Connectors.ForwardIterator();
                temp.MoveNext();
                fireHyrantConnector = temp.Current as Connector;
            }

            double minDistance = double.MaxValue;

            foreach (Connector con in mep.ConnectorManager.Connectors)
            {
                var dis = fireHyrantConnector.Origin.DistanceTo(con.Origin);
                if (dis < minDistance)
                {
                    minDistance = dis;
                    pipeConnector = con;
                }
            }

            //CreateMiddlePipe(pipeConnector,fireHyrantConnector, ,height/304.8);

            return Result.Succeeded;
        }

        private void LinkFireHyrant(Document doc, FamilyInstance fireHyrant, MEPCurve mep, double height, bool isBottom)
        {
            ConnectorManager fireHyrantCM = fireHyrant.MEPModel.ConnectorManager;
            Connector fireHyrantConnector = null; //消火栓
            Connector pipeConnector = null;

        }

        private Connector[] FindConnector(ConnectorManager fireCM, MEPCurve mep, bool isBottom)
        {
            Connector fireConnector = null;
            if (fireCM.Connectors != null)
            {
                ConnectorSetIterator temp = fireCM.Connectors.ForwardIterator();
                temp.MoveNext();
                Connector firstConnector = temp.Current as Connector;
                if (fireCM.Connectors.Size > 1)
                {
                    foreach (Connector con in fireCM.Connectors)
                    {
                        if (isBottom)
                        {
                            if (con.Origin.Z < firstConnector.Origin.Z)
                                fireConnector = con;
                        }
                        else
                        {
                            if (con.Origin.Z > firstConnector.Origin.Z)
                                fireConnector = con;
                        }
                    }
                }
                else
                    fireConnector = firstConnector;
            }
            return new Connector[2] {
                fireConnector,GetNearestConnector(fireConnector,mep)
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
        private void CreateMiddlePipe(Connector pipeConnector, Connector fireHyrantConnector, double dim, double offSet)
        {

        }

        public FamilyInstance PickFireHyrant(UIDocument uiDoc, string prompt)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, new FireHyrantFilter(), prompt);
            return uiDoc.Document.GetElement(refer) as FamilyInstance;
        }
    }

    class FireHyrantFilter : ISelectionFilter
    {
        bool ISelectionFilter.AllowElement(Element elem)
        {
            if (elem is FamilyInstance && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment)
            {
                return true;
            }
            else
                return false;
        }

        bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }


}
