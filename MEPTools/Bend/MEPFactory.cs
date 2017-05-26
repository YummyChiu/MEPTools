using Autodesk.Revit.DB;
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
        public static T Creat<T>(Document doc, ElementId systemTypeId, ElementId mepTypeId, ElementId levelId, XYZ startPoint, XYZ endPoint)
            where T : MEPCurve
        {
            if (typeof(T).Equals(typeof(Pipe)))
            {
                return (T)PipeBend.Single.Create(doc, systemTypeId, mepTypeId, levelId, startPoint, endPoint);
            }
            else if (typeof(T).Equals(typeof(Duct)))
            {
                return (T)DuctBend.Single.Create(doc, systemTypeId, mepTypeId, levelId, startPoint, endPoint);
            }
            return default(T);
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
            return default(double);
        }

        public static ElementId GetMEPTypeId(MEPCurve mep)
        {
            if (mep is Pipe)
            {
                return PipeBend.Single.GetMEPTypeId(mep);
            }
            else if (mep is Duct)
            {
                return DuctBend.Single.GetMEPTypeId(mep);
            }
            return ElementId.InvalidElementId;
        }
    }
}
