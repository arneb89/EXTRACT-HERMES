using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Filters
    {
        public static double[] UniformFilter(double[] ff, int wing)
        {
            double[] filtered = new double[ff.Length];
            int xx1, xx2, nn;
            double ave;
            for (int i = 0; i < ff.Length; i++)
            {
                ave = 0;
                xx1 = i - wing;
                xx2 = i + wing;
                nn = 2 * wing + 1;
                if (xx1 < 0)
                {
                    nn = nn - Math.Abs(xx1);
                    xx1 = 0;
                }
                if (xx2 > ff.Length - 1)
                {
                    nn = nn - Math.Abs(ff.Length - 1 - xx2);
                    xx2 = ff.Length - 1;
                }
                for (int j = xx1; j <= xx2; j++)
                {
                    ave += ff[j];
                }
                ave = ave / nn;
                filtered[i] = ave;
            }
            return filtered;
        }
    }
}
