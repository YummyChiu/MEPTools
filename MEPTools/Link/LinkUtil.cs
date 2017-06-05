using Autodesk.Revit.DB;
using MEPTools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Link
{
    static class LinkUtil
    {
        public static XYZ[] GetMiddleEndPoints(XYZ[] projects, Line toLink, double angle)
        {
            XYZ vector = toLink.Direction;
            if (toLink.GetEndPoint(0).DistanceTo(projects[1]) > toLink.GetEndPoint(1).DistanceTo(projects[1]))
            {
                vector = -vector;
            }
            double symbol = (projects[1] - projects[0]).Z < 0 ? -1 : 1;
            double length = projects[0].DistanceTo(projects[1]) * symbol;
            XYZ ptEnd = projects[1] + vector * Math.Abs(length) * Math.Sin((90 - angle) * Math.PI / 180);
            return new XYZ[] {
                projects[0] + (ptEnd - projects[0]) / 4,
                projects[0] + (ptEnd - projects[0]) * 3 / 4
            };
        }

        public static XYZ[] Project(Line ToLink, Line BeLinked)
        {
            XYZ project1 = null, project2 = null;
            Line tmpBeLinked = Line.CreateBound(BeLinked.GetEndPoint(0) - BeLinked.Direction * 10, BeLinked.GetEndPoint(1) + BeLinked.Direction * 10);
            double min = double.MaxValue;
            for (int i = 0; i < 2; i++)
            {
                XYZ pt = ToLink.GetEndPoint(i);
                IntersectionResult result1 = tmpBeLinked.Project(pt);
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
            Line tmpToLink = Line.CreateBound(ToLink.GetEndPoint(0) - ToLink.Direction * 10, ToLink.GetEndPoint(1) + ToLink.Direction * 10);
            IntersectionResult result2 = tmpToLink.Project(project1);
            project2 = result2.XYZPoint;
            return new XYZ[] { project1, project2 };
        }
    }
}
