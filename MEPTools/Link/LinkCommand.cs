using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using MEPTools.Util;

namespace MEPTools.Link
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class LinkCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            LinkForm form = new LinkForm();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            while (true)
            {
                try
                {
                    MEPCurve toLinkMep = MEPUtil.PickMEPCurve(uiDoc, "请选择连接管道");
                    MEPCurve beLinkedMep = MEPUtil.PickMEPCurve(uiDoc, "请选择被连接管道");
                    LinkTwo(doc, toLinkMep, beLinkedMep, form.Offset / 304.8);
                }
                catch (InvalidOperationException ex)
                {
                    TaskDialog.Show("Revit", ex.Message);
                }
                catch
                {
                    break;
                }
            }
            return Result.Succeeded;
        }

        private void LinkTwo(Document doc, MEPCurve toLinkMep, MEPCurve beLinkedMep, double heightOffset)
        {
            using (Transaction trans = new Transaction(doc, "MEP Link"))
            {
                trans.Start();
                Line toLink = (toLinkMep.Location as LocationCurve).Curve as Line;
                Line beLinked = (beLinkedMep.Location as LocationCurve).Curve as Line;
                if (toLink == null || beLinked == null)
                {
                    throw new InvalidOperationException("暂不支持连接曲线管线");
                }
                XYZ[] projects = Project(toLink, beLinked);
                // 断开被连接管线
                MEPCurve[] meps = MEPUtil.SliceMEPCurveIntoTwo(doc, beLinkedMep, projects[0], 0.3280839895013123);
                // 创建连接管线
                XYZ vector = toLink.Direction.CrossProduct(beLinked.Direction);
                if (vector.Z < 0)
                    vector = -vector;
                double actualHeight = projects[0].DistanceTo(projects[1]) + heightOffset;
                XYZ ptStart = projects[0] + vector * actualHeight / 4;
                XYZ ptEnd = projects[0] + vector * actualHeight * 3 / 4;
                MEPCurve newMep = MEPFactory.CopyTo(doc, toLinkMep, ptStart, ptEnd);
                // 偏移连接管线
                LocationCurve locationCurve = toLinkMep.Location as LocationCurve;
                locationCurve.Curve = locationCurve.Curve.CreateTransformed(Transform.CreateTranslation(vector * heightOffset));
                // 创建连接管件
                CreatFitting(doc, meps[0], meps[1], newMep, toLinkMep);
                trans.Commit();
            }
        }

        private void CreatFitting(Document doc, params MEPCurve[] meps)
        {
            List<Connector> connectors = new List<Connector>(6);
            for (int i = 0; i < 3; i++)
            {
                foreach (Connector conn in meps[i].ConnectorManager.Connectors)
                {
                    connectors.Add(conn);
                }
            }
            XYZ[] MatchPt = new XYZ[] {
                (meps[0].Location as LocationCurve).Curve.GetEndPoint(1),
                (meps[1].Location as LocationCurve).Curve.GetEndPoint(0),
                (meps[2].Location as LocationCurve).Curve.GetEndPoint(0),
            };
            foreach (Connector conn in meps[2].ConnectorManager.Connectors)
            {
                if (!conn.Origin.IsAlmostEqualTo(MatchPt[2]))
                {
                    conn.ConnectNearConnector(doc, meps[3]);
                }
            }
            for (int i = 0; i < connectors.Count; i++)
            {
                bool Remove = true;
                for (int j = 0; j < 3; j++)
                {
                    if (connectors[i].Origin.IsAlmostEqualTo(MatchPt[j]))
                    {
                        Remove = false;
                        break;
                    }
                }
                if (Remove)
                {
                    connectors.RemoveAt(i);
                    i--;
                }
            }
            doc.Create.NewTeeFitting(connectors[0], connectors[1], connectors[2]);
        }

        XYZ[] Project(Line ToLink, Line BeLinked)
        {
            XYZ project1 = null, project2 = null;
            double min = double.MaxValue;
            for (int i = 0; i < 2; i++)
            {
                XYZ pt = ToLink.GetEndPoint(i);
                IntersectionResult result1 = BeLinked.Project(pt);
                if (result1 != null && result1.XYZPoint != null)
                {
                    if (result1.XYZPoint.DistanceTo(pt) < min)
                    {
                        min = result1.XYZPoint.DistanceTo(pt);
                        project1 = result1.XYZPoint;
                    }
                }
            }
            if (project1 == null) throw new InvalidOperationException("连接管线与被连接管线可能存在选择错误");
            Line tmp = Line.CreateBound(ToLink.GetEndPoint(0) - ToLink.Direction, ToLink.GetEndPoint(1) + ToLink.Direction);
            IntersectionResult result2 = tmp.Project(project1);
            project2 = result2.XYZPoint;
            return new XYZ[] { project1, project2 };
        }
    }
}
