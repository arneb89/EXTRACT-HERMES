using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Fitter
    {
        public double[] Polynom(double[] x, double[] y, int degree)
        {
            double[] c = new double[degree + 1];
            double[] b = new double[degree + 1];
            double[][] r = new double[degree + 1][];
            for (int i = 0; i < r.Length; i++) r[i] = new double[degree + 1];


            for (int l = 0; l < degree + 1; l++)
            {
                double sum = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    sum = sum + y[i] * Math.Pow(x[i], l);
                }
                b[l] = sum;
            }

            for (int l = 0; l < degree + 1; l++)
            {
                for (int k = 0; k <= l; k++)
                {
                    double sum = 0;
                    for (int i = 0; i < x.Length; i++)
                    {
                        sum = sum + Math.Pow(x[i], l) *
                            Math.Pow(x[i], k);
                    }
                    r[l][k] = sum;
                    r[k][l] = sum;
                }
            }

            c = this.SolveWithGaussMethod(r, b);

            return c;
        }

        public double[] WightedPolynom(double[] x, double[] y, double[] weigths, int degree)
        {
            double[] c = new double[degree + 1];
            double[] b = new double[degree + 1];
            double[][] r = new double[degree + 1][];
            for (int i = 0; i < r.Length; i++) r[i] = new double[degree + 1];


            for (int l = 0; l < degree + 1; l++)
            {
                double sum = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    sum = sum + y[i] * Math.Pow(x[i], l) / Math.Pow(weigths[i], 2);
                }
                b[l] = sum;
            }

            for (int l = 0; l < degree + 1; l++)
            {
                for (int k = 0; k <= l; k++)
                {
                    double sum = 0;
                    for (int i = 0; i < x.Length; i++)
                    {
                        sum = sum + Math.Pow(x[i], l) *
                            Math.Pow(x[i], k) / Math.Pow(weigths[i], 2);
                    }
                    r[l][k] = sum;
                    r[k][l] = sum;
                }
            }

            c = this.GaussJ(r, b);
             
            return c;
        }

        private double func_polynom(double[] c, double x)
        {
            double sum = 0;
            for (int i = 0; i < c.Length; i++)
            {
                sum += Math.Pow(x, i) * c[i];
            }
            return sum;
        }

        //public double[] WightedPolynom(double[] x, double[] y, double[] weigths, int degree, double sigmaClip, int iterNum)
        //{
        //    double[] c;
        //    double sigma = 0;
        //    for (int i = 0; i < iterNum; i++)
        //    {
        //        sigma = 0;
        //        c = this.WightedPolynom(x, y, weigths, degree);
        //        for (int j = 0; j < x.Length; j++)
        //        {
        //        }
        //    }
        //}

        private double[] SolveWithGaussMethod(double[][] m, double[] l)
        {
            int N = l.Length;

            double[] x = new double[N];

            // Приведение матрицы m к треугольному виду
            for (int s = 0; s <= N - 2; s++)
            {
                double k1 = m[s][s];

                for (int c = s; c <= N - 1; c++)
                {
                    m[s][c] = m[s][c] / k1;
                }

                l[s] = l[s] / k1;
                for (int s1 = s + 1; s1 <= N - 1; s1++)
                {
                    double k2 = m[s1][s];
                    for (int c1 = s; c1 <= N - 1; c1++)
                    {
                        m[s1][c1] = -m[s][c1] * k2 + m[s1][c1];
                    }
                    l[s1] = -l[s] * k2 + l[s1];
                }
            }
            // обратный ход
            x[N - 1] = l[N - 1] / m[N - 1][N - 1];
            for (int i = N - 2; i >= 0; i--)
            {
                double w = 0;
                for (int j = N - 1; j > i; j--)
                {
                    w = w + x[j] * m[i][j];
                }
                x[i] = (l[i] - w);
            }
            return x;
        }

        private double[] GaussJ(double[][] a, double[] b)
        {
            double[][] b1 = new double[b.Length][];
            for (int i = 0; i < b1.Length; i++)
            {
                b1[i] = new double[1];
                b1[i][0] = b[i];
            }
            
            this.GaussJ(ref a, ref b1);
            double[] x = new double[b1.Length];
            for (int i = 0; i < b1.Length; i++) x[i] = b1[i][0];
            return x;
        }

        private void GaussJ(ref double[][] a, ref double[][] b)
        {
            int i, icol=0, irow=0, j, k, l, ll, n = a.Length, m = b[0].Length;
	        double big,dum,pivinv;
	        int[] indxc=new int[n];
            int[] indxr = new int[n];
            int[] ipiv = new int[n];
	        for (j=0;j<n;j++) ipiv[j]=0;
	        for (i=0;i<n;i++) 
            {
		        big=0.0;
		        for (j=0;j<n;j++)
			        if (ipiv[j] != 1)
				for (k=0;k<n;k++) 
                {
					if (ipiv[k] == 0) 
                    {
						if (Math.Abs(a[j][k]) >= big) 
                        {
                            big = Math.Abs(a[j][k]);
							irow=j;
							icol=k;
						}
					}
				}
		        ++(ipiv[icol]);
		        if (irow != icol) 
                {
			        for (l=0;l<n;l++) SWAP(ref a[irow][l],ref a[icol][l]);
			        for (l=0;l<m;l++) SWAP(ref b[irow][l],ref b[icol][l]);
		        }
		        indxr[i]=irow;
		        indxc[i]=icol;
		        //if (a[icol][icol] == 0.0) throw("gaussj: Singular Matrix");
		        pivinv=1.0/a[icol][icol];
		        a[icol][icol]=1.0;
		        for (l=0;l<n;l++) a[icol][l] *= pivinv;
		        for (l=0;l<m;l++) b[icol][l] *= pivinv;
		        for (ll=0;ll<n;ll++)
			    if (ll != icol) 
                {
				    dum=a[ll][icol];
				    a[ll][icol]=0.0;
				    for (l=0;l<n;l++) a[ll][l] -= a[icol][l]*dum;
				    for (l=0;l<m;l++) b[ll][l] -= b[icol][l]*dum;
			    }
	        }
	        for (l=n-1;l>=0;l--) 
            {
		        if (indxr[l] != indxc[l])
			        for (k=0;k<n;k++)
				        SWAP(ref a[k][indxr[l]],ref a[k][indxc[l]]);
	        }
        }

        private void SWAP(ref double a, ref double b)
        {
            double c = a;
            a = b;
            b = c;
        }
    }
}
