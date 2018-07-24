using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class Locator
    {
        private static double[][] pos_ord = null;

        private static double[][] pos_min = null;

        public static void Locate(ref Image im, int polynom_degree)
        {
            double[] order_centers = new double[2];
            int wing = 2;
            int middle = im.NAXIS2 / 2;
            Image slice = new Image(im.NAXIS1, 1 + 2 * wing);

            for (int i = 0; i < slice.NAXIS1; i++)
            {
                for (int j = 0; j < slice.NAXIS2; j++)
                {
                    slice[i, j] = im[i, middle - wing + j];
                }
            }

            double[] aver_column = new double[im.NAXIS1];

            for (int i = 0; i < aver_column.Length; i++)
                aver_column[i] = Statistics.Median(slice.GetRow(i));

            StreamWriter sw = new StreamWriter((string)Init.Value("DIR_MAIN") + "\\Slice.txt");
            for (int i = 0; i < aver_column.Length - 1; i++)
                sw.WriteLine("{0}\t{1}", i, aver_column[i].ToString());
            sw.Close();

            // Search for orders locations in cetral slit;

            double[] xmax_0 = new double[5000];
            int orders_count = 0;

            int dist = 5; // for ZTSh
            EcheData.LoadOrdMidPos("OrdPos.dat", (int)Init.Value("TRIM_COL1"));

            double[] xmax = new double[EcheData.ord_mid_pos.Length];
            for (int i = 0; i < xmax.Length; i++)
            {
                xmax[i] = (double)EcheData.ord_mid_pos[i];
            }

            for (int i = 0; i < xmax.Length; i++)
            {
                int x1 = (int)xmax[i] - dist;
                int x2 = (int)xmax[i] + dist;
                if (x1 < 0) x1 = 0;
                if (x2 > aver_column.Length - 1) x2 = aver_column.Length - 1;
                int xm = x1;
                double max = aver_column[x1];
                for (int j = x1 + 1; j <= x2; j++)
                {
                    if (aver_column[j] > max)
                    {
                        max = aver_column[j];
                        xm = j;
                    }
                }
                xmax[i] = xm;
            }

            orders_count = xmax.Length;

            // Orders Tracing;

            pos_ord = new double[orders_count][];
            for (int i = 0; i < pos_ord.Length; i++) pos_ord[i] = new double[im.NAXIS2];

            for (int i = 0; i < pos_ord.Length; i++) pos_ord[i][middle] = xmax[i];

            int nwin = 8;
            int o1 = 0, o2 = 0;
            for (int i = middle - 1; i >= 0; i--)
            {
                for (int n = o1; n < orders_count + o2; n++)
                {
                    int n1 = (int)pos_ord[n][i + 1] - nwin;
                    int n2 = (int)pos_ord[n][i + 1] + nwin;
                    double max = 0;
                    for (int k = n1; k <= n2; k++)
                    {
                        if (im[k, i] > max)
                        {
                            pos_ord[n][i] = k;
                            max = im[k, i];
                        }
                    }
                }
            }

            for (int i = middle + 1; i < im.NAXIS2; i++)
            {
                for (int n = o1; n < orders_count + o2; n++)
                {
                    int n1 = (int)pos_ord[n][i - 1] - nwin;
                    int n2 = (int)pos_ord[n][i - 1] + nwin;
                    double max = 0;
                    for (int k = n1; k <= n2; k++)
                    {
                        if (im[k, i] > max)
                        {
                            pos_ord[n][i] = k;
                            max = im[k, i];
                        }
                    }
                }
            }

            pos_min = new double[pos_ord.Length + 1][];
            for (int i = 0; i < pos_min.Length; i++) pos_min[i] = new double[im.NAXIS2];

            for (int i = 0; i < im.NAXIS2; i++)
            {
                for (int n = 1; n < pos_min.Length - 1; n++)
                {
                    int n1 = (int)pos_ord[n - 1][i];
                    int n2 = (int)pos_ord[n][i];
                    double min = double.MaxValue;
                    for (int k = n1; k <= n2; k++)
                    {
                        if (im[k, i] < min)
                        {
                            pos_min[n][i] = k;
                            min = im[k, i];
                        }
                    }
                }
            }


            for (int i = 0; i < im.NAXIS2; i++)
            {
                pos_min[0][i] = pos_min[1][i] - (pos_min[2][i] - pos_min[1][i]);
                if (pos_min[0][i] < 0) pos_min[0][i] = 0;
            }


            for (int i = 0; i < im.NAXIS2; i++)
            {
                pos_min[pos_min.Length - 1][i] = pos_min[pos_min.Length - 2][i] +
                    (pos_min[pos_min.Length - 2][i] - pos_min[pos_min.Length - 3][i]);
                if (pos_min[pos_min.Length - 1][i] > aver_column.Length - 1)
                    pos_min[pos_min.Length - 1][i] = aver_column.Length - 1;
            }


            Saver.SaveOrderedDistribution(pos_min, (string)Init.Value("DIR_MAIN") + "\\trace_min.txt");

            Saver.SaveColumn(xmax, (string)Init.Value("DIR_MAIN") + "\\orders_ident.dat");

            double[] pixels_x = new double[im.NAXIS2];
            for (int i = 0; i < pixels_x.Length; i++) pixels_x[i] = i;

            GravImprove(im, ref pos_min, ref pos_ord);
            Saver.SaveOrderedDistribution(pos_ord, (string)Init.Value("DIR_MAIN") + "\\trace.txt");

            for (int i = 0; i < pos_ord.Length; i++)
            {
                ImproveTraces(pixels_x, ref pos_ord[i], polynom_degree);
            }

            for (int i = 0; i < pos_min.Length; i++)
            {
                ImproveTraces(pixels_x, ref pos_min[i], polynom_degree);
            }

            Saver.SaveOrderedDistribution(pos_ord, (string)Init.Value("DIR_MAIN") + "\\orders_impoved.dat");
            Saver.SaveOrderedDistribution(pos_min, (string)Init.Value("DIR_MAIN") + "\\mins_improved.dat");

            for (int i = 0; i < xmax.Length; i++)
            {
                xmax[i] = pos_ord[i][middle];
            }

            Saver.SaveColumn(xmax, (string)Init.Value("DIR_MAIN") + "\\Orders.txt");
        }

        private static void GravImprove(Image im, ref double[][] pos_min, ref double[][] pos_max)
        {
            for (int i = 0; i < pos_max.Length; i++)
            {
                for (int j = 0; j < pos_max[i].Length; j++)
                {
                    double sum_yx = 0;
                    double sum_x = 0;
                    int k1 = (int)Math.Round(pos_min[i][j], 0);
                    int k2 = (int)Math.Round(pos_min[i + 1][j], 0);
                    for (int k = k1; k <= k2; k++)
                    {
                        sum_x += im[k, j];
                        sum_yx += k * im[k, j];
                    }
                    pos_max[i][j] = sum_yx / sum_x;
                }
            }
        }

        private static void ImproveTraces(double[] pix_x, ref double[] pix_y, int polynom_degree)
        {
            double[] pix_x_1 = new double[pix_x.Length];
            double[] pix_y_1 = new double[pix_y.Length];
            double[] pix_y_fit = new double[pix_y.Length];
            double max_x = pix_x[pix_x.Length - 1];
            double max_y = pix_y[pix_y.Length - 1];
            double[] coeffs;

            for (int i = 0; i < pix_x_1.Length; i++)
            {
                pix_x_1[i] = pix_x[i] / max_x;
            }

            for (int i = 0; i < pix_y_1.Length; i++)
            {
                pix_y_1[i] = pix_y[i] / max_y;
            }

            Fitter fitter = new Fitter();

            coeffs = fitter.Polynom(pix_x_1, pix_y_1, polynom_degree);

            for (int i = 0; i < pix_x_1.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < polynom_degree + 1; j++)
                {
                    sum += coeffs[j] * Math.Pow(pix_x_1[i], j);
                }
                pix_y_fit[i] = sum;
            }

            for (int i = 0; i < pix_x.Length; i++)
            {
                pix_y[i] = max_y * pix_y_fit[i];
            }
        }

        public static double[][] Ord_Pos
        {
            get
            {
                return pos_ord;
            }
        }

        public static double[][] Min_Pos
        {
            get
            {
                return pos_min;
            }
        }
    }
}
