using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class WLCalibration
    {
        public static double[][] Lambdas = null;
        
        /// <summary>
        /// Search for pixel number for the input wavelength
        /// </summary>
        /// <param name="lambda">wavelength</param>
        /// <param name="order">order</param>
        /// <param name="bord1">left bound of searching window</param>
        /// <param name="bord2">right bound of searching window</param>
        /// <param name="error">error of computations</param>
        /// <returns>pixel number</returns>
        public static double FindPix(double lambda, int order, double bord1, double bord2, double error, int x_trim_left)
        {
            double diff, f1, fc, center;
            do
            {
                center = 0.5 * (bord1 + bord2);
                f1 = EcheData.GetWL(order, bord1, x_trim_left) - lambda;
                fc = EcheData.GetWL(order, center, x_trim_left) - lambda;
                if (f1 * fc > 0) bord1 = center;
                else bord2 = center;
                diff = Math.Abs(bord2 - bord1);
            } while (diff > error);
            return (bord2 + bord1) * 0.5;
        }

        private static double GravCenter(double[] x, double[] y)
        {
            double sum1 = 0;
            double sum2 = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sum1 += x[i] * y[i];
                sum2 += y[i];
            }
            return sum1 / sum2;
        }

        private static double GravCenterSpline(double[] x, double[] y)
        {
            double sum1 = 0;
            double sum2 = 0;
            double center;
            Spline31D spline = new Spline31D(x, y);
            int points_num = 100;
            double[] xx = new double[points_num];
            double[] yy = new double[points_num];
            double length = x[x.Length - 1] - x[0];
            double step = length / (points_num - 1);
            for (int i = 0; i < points_num; i++)
            {
                xx[i] = x[0] + i * step;
                yy[i] = spline.Interp(xx[i]);
            }
            for (int i = 0; i < points_num; i++)
            {
                sum1 += yy[i] * xx[i];
                sum2 += yy[i];
            }
            center = sum1 / sum2;
            return center;
        }

        public static void Calibrate(double[][] fluxes, int oo, int ox, double cutLimit, int iterMax, double fluxLimit, int x_trim_left)
        {
            int orders_count = fluxes.Length;
            int pixels_count = fluxes[0].Length;

            double[] point_order = new double[5000];
            double[] point_lambda = new double[5000];
            double[] point_pixel = new double[5000];
            int points_number = 0;

            Console.WriteLine("Repers identification...");

            // repers identification cycle;
            Console.Write("Search in orders: ");
            for (int order = 0; order < orders_count; order++)
            {
                Console.Write("[{0}]", order);
                double wbegin = EcheData.GetWL(order, 0, x_trim_left);
                double wend = EcheData.GetWL(order, pixels_count - 1, x_trim_left);
                double[] cur_fluxes = new double[pixels_count];
                double tmx = 0;
                for (int i = 0; i < pixels_count; i++)
                {
                    cur_fluxes[i] = fluxes[order][i];
                    if (cur_fluxes[i] > tmx) tmx = cur_fluxes[i];
                }
                for (int i = 0; i < pixels_count; i++)
                {
                    cur_fluxes[i] = cur_fluxes[i] / tmx;
                }

                int n_atlas = ThAr.Wls.Length;

                int lines_count = 0;
                double[] lines_wavls = new double[n_atlas];
                for (int i = 0; i < n_atlas; i++)
                {
                    if (wbegin > wend)  // smaller pixels numbers corresponded largest wavelengths
                    {
                        if (ThAr.Wls[i] < wbegin) 
                        {
                            if (ThAr.Wls[i] > wend)
                            {
                                lines_wavls[lines_count] = ThAr.Wls[i];
                                lines_count++;
                            }
                        }
                    }
                    else
                    {
                        if (ThAr.Wls[i] > wbegin)
                        {
                            if (ThAr.Wls[i] < wend)
                            {
                                lines_wavls[lines_count] = ThAr.Wls[i];
                                lines_count++;
                            }
                        }
                    }
                }

                Array.Resize(ref lines_wavls, lines_count);

                int swin = 6;
                int shift = 0;

                for (int i = 0; i < lines_count; i++)
                {
                    double pix = FindPix(lines_wavls[i], order, 0, pixels_count - 1, 0.001, x_trim_left);
                    int k1 = (int)pix - swin + shift;
                    int k2 = (int)pix + swin + shift;
                    if (k1 < 1) k1 = 1;
                    if (k2 > pixels_count - 2) k2 = pixels_count - 2;
                    double max = 0;
                    int j_max = 0;
                    for (int j = k1; j <= k2; j++)
                    {
                        if (cur_fluxes[j] > cur_fluxes[j - 1] && cur_fluxes[j] > cur_fluxes[j + 1])
                        {
                            if (cur_fluxes[j] > max)
                            {
                                max = cur_fluxes[j];
                                j_max = j;
                            }
                        }
                    }
                    if (max*tmx > fluxLimit)
                    {
                        //Saver.SaveColumn(cur_fluxes, "CURR.txt");

                        k1 = j_max - 4;
                        k2 = j_max + 4;
                        if (k1 < 0) k1 = 0;
                        if (k2 > pixels_count - 1) k2 = pixels_count - 1;
                        double[] x = new double[k2 - k1 + 1];
                        double[] y = new double[k2 - k1 + 1];
                        int l = 0;
                        for (int k = k1; k <= k2; k++)
                        {
                            x[l] = (double)k;
                            y[l] = cur_fluxes[k];
                            l++;
                        }
                        //double center = GravCenter(x, y);
                        double center = GravCenterSpline(x, y);

                        point_lambda[points_number] = lines_wavls[i];
                        point_order[points_number] = order;
                        point_pixel[points_number] = center;
                        points_number++;
                    }
                }
            }
            Console.Write("\r\n");
            Console.WriteLine("{0} repers were found.", points_number);

            Array.Resize(ref point_lambda, points_number);
            Array.Resize(ref point_order, points_number);
            Array.Resize(ref point_pixel, points_number);

            for (int i = 0; i < points_number; i++) point_order[i]++;
            for (int i = 0; i < points_number; i++) point_order[i] = point_order[i] / orders_count;
            for (int i = 0; i < points_number; i++) point_pixel[i] = point_pixel[i] / pixels_count;
            for (int i = 0; i < points_number; i++) point_lambda[i] = point_lambda[i] / 7000;

            double[] coeff = null;
            double stderror;
            int rejected_poins_number = 0;
            Console.Write("Iterations: ");
            for (int i = 0; i < iterMax; i++)
            {
                Console.Write("[{0}]", i + 1);
                coeff = Fitting(point_order, point_pixel, point_lambda, oo, ox);
                stderror = 0;
                for (int j = 0; j < points_number; j++)
                {
                    stderror += Math.Pow(point_lambda[j] - Surface(coeff, point_order[j], point_pixel[j], oo, ox), 2);
                }
                stderror = Math.Sqrt(stderror / points_number);
                double diff;
                int k = 0;
                for (int j = 0; j < points_number; j++)
                {
                    diff = Math.Abs(point_lambda[j] - Surface(coeff, point_order[j], point_pixel[j], oo, ox));
                    if (diff < cutLimit * stderror)
                    {
                        point_lambda[k] = point_lambda[j];
                        point_order[k] = point_order[j];
                        point_pixel[k] = point_pixel[j];
                        k++;
                    }
                    else
                    {
                        rejected_poins_number++;
                    }
                }
                points_number = k;
            }
            Console.WriteLine("\r\n{0} points were rejected.", rejected_poins_number);

            double[] diffs = new double[points_number];
            stderror = 0;
            for (int j = 0; j < points_number; j++)
            {
                diffs[j] = point_lambda[j]*7000 - 7000*Surface(coeff, point_order[j], point_pixel[j], oo, ox);
                stderror += diffs[j] * diffs[j];
            }
            stderror = Math.Sqrt(stderror / points_number);
            Console.WriteLine("Std. Error: {0:0.000E00}", stderror);

            System.IO.StreamWriter sw = new System.IO.StreamWriter((string)Init.Value("DIR_MAIN") + "\\found_repers.dat");
            for (int i = 0; i < points_number; i++)
            {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}", point_order[i] * orders_count - 1, point_pixel[i] * pixels_count, point_lambda[i] * 7000, diffs[i]);
            }
            sw.Close();

            Lambdas = new double[orders_count][];
            for (int i = 0; i < orders_count; i++) Lambdas[i] = new double[pixels_count];

            for (int order = 1; order <= orders_count; order++)
            {
                for (int p = 0; p < pixels_count; p++)
                {
                    int k = 0;
                    double sum = 0;
                    for (int i = 0; i <= oo; i++)
                    {
                        for (int j = 0; j <= ox; j++)
                        {
                            sum += coeff[k] * Math.Pow((double)order / orders_count, i) * 
                                Math.Pow((double)p / pixels_count, j);
                            k++;
                        }
                    }
                    Lambdas[order-1][p] = sum * 7000;
                }
            }
        }

        private static double Surface(double[] coeff, double order, double pix, int ox, int oy)
        {
            int k = 0;
            double sum = 0;
            for (int i = 0; i <= ox; i++)
            {
                for (int j = 0; j <= oy; j++)
                {
                    sum += coeff[k] * Math.Pow(order, i) * Math.Pow(pix, j);
                    k++;
                }
            }
            return sum;
        }

        private static double[] Fitting(double[] x, double[] y, double[] f, int ox, int oy)
        {
            int g_col_count;
            int g_row_count;
            g_col_count = (ox + 1) * (oy + 1);
            g_row_count = f.Length;
            double[,] g_matrix = new double[g_row_count, g_col_count];
            double[,] g_matrix_tr = new double[g_col_count, g_row_count];
            double[,] ggtr = new double[g_col_count, g_col_count];
            double[] gf = new double[g_col_count];
            double[] coeff;
            

            for (int p = 0; p < g_row_count; p++)
            {
                int k = 0;
                for (int i = 0; i <= ox; i++)
                {
                    for (int j = 0; j <= oy; j++)
                    {
                        g_matrix[p, k] = Math.Pow(x[p], i) * Math.Pow(y[p], j);
                        k++;
                    }
                }
            }

            for (int i = 0; i < g_col_count; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < g_row_count; k++)
                    {
                        sum += g_matrix[k, i] * g_matrix[k, j];
                    }
                    ggtr[i, j] = sum;
                    ggtr[j, i] = sum;
                }
            }

            for (int i = 0; i < g_col_count; i++)
            {
                double sum = 0;
                for (int j = 0; j < g_row_count; j++)
                {
                    sum += g_matrix[j, i] * f[j];
                }
                gf[i] = sum;
            }

            coeff = SolveWithGaussMethod(ggtr, gf);

            return coeff;
        }

        static public double[] SolveWithGaussMethod(double[,] m, double[] l)
        {
            int N = l.Length;

            double[] x = new double[N];

            // Приведение матрицы m к треугольному виду
            for (int s = 0; s <= N - 2; s++)
            {
                double k1 = m[s,s];

                for (int c = s; c <= N - 1; c++)
                {
                    m[s,c] = m[s,c] / k1;
                }

                l[s] = l[s] / k1;
                for (int s1 = s + 1; s1 <= N - 1; s1++)
                {
                    double k2 = m[s1,s];
                    for (int c1 = s; c1 <= N - 1; c1++)
                    {
                        m[s1,c1] = -m[s,c1] * k2 + m[s1,c1];
                    }
                    l[s1] = -l[s] * k2 + l[s1];
                }
            }
            // обратный ход
            x[N - 1] = l[N - 1] / m[N - 1,N - 1];
            for (int i = N - 2; i >= 0; i--)
            {
                double w = 0;
                for (int j = N - 1; j > i; j--)
                {
                    w = w + x[j] * m[i,j];
                }
                x[i] = (l[i] - w);
            }
            return x;
        }
    }
}
