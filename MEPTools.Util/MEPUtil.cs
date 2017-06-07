using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPTools.Util.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Util
{
    public static class MEPUtil
    {
        /// <summary>
        /// 选择一个机电管件
        /// </summary>
        public static MEPCurve PickMEPCurve(UIDocument uiDoc, string prompt)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, new MEPSelectionFilter(), prompt);
            return uiDoc.Document.GetElement(refer) as MEPCurve;
        }

        /// <summary>
        /// 在管件上选择一个打断点
        /// </summary>
        public static XYZ PickPointOnMEPCurve(UIDocument uiDoc, string prompt, out MEPCurve mep)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.PointOnElement, prompt);
            LocationCurve locationCurve = (uiDoc.Document.GetElement(refer) as MEPCurve).Location as LocationCurve;
            IntersectionResult intersectionResult = locationCurve.Curve.Project(refer.GlobalPoint);
            mep = uiDoc.Document.GetElement(refer) as MEPCurve;
            return intersectionResult.XYZPoint;
        }

        public static PointSet PickTwoPointOnMEPCurve(UIDocument uiDoc, string[] prompts)
        {
            PointSet result = new PointSet(2);
            Reference refer1 = uiDoc.Selection.PickObject(ObjectType.PointOnElement, /*new MEPSelectionFilter(),*/ prompts[0]);
            LocationCurve locationCurve = (uiDoc.Document.GetElement(refer1) as MEPCurve).Location as LocationCurve;
            IntersectionResult intersectionResult = locationCurve.Curve.Project(refer1.GlobalPoint);
            result.AddMepCurve(uiDoc.Document.GetElement(refer1) as MEPCurve);
            result.AddPoint(intersectionResult.XYZPoint);
            Reference refer2 = uiDoc.Selection.PickObject(ObjectType.PointOnElement, /*new MEPSelectionFilter(),*/ prompts[1]);
            if (refer1.ElementId != refer2.ElementId)
            {
                locationCurve = (uiDoc.Document.GetElement(refer2) as MEPCurve).Location as LocationCurve;
                result.AddMepCurve(uiDoc.Document.GetElement(refer2) as MEPCurve);
            }
            intersectionResult = locationCurve.Curve.Project(refer2.GlobalPoint);
            result.AddPoint(intersectionResult.XYZPoint);
            return result;
        }

        /// <summary>
        /// 打断管件
        /// </summary>
        public static MEPCurve[] SliceMEPCurveIntoTwo(Document doc, MEPCurve mep, XYZ pt, double sliceSpace)
        {
            LocationCurve locationCurve = mep.Location as LocationCurve;
            XYZ StartPt = locationCurve.Curve.GetEndPoint(0);
            XYZ EndPt = locationCurve.Curve.GetEndPoint(1);
            XYZ vector = (EndPt - StartPt).Normalize();
            Line l1 = Line.CreateBound(StartPt, pt - vector * sliceSpace * 0.5);
            Line l2 = Line.CreateBound(pt + vector * sliceSpace * 0.5, EndPt);
            // 原地复制原机电管线
            MEPCurve newMEP = doc.GetElement(ElementTransformUtils.CopyElement(doc, mep.Id, XYZ.Zero).ElementAt(0)) as MEPCurve;
            LocationCurve newLocationCurve = newMEP.Location as LocationCurve;
            // 为复制的机电管线指定基线
            newLocationCurve.Curve = l1;
            // 断开原机电管线的连接，建立新机电管线的连接
            foreach (Connector Conn in mep.ConnectorManager.Connectors)
            {
                if (Conn.Origin.IsAlmostEqualTo(StartPt))
                {
                    foreach (Connector tmpConn in Conn.AllRefs)
                    {
                        if (tmpConn != null)
                        {
                            if (tmpConn.ConnectorType == ConnectorType.End ||
                                tmpConn.ConnectorType == ConnectorType.Curve ||
                                tmpConn.ConnectorType == ConnectorType.Physical)
                            {
                                if (tmpConn.Owner.UniqueId != mep.UniqueId)
                                {
                                    Conn.DisconnectFrom(tmpConn);
                                    foreach (Connector newConn in newMEP.ConnectorManager.Connectors)
                                    {
                                        if (newConn.Origin.IsAlmostEqualTo(StartPt))
                                        {
                                            newConn.ConnectTo(tmpConn);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 缩短原机电管线
            locationCurve.Curve = l2;
            MEPCurve[] result = new MEPCurve[2] { newMEP, mep };
            return result;
        }

        public static MEPCurve[] SliceMEPCurveIntoThree(Document doc, MEPCurve mep, XYZ[] pts, double sliceSpace)
        {
            LocationCurve locationCurve = mep.Location as LocationCurve;
            XYZ StartPt = locationCurve.Curve.GetEndPoint(0);
            XYZ EndPt = locationCurve.Curve.GetEndPoint(1);
            XYZ vector = (EndPt - StartPt).Normalize();
            int twicePt = vector.IsAlmostEqualTo((pts[1] - pts[0]).Normalize()) ? 1 : 0;
            MEPCurve[] tmp1 = SliceMEPCurveIntoTwo(doc, mep, pts[1 - twicePt], sliceSpace);
            MEPCurve[] tmp2 = SliceMEPCurveIntoTwo(doc, tmp1[1], pts[twicePt], sliceSpace);
            return new MEPCurve[] { tmp1[0], tmp2[0], tmp2[1] };
        }

        /// <summary>
        /// 在距离最近的连接处作弯头连接
        /// </summary>
        public static void ConnectNearConnector(this Connector This, Document doc, params MEPCurve[] meps)
        {
            Connector near = This.GetNearestConnector(meps);
            try
            {
                doc.Create.NewElbowFitting(This, near);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException Ex)
            {
                if (Ex.Message.Contains("failed to insert elbow"))
                    throw new InvalidOperationException("请尝试导入弯头族再进行操作");
            }
        }

        public static Connector GetNearestConnector(this Connector fireConnector, params MEPCurve[] meps)
        {
            double minDistance = double.MaxValue;
            Connector pipeConnector = null;
            foreach (MEPCurve mep in meps)
            {
                foreach (Connector con in mep.ConnectorManager.UnusedConnectors)
                {
                    var dis = fireConnector.Origin.DistanceTo(con.Origin);
                    if (dis < minDistance)
                    {
                        minDistance = dis;
                        pipeConnector = con;
                    }
                }
            }
            return pipeConnector;
        }

        public static void Delete(Document doc, Element elem)
        {
            doc.Delete(elem.Id);
            doc.Regenerate();
        }

        public static Line ToLine(this MEPCurve mep)
        {
            return ((LocationCurve)mep.Location).Curve as Line;
        }

        public static Connector GetConnectorInPoint(this MEPCurve mep, XYZ point)
        {
            foreach (Connector con in mep.ConnectorManager.Connectors)
            {
                if (con.Origin.IsAlmostEqualTo(point))
                    return con;
            }
            return null;
        }
    }
}
