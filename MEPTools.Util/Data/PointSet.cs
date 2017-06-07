using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Util.Data
{
    public class PointSet
    {
        XYZ[] pointSet;
        List<MEPCurve> mepCurveSet;
        List<int> endIdxs;
        int idx;

        public PointSet(int capacity)
        {
            pointSet = new XYZ[capacity];
            idx = 0;
            mepCurveSet = new List<MEPCurve>();
            endIdxs = new List<int>();
        }

        public bool IsOnSameCurve
        {
            get { return mepCurveSet.Count == 1; }
        }

        public void AddMepCurve(MEPCurve mep)
        {
            mepCurveSet.Add(mep);
            if (mepCurveSet.Count > 1)
            {
                endIdxs.Add(idx);
            }
        }

        public void AddPoint(XYZ pt)
        {
            if (idx < pointSet.Length)
            {
                pointSet[idx++] = pt;
                if (idx >= pointSet.Length)
                {
                    endIdxs.Add(idx);
                }
            }
        }

        public List<MEPCurve> MepCurveSet
        {
            get { return mepCurveSet; }
        }

        public XYZ[] GetPoints(MEPCurve mep)
        {
            int index = mepCurveSet.IndexOf(mep);
            int start = index > 0 ? endIdxs[index - 1] : 0;
            int end = endIdxs[index];
            XYZ[] result = new XYZ[end - start];
            int step = 0;
            while (start + step < end)
            {
                result[step] = pointSet[start + step];
                step++;
            }
            return result;
        }
    }
}
