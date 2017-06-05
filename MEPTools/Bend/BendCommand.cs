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
                            BendUtil.BendOneSide(doc, pts, mep, form.Direction, form.Offset / 304.8, form.Angle);
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
                            BendUtil.BendTwoSide(doc, pts, mep, form.Direction, form.Offset / 304.8, form.Angle);
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
    }
}
