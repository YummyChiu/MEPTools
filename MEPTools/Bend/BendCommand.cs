using Autodesk.Revit.UI;
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
    class BendCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            MEPCurve mep = MEPUtil.PickMEPCurve(uiDoc, "请选择管道");
            XYZ[] pts = new XYZ[2];
            pts[0] = MEPUtil.PickPointOnMEPCurve(uiDoc, mep, "点选第一点开始起翻");
            pts[1] = uiDoc.Selection.PickPoint(ObjectSnapTypes.Nearest, "第二点为起翻的方向");

            using (Transaction trans = new Transaction(doc, "MEP Bend"))
            {
                trans.Start();
                BendOneSide(doc, pts, mep, Direction.Left, 600 / 304.8);
                trans.Commit();
            }
            return Result.Succeeded;
        }

        internal enum Direction { Up, Down, Left, Right };
        private void BendOneSide(Document doc, XYZ[] pts, MEPCurve mep, Direction direction, double heightOffset)
        {
            Curve curve = ((LocationCurve)mep.Location).Curve;
            if (!(curve is Line))
                throw new InvalidOperationException("暂不支持翻弯弯曲管线");

            // 判断起翻高度是否合理
            double dim = MEPFactory.GetDimension(mep, direction);
            if (heightOffset < 1.5 * dim) throw new InvalidOperationException("起翻高度过低");

            XYZ bendDirection = (pts[1] - pts[0]).Normalize();
            XYZ mepDirection = ((Line)curve).Direction;
            MEPCurve[] meps = MEPUtil.SliceMEPCurveIntoTwo(doc, mep, pts[0]);
            int IdxAdjust = mepDirection.AngleTo(bendDirection) <= Math.PI * 0.5 ? 1 : 0;

            Transform translation = Transform.Identity;
            switch (direction)
            {
                case Direction.Up:
                    translation = Transform.CreateTranslation(new XYZ(0, 0, heightOffset));
                    break;
                case Direction.Down:
                    translation = Transform.CreateTranslation(new XYZ(0, 0, -heightOffset));
                    break;
                case Direction.Left:
                    XYZ left = new XYZ(-mepDirection.Y, mepDirection.X, 0);
                    translation = Transform.CreateTranslation(left * heightOffset);
                    break;
                case Direction.Right:
                    XYZ right = new XYZ(mepDirection.Y, -mepDirection.X, 0);
                    translation = Transform.CreateTranslation(right * heightOffset);
                    break;
            }

            LocationCurve locationCurve = meps[IdxAdjust].Location as LocationCurve;
            locationCurve.Curve = locationCurve.Curve.CreateTransformed(translation);
        }

        private ElementId[] GetParameters(MEPCurve mepCurve)
        {
            ElementId[] result = new ElementId[3];
            result[0] = mepCurve.MEPSystem.Id;
            result[1] = MEPFactory.GetMEPTypeId(mepCurve);
            result[2] = mepCurve.LevelId;

            if (result[1] == null)
            {
                throw new ArgumentException("不支持管线类型", "MEPCurve mepCurve");
            }
            return result;
        }
    }
}
