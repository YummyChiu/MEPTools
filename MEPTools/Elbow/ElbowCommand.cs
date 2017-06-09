using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPTools.Util;
using MEPTools.Bend;

namespace MEPTools.Elbow
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ElbowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            ElbowForm form = new ElbowForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    using (Transaction trans = new Transaction(doc, "排水倒角"))
                    {
                        trans.Start();
                        FamilyInstance elbow = PickElbow(uiDoc, "请选择弯头");
                        Connector[] connectors = GetConnectorsBeside(elbow);
                        MEPUtil.Delete(doc, elbow);

                        // 管道向内缩的长度的值为x,界面指定的offset为D，两个connector的距离为d，那么
                        // √2x + d = D,具体参见45°等腰梯形

                        double offset = (form.Offset / 304.8 - connectors[0].Origin.DistanceTo(connectors[1].Origin)) / (2 * Math.Cos(Math.PI / 4));
                        MEPCurve mep = connectors[0].Owner as MEPCurve;
                        (mep.Location as LocationCurve).Curve = mep.ToLine().GetEndPoint(0).IsAlmostEqualTo(connectors[0].Origin) ?
                            Line.CreateBound(mep.ToLine().GetEndPoint(0) + mep.ToLine().Direction * offset, mep.ToLine().GetEndPoint(1)) :
                            Line.CreateBound(mep.ToLine().GetEndPoint(0), mep.ToLine().GetEndPoint(1) - mep.ToLine().Direction * offset);

                        mep = connectors[1].Owner as MEPCurve;
                        (mep.Location as LocationCurve).Curve = mep.ToLine().GetEndPoint(0).IsAlmostEqualTo(connectors[1].Origin) ?
                            Line.CreateBound(mep.ToLine().GetEndPoint(0) + mep.ToLine().Direction * offset, mep.ToLine().GetEndPoint(1)) :
                            Line.CreateBound(mep.ToLine().GetEndPoint(0), mep.ToLine().GetEndPoint(1) - mep.ToLine().Direction * offset);

                        MEPCurve newMep = Bend.MEPFactory.CopyTo(doc, mep, connectors[0].Origin + (connectors[1].Origin - connectors[0].Origin) / 4, connectors[0].Origin + (connectors[1].Origin - connectors[0].Origin) * 3 / 4);
                        if (!connectors[0].IsValidObject || !connectors[1].IsValidObject)
                            trans.RollBack();
                        else
                        {
                            connectors[0].ConnectNearConnector(doc, newMep);
                            connectors[1].ConnectNearConnector(doc, newMep);
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

        /// <summary>
        /// 得到弯头的两个connector(AllRefs's owner可能不是pipe,有可能是pipe system，具体判断请转至 BendUtil.ConnectorSetIteratorSearch<T>(iterator)) 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        private Connector[] GetConnectorsBeside(FamilyInstance familyInstance)
        {
            Connector[] result = new Connector[2];
            int idx = 0;
            foreach (Connector conn in familyInstance.MEPModel.ConnectorManager.Connectors)
            {
                ConnectorSetIterator iterator = conn.AllRefs.ForwardIterator();
                result[idx++] = BendUtil.ConnectorSetIteratorSearch<MEPCurve>(iterator);
            }
            return result;
        }

        public FamilyInstance PickElbow(UIDocument uiDoc, string prompt)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, new ElbowFilter(), prompt);
            return uiDoc.Document.GetElement(refer) as FamilyInstance;
        }
    }
}
