using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class FitSVD
    {
        int ndat, ma;
	    double tol;
	    double[] x, y, sig;
	    fit_functions funcs;
        fit_finctions_md funcsmd;
	    double[] a;
	    double[][] covar;
	    double chisq;
        double[][] xmd;


	    public FitSVD(double[] xx, double[] yy, double[] ssig,
	        fit_functions funks, double TOL /* = 1.e-12*/)
        {
            this.x=xx;
            this.y=yy;
            this.sig=ssig;
            this.ndat=yy.Length;
            this.funcs = funks;
            this.xmd=null;
            this.tol=TOL;
        }

        public FitSVD(double[][] xx, double[] yy, double[] ssig,
	        fit_finctions_md funks, double TOL /*=1.e-12*/)
        {
            this.ndat=yy.Length;
            this.x=null;
            this.xmd=xx;
            this.y=yy;
            this.sig=ssig;
            this.funcsmd=funks;
            this.tol=TOL;
        }

        public delegate double[] fit_functions(double x);
        public delegate double[] fit_finctions_md(double[] x);

        public void fit()
        {
            int i, j, k;
            double tmp, thresh, sum;
            if (x!=null) ma = funcs(x[0]).Length;
            else ma = funcsmd(row(xmd, 0)).Length;
            a = new double[ma];
            covar = new double[ma][];
            for (i = 0; i < ma; i++) covar[i] = new double[ma];

            double[][] aa = new double[ndat][];
            for (i = 0; i < ndat; i++) aa[i] = new double[ma];

            double[] b = new double[ndat];
            double[] afunc = new double[ma];

            for (i = 0; i < ndat; i++)
            {
                if (x!=null) afunc = funcs((x)[i]);
                else afunc = funcsmd(row(xmd, i));
                tmp = 1.0 / sig[i];
                for (j = 0; j < ma; j++) aa[i][j] = afunc[j] * tmp;
                b[i] = y[i] * tmp;
            }
            SVD svd = new SVD(aa);
            if (tol > 0) thresh = tol * svd.W[0];
            else thresh = -1;
            //thresh = (tol > 0. ? tol*svd.w[0] : -1.);
            svd.Solve(b, a, thresh);
            chisq = 0.0;
            for (i = 0; i < ndat; i++)
            {
                sum = 0.0;
                for (j = 0; j < ma; j++) sum += aa[i][j] * a[j];
                chisq += Math.Pow((sum - b[i]), 2);
            }
            for (i = 0; i < ma; i++)
            {
                for (j = 0; j < i + 1; j++)
                {
                    sum = 0.0;
                    for (k = 0; k < ma; k++)
                        if (svd.W[k] > svd.TSH)
                            sum += svd.V[i][k] * svd.V[j][k] / Math.Pow((svd.W[k]), 2);
                    covar[j][i] = covar[i][j] = sum;
                }
            }

        }

        private double[] row(double[][] a, int i)
        {
            int j, n = a[0].Length;
            double[] ans = new double[n];
            for (j = 0; j < n; j++) ans[j] = a[i][j];
            return ans;
        }

        public double[] FittedCoeffs
        {
            get
            {
                return this.a;
            }
        }

        public double ChiSq { get { return this.chisq; } }
    }
}
