using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Bend
{
    abstract class IBend
    {
        public virtual MEPCurve CopyTo(Document doc, MEPCurve mep, XYZ startPoint, XYZ endPoint)
        {
            MEPCurve newMEP = doc.GetElement(ElementTransformUtils.CopyElement(doc, mep.Id, XYZ.Zero).ElementAt(0)) as MEPCurve;
            LocationCurve newLocationCurve = newMEP.Location as LocationCurve;
            newLocationCurve.Curve = Line.CreateBound(startPoint, endPoint);       
            return newMEP;
        }
        public abstract double GetDimension(MEPCurve mep, BendUtil.Direction direction);
        //ElementId GetMEPTypeId(MEPCurve mep);
    }

    class PipeBend : IBend
    {
        public static PipeBend Single = new PipeBend();

        private PipeBend() { }

        public override double GetDimension(MEPCurve mep, BendUtil.Direction direction)
        {
            Parameter dim = mep.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
            return dim.AsDouble() * 1.5;
        }

        //public ElementId GetMEPTypeId(MEPCurve mep)
        //{
        //    return ((Pipe)mep).PipeType.Id;
        //}
    }

    class DuctBend : IBend
    {
        public static DuctBend Single = new DuctBend();

        private DuctBend() { }

        public override double GetDimension(MEPCurve mep, BendUtil.Direction direction)
        {
            Parameter dim = null;
            switch (direction)
            {
                case BendUtil.Direction.Up:
                case BendUtil.Direction.Down:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
                    break;
                case BendUtil.Direction.Left:
                case BendUtil.Direction.Right:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                    break;
            }
            return dim.AsDouble() * 2;
        }

        //public ElementId GetMEPTypeId(MEPCurve mep)
        //{
        //    return ((Duct)mep).DuctType.Id;
        //}
    }

    class CableTrayBend : IBend
    {
        public static CableTrayBend Single = new CableTrayBend();

        private CableTrayBend() { }

        public override double GetDimension(MEPCurve mep, BendUtil.Direction direction)
        {
            Parameter dim = null;
            switch (direction)
            {
                case BendUtil.Direction.Up:
                case BendUtil.Direction.Down:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM);
                    break;
                case BendUtil.Direction.Left:
                case BendUtil.Direction.Right:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM);
                    break;
            }
            return dim.AsDouble() * 2;
        }

        //public ElementId GetMEPTypeId(MEPCurve mep)
        //{
        //    return mep.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsElementId();
        //}
    }
}
