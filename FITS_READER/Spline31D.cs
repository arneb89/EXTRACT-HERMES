using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    /// <summary>
    /// Class is assigned for interpolation of tabulated function by cubic spline;
    /// </summary>
    public class Spline31D
    {
        protected double[][] s;
        protected double[] x;

        public Spline31D() { }

        /// <summary>
        /// Constructor function of the class;
        /// </summary>
        /// <param name="x">x - array of abcissa data;</param>
        /// <param name="y">y - array of ordinate data.</param>
        public Spline31D(double[] x, double[] y)
        {
            int n = x.Length;
            this.x = x;
            double[] h = new double[n - 1];
            

            this.s = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                this.s[i] = new double[n];
            }

            double[] sp = new double[n];
            double[] a = new double[n - 2];
            double[] b = new double[n - 2];
            double[] c = new double[n - 2];
            double[] r = new double[n - 2];

            for (int i = 0; i < n - 1; i++)
            {
                h[i] = x[i + 1] - x[i];
            }

            for (int i = 0; i < n - 2; i++)
            {
                b[i] = 2 * (h[i] + h[i + 1]);
            }

            a[0] = 0;
            for (int i = 1; i < n - 2; i++)
            {
                a[i] = h[i];
            }

            for (int i = 0; i < n - 3; i++)
            {
                c[i] = h[i + 1];
            }
            c[n - 3] = 0;

            for (int i = 0; i < n - 2; i++)
            {
                r[i] = 6 * ((y[i + 2] - y[i + 1]) / h[i + 1] - (y[i + 1] - y[i]) / h[i]);
            }

            // Solving of the linear system TriDiag(a,b,c)sp=r;
            sp[0] = 0;
            sp[n-1] = 0;
            double bet = b[0];
            double[] gam = new double[n - 2];
            sp[1] = r[0] / bet;
            for (int j = 1; j < n - 2; j++)
            {
                gam[j] = c[j - 1] / bet;
                bet = b[j] - a[j] * gam[j];
                sp[j + 1] = (r[j] - a[j] * sp[j]) / bet;
            }

            for (int j = n - 4; j >= 0; j--)
            {
                sp[j + 1] -= gam[j + 1] * sp[j + 2];
            }
            // End of solving of SLE;

            for (int i = 0; i <= n-2; i++)
            {
                this.s[3][i] = (sp[i + 1] - sp[i]) / (6*h[i]);
                this.s[2][i] = sp[i]*0.5;
                this.s[1][i] = (y[i + 1] - y[i]) / h[i] - (2 * h[i] * sp[i] + h[i] * sp[i + 1]) / 6;
                this.s[0][i] = y[i];
            }
        }

        public double[] GetX
        {
            get { return this.x; }
        }

        public double[] GetY
        {
            get { return this.s[0]; }
        }

        public Spline31D(double[] xv, double[] yv, double yp1, double ypn)
        {
            int i,k;
	        double p,qn,sig,un;
            double[] y2=new double[xv.Length];
            this.x = xv;
	        int n=y2.Length;
	        double[] u  = new double[n-1];

            double[] h = new double[n - 1];
            for (i = 0; i < n - 1; i++)
            {
                h[i] = x[i + 1] - x[i];
            }

            this.s = new double[4][];
            for (i = 0; i < 4; i++)
            {
                this.s[i] = new double[n];
            }

            y2[0] = -0.5;
            u[0] = (3.0 / (xv[1] - xv[0])) * ((yv[1] - yv[0]) / (xv[1] - xv[0]) - yp1);

	        for (i=1;i<n-1;i++) 
            {
                sig = (xv[i] - xv[i - 1]) / (xv[i + 1] - xv[i - 1]);
                p = sig * y2[i - 1] + 2.0;
                y2[i] = (sig - 1.0) / p;
                u[i] = (yv[i + 1] - yv[i]) / (xv[i + 1] - xv[i]) - (yv[i] - yv[i - 1]) / (xv[i] - xv[i - 1]);
                u[i] = (6.0 * u[i] / (xv[i + 1] - xv[i - 1]) - sig * u[i - 1]) / p;
	        }

		    qn=0.5;
            un = (3.0 / (xv[n - 1] - xv[n - 2])) * (ypn - (yv[n - 1] - yv[n - 2]) / (xv[n - 1] - xv[n - 2]));

            y2[n - 1] = (un - qn * u[n - 2]) / (qn * y2[n - 2] + 1.0);
            for (k = n - 2; k >= 0; k--)
                y2[k] = y2[k] * y2[k + 1] + u[k];

            for (i = 0; i <= n - 2; i++)
            {
                this.s[3][i] = (y2[i + 1] - y2[i]) / (6 * h[i]);
                this.s[2][i] = y2[i] * 0.5;
                this.s[1][i] = (yv[i + 1] - yv[i]) / h[i] - (2 * h[i] * y2[i] + h[i] * y2[i + 1]) / 6;
                this.s[0][i] = yv[i];
            }

            //int n = x.Length;
            //this.x = x;
            //double[] h = new double[n - 1];


            //this.s = new double[4][];
            //for (int i = 0; i < 4; i++)
            //{
            //    this.s[i] = new double[n];
            //}

            //double[] sp = new double[n];
            //double[] a = new double[n - 2];
            //double[] b = new double[n - 2];
            //double[] c = new double[n - 2];
            //double[] r = new double[n - 2];

            //for (int i = 0; i < n - 1; i++)
            //{
            //    h[i] = x[i + 1] - x[i];
            //}

            //for (int i = 0; i < n - 2; i++)
            //{
            //    b[i] = 2 * (h[i] + h[i + 1]);
            //}

            //a[0] = 0;
            //for (int i = 1; i < n - 2; i++)
            //{
            //    a[i] = h[i];
            //}

            //for (int i = 0; i < n - 3; i++)
            //{
            //    c[i] = h[i + 1];
            //}
            //c[n - 3] = 0;

            //for (int i = 0; i < n - 2; i++)
            //{
            //    r[i] = 6 * ((y[i + 2] - y[i + 1]) / h[i + 1] - (y[i + 1] - y[i]) / h[i]);
            //}

            //// Solving of the linear system TriDiag(a,b,c)sp=r;
            //sp[0] = xder2begin;
            //sp[n - 1] = xder2end;
            //double bet = b[0];
            //double[] gam = new double[n - 2];
            //sp[1] = r[0] / bet;
            //for (int j = 1; j < n - 2; j++)
            //{
            //    gam[j] = c[j - 1] / bet;
            //    bet = b[j] - a[j] * gam[j];
            //    sp[j + 1] = (r[j] - a[j] * sp[j]) / bet;
            //}

            //for (int j = n - 4; j >= 0; j--)
            //{
            //    sp[j + 1] -= gam[j + 1] * sp[j + 2];
            //}
            //// End of solving of SLE;

            //for (int i = 0; i <= n - 2; i++)
            //{
            //    this.s[3][i] = (sp[i + 1] - sp[i]) / (6 * h[i]);
            //    this.s[2][i] = sp[i] * 0.5;
            //    this.s[1][i] = (y[i + 1] - y[i]) / h[i] - (2 * h[i] * sp[i] + h[i] * sp[i + 1]) / 6;
            //    this.s[0][i] = y[i];
            //}
        }

        private int SearchNearXPoint(double x0)
        {
            int i;
            for (i = 1; ; i++)
            {
                if (x0 < x[i] || i == x.Length - 1)
                {
                    break;
                }
            }
            return i-1;
        }

        /// <summary>
        /// Returns value of function in point x0;
        /// </summary>
        /// <param name="x0"></param>
        /// <returns></returns>
        public double Interp(double x0)
        {
            int i=this.SearchNearXPoint(x0);
            
            double y0 = this.s[3][i] * Math.Pow((x0 - x[i]), 3) + 
                        this.s[2][i] * Math.Pow((x0 - x[i]), 2) + 
                        this.s[1][i] * (x0 - x[i]) + 
                        this.s[0][i];

            return y0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0"></param>
        /// <returns></returns>
        public double InterpD1(double x0)
        {
            int i=this.SearchNearXPoint(x0);

            double y0 = this.s[3][i] * Math.Pow((x0 - x[i]), 2)*3 +
                        this.s[2][i] * (x0 - x[i])*2 +
                        this.s[1][i];

            return y0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0"></param>
        /// <returns></returns>
        public double InterpD2(double x0)
        {
            int i = this.SearchNearXPoint(x0);

            double y0 = this.s[3][i] * (x0 - x[i]) * 6 +
                        this.s[2][i] * 2;

            return y0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0"></param>
        /// <returns></returns>
        public double InterpD3(double x0)
        {
            int i = this.SearchNearXPoint(x0);

            double y0 = this.s[3][i] * 6;

            return y0;
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masA
        {
            get
            {
                return this.s[3];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masB
        {
            get
            {
                return this.s[2];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masC
        {
            get
            {
                return this.s[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masD
        {
            get
            {
                return this.s[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masD0
        {
            get
            {
                return this.s[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masD1
        {
            get
            {
                return this.s[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masD2
        {
            get
            {
                double[] masd2 = new double[this.s[2].Length];
                for (int i = 0; i < masd2.Length; i++)
                {
                    masd2[i] = this.s[2][i] * 2;
                }
                return masd2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] masD3
        {
            get
            {
                double[] masd3 = new double[this.s[3].Length];
                for (int i = 0; i < masd3.Length; i++)
                {
                    masd3[i] = this.s[3][i] * 6;
                }
                return masd3;
            }
        }
    }
}
