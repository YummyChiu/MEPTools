using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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
        public static XYZ PickPointOnMEPCurve(UIDocument uiDoc, MEPCurve mep, string prompt)
        {
            XYZ selectionPoint = uiDoc.Selection.PickPoint(ObjectSnapTypes.Nearest, prompt);
            LocationCurve locationCurve = mep.Location as LocationCurve;
            IntersectionResult intersectionResult = locationCurve.Curve.Project(selectionPoint);
            if (intersectionResult == null || intersectionResult.XYZPoint == null)
            {
                throw new InvalidOperationException("Method: MEPUtil.PickPointOnMEPCurve\nReason: Project Failed.");
            }
            return intersectionResult.XYZPoint;
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

        /// <summary>
        /// 在距离最近的连接处作弯头连接
        /// </summary>
        public static void ConnectNearConnector(this Connector This, Document doc, params MEPCurve[] meps)
        {
            Connector near = null;
            double min = double.MaxValue;
            foreach (MEPCurve mep in meps)
            {
                foreach (Connector Conn in mep.ConnectorManager.Connectors)
                {
                    double distance = This.Origin.DistanceTo(Conn.Origin);
                    if (distance < min)
                    {
                        min = distance;
                        near = Conn;
                    }
                }
            }
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
    }
}
