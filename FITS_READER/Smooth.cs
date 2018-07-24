using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Smooth
    {
        public static void DoSmooth(ref Image im)
        {
            double[] row;
            for (int j = 0; j < im.NAXIS1; j++)
            {
                row = im.GetRow(j);
                int[] xb = null;
                int nlim = 0;
                clean(ref row);
                //slines(row, ref xb, ref nlim);
                //int l1, l2;
                //for (int l = 0; l <= nlim; l = l + 2)
                //{
                //    l1 = xb[l];
                //    l2 = xb[l + 1];
                //    for (int k = 0; k < 5; k++)
                //    {
                //        for(int i=l1+3; i<=l2-3; i++)
                //        {
                //            row[i] = (7.0 * row[i] + 6.0 * (row[i - 1] + row[i + 1]) + 
                //                3.0 * (row[i - 2] + row[i + 2]) - 2.0 * (row[i - 3] + row[i + 3])) / 21.0;
                //        }
                //    }
                //}
                for (int i = 0; i < im.NAXIS2; i++)
                {
                    im[j, i] = row[i];
                }
            }
        }

        private static void clean(ref double[] r)
        {
            double a, b, ri, sig;
            int ier = 1;
            while (ier != 0)
            {
                double nc = 0;
                int i1, i2;
                for (int i = 3; i <= r.Length - 3; i++)
                {
                    i1 = i - 1;
                    i2 = i + 1;
                    if (r[i] > r[i1] && r[i] > r[i2])
                    {
                        sig = Math.Sqrt(Math.Abs(r[i]));
                        b = (r[i2] - r[i1]) / (i2 - i1);
                        a = r[i1] - b * i1;
                        ri = a + b * i;
                        if (r[i] - ri > 3 * sig)
                        {
                            nc++;
                            b=(r[i2+1]-r[i1-1])/((i2+1)-(i1-1));
	                        a=r[i1-1]-b*(i1-1);
                            for (int l = i1 - 1; l <= i2 + 1; l++)
                            {
                                ri = a + b * l;
                                r[l] = ri;
                            }
                        }
                    }
                }
                if (nc == 0) ier = 0;
            }
        }

        private static void slines(double[] r, ref int[] xb, ref int nlim)
        {
            int nleft, nrigth;
            xb = new int[r.Length * 2];
            nlim = 0;
            for (int i = 1; i < r.Length - 1; i++)
            {
                nleft = i - 1;
                while (nleft > 0 && r[nleft + 1] < r[nleft])
                {
                    nleft--;
                    if (nleft == 0) break; //!
                    if (i - nleft > 10) break;
                }
                nrigth = i + 1;
                while (nrigth < r.Length-1 && r[nrigth - 1] < r[nrigth])
                {
                    nrigth++;
                    if (nrigth == r.Length-1) break;
                    if (nrigth - i > 10) break;
                }
                double sig = 5 * Math.Sqrt(Math.Abs(r[i]));
                double a = (r[nrigth] - r[nleft]) / (nrigth - nleft);
                double b = -a * nleft + r[nleft];
                double ri = a * i + b;
                if (ri - r[i] > sig)
                {
                    nlim++;
                    xb[nlim] = nleft;
                    nlim++;
                    xb[nlim] = nrigth;
                }
            }
            nlim++;
            xb[nlim] = r.Length - 1;
            Array.Resize(ref xb, nlim+1);
        }
    }
}
