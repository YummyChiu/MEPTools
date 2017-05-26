﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Bend
{
    interface IBend
    {
        MEPCurve CopyTo(Document doc, MEPCurve mep, XYZ startPoint, XYZ endPoint);
        ElementId GetMEPTypeId(MEPCurve mep);
        double GetDimension(MEPCurve mep, BendCommand.Direction direction);
    }

    class PipeBend : IBend
    {
        public static PipeBend Single = new PipeBend();

        private PipeBend() { }

        public MEPCurve CopyTo(Document doc, MEPCurve mep, XYZ startPoint, XYZ endPoint)
        {
            MEPCurve newMEP = doc.GetElement(ElementTransformUtils.CopyElement(doc, mep.Id, XYZ.Zero).ElementAt(0)) as MEPCurve;
            LocationCurve newLocationCurve = newMEP.Location as LocationCurve;
            newLocationCurve.Curve = Line.CreateBound(startPoint, endPoint);
            return newMEP;
        }

        public double GetDimension(MEPCurve mep, BendCommand.Direction direction)
        {
            Parameter dim = mep.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
            return dim.AsDouble() * 1.5;
        }

        public ElementId GetMEPTypeId(MEPCurve mep)
        {
            return ((Pipe)mep).PipeType.Id;
        }
    }

    class DuctBend : IBend
    {
        public static DuctBend Single = new DuctBend();

        private DuctBend() { }

        public MEPCurve CopyTo(Document doc, MEPCurve mep, XYZ startPoint, XYZ endPoint)
        {
            MEPCurve newMEP = doc.GetElement(ElementTransformUtils.CopyElement(doc, mep.Id, XYZ.Zero).ElementAt(0)) as MEPCurve;
            LocationCurve newLocationCurve = newMEP.Location as LocationCurve;
            newLocationCurve.Curve = Line.CreateBound(startPoint, endPoint);
            return newMEP;
        }

        public double GetDimension(MEPCurve mep, BendCommand.Direction direction)
        {
            Parameter dim = null;
            switch (direction)
            {
                case BendCommand.Direction.Up:
                case BendCommand.Direction.Down:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
                    break;
                case BendCommand.Direction.Left:
                case BendCommand.Direction.Right:
                    dim = mep.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                    break;
            }
            return dim.AsDouble() * 2;
        }

        public ElementId GetMEPTypeId(MEPCurve mep)
        {
            return ((Duct)mep).DuctType.Id;
        }
    }

}
