using System;
using System.Threading.Tasks;

namespace Pipeline
{
	class Background
	{
		private static int ord_x, ord_y;

        public static Image Fit(Image im, double[][] pos_min, int ox, int oy, int smooth_wing, int step)
		{
			ord_x = ox;
			ord_y = oy;
            int n_pix_y = im.NAXIS1;
            int n_pix_x = im.NAXIS2;
            int n_mins = pos_min.Length;
			int n_points = pos_min.Length * pos_min [0].Length;
			
            double[] xx = new double[n_points];
            double[] yy = new double[n_points];
            //for (int i = 0; i < xx.Length; i++)
            //    xx[i] = new double[2];
			
            double[] zz = new double[n_points];
			double[] sigmas = new double[n_points];
            for (int i = 0; i < sigmas.Length; i++)
                sigmas[i] = 1.0;

            double[][] zz_filt = new double[n_mins][];
            for (int i = 0; i < pos_min.Length; i++) 
                zz_filt[i] = new double[n_pix_x];
            for (int i = 0; i < zz_filt.Length; i++)
            {
                for (int j = 0; j < zz_filt[i].Length; j++)
                    zz_filt[i][j] = im[(int)Math.Round(pos_min[i][j]), j];
                zz_filt[i] = Filters.UniformFilter(zz_filt[i], smooth_wing);
            }

            double max_flux = im[(int)Math.Round(pos_min[0][0]), 0];
            for (int i = 0; i < n_mins; i++)
                for (int j = 0; j < n_pix_x; j++)
                    if (zz_filt[i][j] > max_flux) max_flux = zz_filt[i][j];

			int k = 0;
            for (int i = 0; i < pos_min.Length; i++)
                for (int j = 0; j < pos_min[i].Length; j = j + step)
                {
                    zz[k] = im[(int)Math.Round(pos_min[i][j]), j] / max_flux;
                    yy[k] = pos_min[i][j] / n_pix_y;
                    xx[k] = (double)j / n_pix_x;
                    k++;
                }
            
            n_points = k;
            Array.Resize(ref zz, n_points);
            Array.Resize(ref xx, n_points);
            Array.Resize(ref yy, n_points);
            Array.Resize(ref sigmas, n_points);

			//FitSVD.fit_finctions_md func = new FitSVD.fit_finctions_md (Pars);

			//FitSVD fitter = new FitSVD (xx, zz, sigmas, func, 1e-50);

			//fitter.fit ();

            //double rms = Math.Sqrt(fitter.ChiSq/n_points);

			//double[] coeffs = fitter.FittedCoeffs;

            double[] coeffs = Fitting(xx, yy, zz, ox, oy);

			Image bb = new Image (n_pix_y, n_pix_x);

            Parallel.For(0, n_pix_y, i =>
            {
                //for (int i = 0; i < bb.NAXIS1; i++)
                //{
                for (int j = 0; j < bb.NAXIS2; j++)
                {
                    bb[i, j] = Surface(coeffs, (double)j / n_pix_x, (double)i / n_pix_y, ox, oy) * max_flux;
                    if (bb[i, j] < 0) bb[i, j] = 0;
                }
                //}
            });
			return bb;
		}

		private static double[] Pars(double[] xy)
        {
            double[] pars = new double[(ord_y + 1) * (ord_x + 1)];
            int k = 0;
            for (int i = 0; i <= ord_y; i++)
            {
                for (int j = 0; j <= ord_x; j++ )
                {
                    pars[k] = Math.Pow(xy[0], i) * Math.Pow(xy[1], j);
                    k++;
                }
            }
            return pars;
        }

        private static double Surface(double[] coeff, double x, double y, int ox, int oy)
        {
            double sum = 0;
            double sum1;
            int k1, k2;
            for (int i = 0; i <= ox; i++)
            {
                k1 = i * (oy + 1);
                k2 = k1 + oy;
                sum1 = coeff[k2];
                while(k2>k1) 
                    sum1 = sum1 * y + coeff[--k2];
                sum += Math.Pow(x, i) * sum1;
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
                double k1 = m[s, s];

                for (int c = s; c <= N - 1; c++)
                {
                    m[s, c] = m[s, c] / k1;
                }

                l[s] = l[s] / k1;
                for (int s1 = s + 1; s1 <= N - 1; s1++)
                {
                    double k2 = m[s1, s];
                    for (int c1 = s; c1 <= N - 1; c1++)
                    {
                        m[s1, c1] = -m[s, c1] * k2 + m[s1, c1];
                    }
                    l[s1] = -l[s] * k2 + l[s1];
                }
            }
            // обратный ход
            x[N - 1] = l[N - 1] / m[N - 1, N - 1];
            for (int i = N - 2; i >= 0; i--)
            {
                double w = 0;
                for (int j = N - 1; j > i; j--)
                {
                    w = w + x[j] * m[i, j];
                }
                x[i] = (l[i] - w);
            }
            return x;
        }
	}
}

