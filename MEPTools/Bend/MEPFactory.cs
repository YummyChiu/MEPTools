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
    static class MEPFactory
    {
        public static MEPCurve CopyTo(Document doc, MEPCurve mep, XYZ startPoint, XYZ endPoint)
        {
            if (mep is Pipe)
            {
                return PipeBend.Single.CopyTo(doc, mep, startPoint, endPoint);
            }
            else if (mep is Duct)
            {
                return DuctBend.Single.CopyTo(doc, mep, startPoint, endPoint);
            }
            else if (mep is CableTray)
            {
                return CableTrayBend.Single.CopyTo(doc, mep, startPoint, endPoint);
            }
            return default(MEPCurve);
        }

        public static double GetDimension(MEPCurve mep, BendCommand.Direction direction)
        {
            if (mep is Pipe)
            {
                return PipeBend.Single.GetDimension(mep, direction);
            }
            else if (mep is Duct)
            {
                return DuctBend.Single.GetDimension(mep, direction);
            }
            else if (mep is CableTray)
            {
                return CableTrayBend.Single.GetDimension(mep, direction);
            }
            return default(double);
        }

        //public static ElementId GetMEPTypeId(MEPCurve mep)
        //{
        //    if (mep is Pipe)
        //    {
        //        return PipeBend.Single.GetMEPTypeId(mep);
        //    }
        //    else if (mep is Duct)
        //    {
        //        return DuctBend.Single.GetMEPTypeId(mep);
        //    }
        //    else if (mep is CableTray)
        //    {
        //        return CableTrayBend.Single.GetMEPTypeId(mep);
        //    }
        //    return ElementId.InvalidElementId;
        //}

        //private static ElementId[] GetParameters(MEPCurve mepCurve)
        //{
        //    ElementId[] result = new ElementId[3];
        //    result[0] = mepCurve.LookupParameter("系统类型").AsElementId();
        //    result[1] = MEPFactory.GetMEPTypeId(mepCurve);
        //    result[2] = mepCurve.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId();

        //    if (result[1] == null)
        //    {
        //        throw new ArgumentException("不支持管线类型", "MEPCurve mepCurve");
        //    }
        //    return result;
        //}
    }
}
