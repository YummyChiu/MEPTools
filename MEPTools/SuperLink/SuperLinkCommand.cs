using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MEPTools.Util;
using Autodesk.Revit.UI.Selection;
using MEPTools.Bend;
using MEPTools.Link;

namespace MEPTools.SuperLink
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SuperLinkCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            SuperLinkForm form = new SuperLinkForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    if (form.Partial)
                    {
                        FamilyInstance tee = PickTee(uiDoc, "请选择三通");
                        MEPCurve bendMep = null;
                        XYZ bendPt = MEPUtil.PickPointOnMEPCurve(uiDoc, "点选第一点开始起翻", out bendMep);
                        Connector[] connectors = GetConnectors(tee, bendMep);
                        BendUtil.Direction direction = Math.Abs(connectors[0].Origin.Z - connectors[1].Origin.Z) < 0.1 ? BendUtil.Direction.Up : BendUtil.Direction.Left;
                        using (Transaction trans = new Transaction(doc, "SuperLink"))
                        {
                            trans.Start();
                            MEPUtil.Delete(doc, tee);
                            MEPCurve[] meps = BendUtil.BendOneSide(doc, new XYZ[] { bendPt, connectors[2].Origin }, bendMep, direction, form.Offset / 304.8, form.Angle);
                            XYZ[] projects = LinkUtil.Project(meps[0].ToLine(), ((MEPCurve)connectors[0].Owner).ToLine());
                            // 创建中间管线
                            XYZ[] endPoints = LinkUtil.GetMiddleEndPoints(projects, meps[0].ToLine(), form.Angle);
                            MEPCurve newMep = Util.MEPFactory.CopyTo(doc, meps[0], endPoints[0], endPoints[1]);
                            // 创建连接管件
                            CreatFitting(doc, connectors, newMep, meps[0]);
                            trans.Commit();
                        }
                    }
                    else
                    {
                        FamilyInstance tee = PickTee(uiDoc, "请选择三通");
                        MEPCurve liftMep = MEPUtil.PickMEPCurve(uiDoc, "请选择提拉管线");
                        Connector[] connectors = GetConnectors(tee, liftMep);
                        BendUtil.Direction direction = Math.Abs(connectors[0].Origin.Z - connectors[1].Origin.Z) < 0.1 ? BendUtil.Direction.Up : BendUtil.Direction.Left;
                        Transform transform = BendUtil.GetTransform(liftMep.ToLine().Direction, direction, form.Offset / 304.8);
                        using (Transaction trans = new Transaction(doc, "SuperLink"))
                        {
                            trans.Start();
                            MEPUtil.Delete(doc, tee);
                            // 偏移连接管线
                            ElementTransformUtils.MoveElement(doc, liftMep.Id, transform.Origin);
                            XYZ[] projects = LinkUtil.Project(liftMep.ToLine(), ((MEPCurve)connectors[0].Owner).ToLine());
                            // 创建中间管线
                            XYZ[] endPoints = LinkUtil.GetMiddleEndPoints(projects, liftMep.ToLine(), form.Angle);
                            MEPCurve newMep = Util.MEPFactory.CopyTo(doc, liftMep, endPoints[0], endPoints[1]);
                            // 创建连接管件
                            CreatFitting(doc, connectors, newMep, liftMep);
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

        private void CreatFitting(Document doc, Connector[] connectors, MEPCurve newMep, MEPCurve toLink)
        {
            newMep.GetConnectorInPoint(newMep.ToLine().GetEndPoint(1)).ConnectNearConnector(doc, toLink);
            doc.Create.NewTeeFitting(connectors[0], connectors[1], newMep.GetConnectorInPoint(newMep.ToLine().GetEndPoint(0)));
        }

        private Connector[] GetConnectors(FamilyInstance tee, MEPCurve bendMep)
        {
            Connector[] result = new Connector[3];
            int idx = 0;
            foreach (Connector conn in tee.MEPModel.ConnectorManager.Connectors)
            {
                ConnectorSetIterator iterator = conn.AllRefs.ForwardIterator();
                if (iterator.MoveNext())
                {
                    if (idx < 2 && ((Connector)iterator.Current).Owner.Id != bendMep.Id)
                    {
                        result[idx++] = (Connector)iterator.Current;
                    }
                    else
                    {
                        result[2] = (Connector)iterator.Current;
                    }
                }
            }
            if (idx < 1) throw new InvalidOperationException("所选三通元件存在连接错误");
            return result;
        }

        public FamilyInstance PickTee(UIDocument uiDoc, string prompt)
        {
            Reference refer = uiDoc.Selection.PickObject(ObjectType.Element, new TeeFilter(), prompt);
            return uiDoc.Document.GetElement(refer) as FamilyInstance;
        }
    }
}
