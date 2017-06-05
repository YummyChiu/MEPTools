using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace MEPTools.SuperLink
{
    class TeeFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is FamilyInstance)
            {
                MechanicalFitting fitting = ((FamilyInstance)elem).MEPModel as MechanicalFitting;
                if (fitting != null)
                {
                    return fitting.PartType == PartType.Tee;
                }
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
