using Autodesk.Revit.DB;
using MEPTools.Util;
using MEPTools.Util.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Bend
{
    public static class BendUtil
    {
        public static void BendTwoSide(Document doc, PointSet ptSet, Direction direction, double heightOffset, double angle)
        {
            if (ptSet.IsOnSameCurve)
            {
                MEPCurve mep = ptSet.MepCurveSet[0];
                XYZ[] pts = ptSet.GetPoints(mep);
                if (Valid(mep, direction, heightOffset, angle))
                {
                    Curve curve = ((LocationCurve)mep.Location).Curve;
                    XYZ mepDirection = ((Line)curve).Direction;
                    Transform translation = GetTransform(mepDirection, direction, heightOffset);
                    MEPCurve[] meps = MEPUtil.SliceMEPCurveIntoThree(doc, mep, pts, 0);

                    double Tan = heightOffset * Math.Tan((90 - angle) * Math.PI / 180);
                    LocationCurve locationCurve = meps[1].Location as LocationCurve;
                    locationCurve.Curve = Line.CreateBound(locationCurve.Curve.GetEndPoint(0) + (locationCurve.Curve as Line).Direction * Tan, locationCurve.Curve.GetEndPoint(1) - (locationCurve.Curve as Line).Direction * Tan).CreateTransformed(translation);

                    XYZ ptStart = (meps[0].Location as LocationCurve).Curve.GetEndPoint(1);
                    XYZ ptEnd = locationCurve.Curve.GetEndPoint(0);
                    MEPCurve newMep = MEPFactory.CopyTo(doc, mep, ptStart + (ptEnd - ptStart) / 4, ptStart + (ptEnd - ptStart) * 3 / 4);
                    foreach (Connector Conn in newMep.ConnectorManager.Connectors)
                    {
                        Conn.ConnectNearConnector(doc, meps);
                    }
                    ptStart = locationCurve.Curve.GetEndPoint(1);
                    ptEnd = (meps[2].Location as LocationCurve).Curve.GetEndPoint(0);
                    newMep = MEPFactory.CopyTo(doc, mep, ptStart + (ptEnd - ptStart) / 4, ptStart + (ptEnd - ptStart) * 3 / 4);
                    foreach (Connector Conn in newMep.ConnectorManager.Connectors)
                    {
                        Conn.ConnectNearConnector(doc, meps);
                    }
                }
            }
            else
            {
                MEPCurve[] meps1 = MEPUtil.SliceMEPCurveIntoTwo(doc, ptSet.MepCurveSet[0], ptSet.GetPoints(ptSet.MepCurveSet[0])[0], 0);
                MEPCurve[] meps2 = MEPUtil.SliceMEPCurveIntoTwo(doc, ptSet.MepCurveSet[1], ptSet.GetPoints(ptSet.MepCurveSet[1])[0], 0);
                doc.Regenerate();
                MEPCurve moveMep1 = meps1[0];
                MEPCurve moveMep2 = meps2[0];
                Connector tmpConn = moveMep1.GetConnectorInPoint(moveMep1.ToLine().GetEndPoint(1));
                if (!tmpConn.GetConnectorsByStep(3).Exists(c => c.Origin.IsAlmostEqualTo(ptSet.GetPoints(ptSet.MepCurveSet[1])[0])))
                {
                    moveMep1 = meps1[1];
                }
                tmpConn = moveMep2.GetConnectorInPoint(moveMep2.ToLine().GetEndPoint(1));
                if (!tmpConn.GetConnectorsByStep(3).Exists(c => c.Origin.IsAlmostEqualTo(ptSet.GetPoints(ptSet.MepCurveSet[0])[0])))
                {
                    moveMep2 = meps2[1];
                }
                Transform translation = GetTransform(moveMep1.ToLine().Direction, direction, heightOffset);
                ElementTransformUtils.MoveElement(doc, moveMep1.Id, translation.Origin);
                doc.Regenerate();

                double Tan = heightOffset * Math.Tan((90 - angle) * Math.PI / 180);
                LocationCurve locationCurve = moveMep1.Location as LocationCurve;
                locationCurve.Curve = moveMep1.Id == meps1[0].Id ?
                    Line.CreateBound(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1) - moveMep1.ToLine().Direction * Tan) :
                    Line.CreateBound(locationCurve.Curve.GetEndPoint(0) + moveMep1.ToLine().Direction * Tan, locationCurve.Curve.GetEndPoint(1));
                locationCurve = moveMep2.Location as LocationCurve;
                locationCurve.Curve = moveMep2.Id == meps2[0].Id ?
                    Line.CreateBound(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1) - moveMep2.ToLine().Direction * Tan) :
                    Line.CreateBound(locationCurve.Curve.GetEndPoint(0) + moveMep2.ToLine().Direction * Tan, locationCurve.Curve.GetEndPoint(1));

                XYZ ptStart = ptSet.GetPoints(ptSet.MepCurveSet[0])[0];
                XYZ ptEnd = moveMep1.Id == meps1[0].Id ? moveMep1.ToLine().GetEndPoint(1) : moveMep1.ToLine().GetEndPoint(0);
                MEPCurve newMep = MEPFactory.CopyTo(doc, moveMep1, ptStart + (ptEnd - ptStart) / 4, ptStart + (ptEnd - ptStart) * 3 / 4);
                foreach (Connector Conn in newMep.ConnectorManager.Connectors)
                {
                    Conn.ConnectNearConnector(doc, meps1);
                }
                ptStart = ptSet.GetPoints(ptSet.MepCurveSet[1])[0];
                ptEnd = moveMep2.Id == meps2[0].Id ? moveMep2.ToLine().GetEndPoint(1) : moveMep2.ToLine().GetEndPoint(0);
                newMep = MEPFactory.CopyTo(doc, moveMep2, ptStart + (ptEnd - ptStart) / 4, ptStart + (ptEnd - ptStart) * 3 / 4);
                foreach (Connector Conn in newMep.ConnectorManager.Connectors)
                {
                    Conn.ConnectNearConnector(doc, meps2);
                }
            }
        }

        public static List<Connector> GetConnectorsByStep(this Connector Self, int step)
        {
            List<Connector> result = new List<Connector>();
            if (step == 0)
            {
                result.Add(Self);
            }
            else if (Self.Owner is FamilyInstance)
            {
                foreach (Connector bro in ((FamilyInstance)Self.Owner).MEPModel.ConnectorManager.Connectors)
                {
                    if (!bro.Origin.IsAlmostEqualTo(Self.Origin))
                    {
                        ConnectorSetIterator iterator = bro.AllRefs.ForwardIterator();
                        Connector next = ConnectorSetIteratorSearch<MEPCurve>(iterator);
                        if (next != null)
                            result.AddRange(next.GetConnectorsByStep(step - 1));
                        else
                            result.Add(bro);
                    }
                }
            }
            else if (Self.Owner is MEPCurve)
            {
                foreach (Connector bro in ((MEPCurve)Self.Owner).ConnectorManager.Connectors)
                {
                    if (!bro.Origin.IsAlmostEqualTo(Self.Origin))
                    {
                        ConnectorSetIterator iterator = bro.AllRefs.ForwardIterator();
                        Connector next = ConnectorSetIteratorSearch<FamilyInstance>(iterator);
                        if (next != null)
                            result.AddRange(next.GetConnectorsByStep(step - 1));
                        else
                            result.Add(bro);
                    }
                }
            }
            else
            {
                throw new Exception("GetConnectorsByStep: Connector.Owener is " + Self.Owner.GetType().FullName);
            }
            return result;
        }

        public static Connector ConnectorSetIteratorSearch<T>(ConnectorSetIterator iterator) where T : class
        {
            while (iterator.MoveNext())
            {
                if (((Connector)iterator.Current).Owner is T)
                {
                    return iterator.Current as Connector;
                }
            }
            return null;
        }

        public enum Direction { Up, Down, Left, Right };
        public static MEPCurve[] BendOneSide(Document doc, XYZ[] pts, MEPCurve mep, Direction direction, double heightOffset, double angle)
        {
            if (Valid(mep, direction, heightOffset, angle))
            {
                Curve curve = ((LocationCurve)mep.Location).Curve;
                XYZ bendDirection = (pts[1] - pts[0]).Normalize();
                XYZ mepDirection = ((Line)curve).Direction;
                int IdxAdjust = mepDirection.AngleTo(bendDirection) <= Math.PI * 0.5 ? 1 : 0;

                Transform translation = GetTransform(mepDirection, direction, heightOffset);
                MEPCurve[] meps = MEPUtil.SliceMEPCurveIntoTwo(doc, mep, pts[0], 0);

                LocationCurve locationCurve = meps[IdxAdjust].Location as LocationCurve;
                XYZ Tan = (locationCurve.Curve as Line).Direction * Math.Abs(heightOffset) * Math.Tan((90 - angle) * Math.PI / 180);
                XYZ ptStart = pts[0];
                XYZ ptEnd = null;
                if (IdxAdjust == 0)
                {
                    locationCurve.Curve = Line.CreateBound(locationCurve.Curve.GetEndPoint(0), locationCurve.Curve.GetEndPoint(1) - Tan).CreateTransformed(translation);
                    ptEnd = locationCurve.Curve.GetEndPoint(1);
                }
                else
                {
                    locationCurve.Curve = Line.CreateBound(locationCurve.Curve.GetEndPoint(0) + Tan, locationCurve.Curve.GetEndPoint(1)).CreateTransformed(translation);
                    ptEnd = locationCurve.Curve.GetEndPoint(0);
                }

                ptStart = pts[0] + (ptEnd - ptStart) / 4;
                ptEnd = pts[0] + (ptEnd - ptStart) * 3 / 4;
                MEPCurve newMep = MEPFactory.CopyTo(doc, mep, ptStart, ptEnd);
                foreach (Connector Conn in newMep.ConnectorManager.Connectors)
                {
                    Conn.ConnectNearConnector(doc, meps);
                }
                return new MEPCurve[] { meps[IdxAdjust], newMep, meps[1 - IdxAdjust] };
            }
            return null;
        }

        private static bool Valid(MEPCurve mep, Direction direction, double offset, double angle)
        {
            Curve curve = ((LocationCurve)mep.Location).Curve;
            if (!(curve is Line))
                throw new InvalidOperationException("暂不支持翻弯弯曲管线");

            // 判断起翻高度是否合理
            //double dim = MEPFactory.GetDimension(mep, direction);
            //if (Math.Abs(offset / Math.Cos((90 - angle) * Math.PI / 180)) < dim) throw new InvalidOperationException("起翻高度过低");

            return true;
        }

        public static Transform GetTransform(XYZ mepDirection, Direction direction, double offset)
        {
            Transform translation = Transform.Identity;
            switch (direction)
            {
                case Direction.Up:
                    translation = Transform.CreateTranslation(new XYZ(0, 0, offset));
                    break;
                case Direction.Down:
                    translation = Transform.CreateTranslation(new XYZ(0, 0, -offset));
                    break;
                case Direction.Left:
                    XYZ left = new XYZ(-mepDirection.Y, mepDirection.X, 0);
                    translation = Transform.CreateTranslation(left * offset);
                    break;
                case Direction.Right:
                    XYZ right = new XYZ(mepDirection.Y, -mepDirection.X, 0);
                    translation = Transform.CreateTranslation(right * offset);
                    break;
            }
            return translation;
        }
    }
}
