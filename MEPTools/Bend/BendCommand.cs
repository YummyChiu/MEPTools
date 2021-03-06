﻿using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using MEPTools.Util;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace MEPTools.Bend
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class BendCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            BendForm form = new BendForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    if (form.IsOneSideBend)
                    {
                        MEPCurve mep = null;
                        XYZ[] pts = MEPUtil.PickTwoPointOnMEPCurve(uiDoc, new string[] { "点选第一点开始起翻", "第二点为起翻的方向" }, out mep);

                        using (Transaction trans = new Transaction(doc, "MEP Bend"))
                        {
                            trans.Start();
                            BendOneSide(doc, pts, mep, form.Direction, form.Offset / 304.8, form.Angle);
                            trans.Commit();
                        }
                    }
                    else
                    {
                        MEPCurve mep = null;
                        XYZ[] pts = MEPUtil.PickTwoPointOnMEPCurve(uiDoc, new string[] { "点选第一点开始起翻", "点选第二点结束起翻" }, out mep);

                        using (Transaction trans = new Transaction(doc, "MEP Bend"))
                        {
                            trans.Start();
                            BendTwoSide(doc, pts, mep, form.Direction, form.Offset / 304.8, form.Angle);
                            trans.Commit();
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TaskDialog.Show("Revit", ex.Message);
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

        private void BendTwoSide(Document doc, XYZ[] pts, MEPCurve mep, Direction direction, double heightOffset, double angle)
        {
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

        public enum Direction { Up, Down, Left, Right };
        private void BendOneSide(Document doc, XYZ[] pts, MEPCurve mep, Direction direction, double heightOffset, double angle)
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
                XYZ Tan = (locationCurve.Curve as Line).Direction * heightOffset * Math.Tan((90 - angle) * Math.PI / 180);
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
            }
        }

        private bool Valid(MEPCurve mep, Direction direction, double offset, double angle)
        {
            Curve curve = ((LocationCurve)mep.Location).Curve;
            if (!(curve is Line))
                throw new InvalidOperationException("暂不支持翻弯弯曲管线");

            // 判断起翻高度是否合理
            double dim = MEPFactory.GetDimension(mep, direction);
            if (Math.Abs(offset / Math.Cos((90 - angle) * Math.PI / 180)) < dim) throw new InvalidOperationException("起翻高度过低");

            return true;
        }

        private Transform GetTransform(XYZ mepDirection, Direction direction, double offset)
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
