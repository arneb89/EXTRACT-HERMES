using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Normator
    {
        public static double[][] FitCurves = null;

        private static int poly_degree;

        public static double[][] OrdersNorm = null;
        
        public static void Norm(double[][] orders, int polynom_degree, int order_1, int order_2)
        {
            poly_degree = polynom_degree;
            int orders_count=orders.Length;
            int pixels_count = orders[0].Length;

            OrdersNorm = new double[orders_count][];
            for (int i = 0; i < orders_count; i++)
            {
                OrdersNorm[i] = new double[pixels_count];
                for (int j = 0; j < pixels_count; j++)
                {
                    OrdersNorm[i][j] = 1.0;
                }
            }

            FitCurves = new double[orders_count][];
            for (int i = 0; i < orders_count; i++)
            {
                FitCurves[i] = new double[pixels_count];
                for (int j = 0; j < pixels_count; j++)
                {
                    FitCurves[i][j] = orders[i][j];
                }
            }

            double[][] orders1 = new double[orders_count][];
            for (int i = 0; i < orders_count; i++)
            {
                orders1[i] = new double[pixels_count];
                for (int j = 0; j < pixels_count; j++)
                {
                    orders1[i][j] = orders[i][j];
                }
            }

            double[] pixels = new double[pixels_count];
            for (int i = 0; i < pixels_count; i++)
            {
                pixels[i] = i;
            }

            double pix_max = pixels[pixels.Length - 1];
            double flx_max;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = pixels[i] / pix_max;
            }

            double[] coeffs;
            double[] sigmas = new double[pixels_count];

            for (int i = order_1; i <= order_2; i++)
            {
                flx_max = orders1[i].Max();
                for (int j = 0; j < pixels_count; j++)
                {
                    orders1[i][j] = orders1[i][j] / flx_max;
                }

                for (int j = 0; j < sigmas.Length; j++)
                {
                    sigmas[j] = orders1[i][j];
                    if (sigmas[j] <= 0) sigmas[j] = 1;
                    //if (orders1[i][j] * flx_max > 1)
                    //{
                    //    sigmas[j] = Math.Sqrt(orders1[i][j]);
                    //}
                    //else
                    //{
                    //    sigmas[j] = 1;
                    //}
                }

                FitSVD fitterSVD = new FitSVD(pixels, orders1[i], sigmas, polynom, 1e-30);
                //Fitter fitter = new Fitter();
                fitterSVD.fit();
                //coeffs = fitter.WightedPolynom(pixels, orders1[i], sigmas, poly_degree);
                coeffs = fitterSVD.FittedCoeffs;

                for (int j = 0; j < pixels.Length; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < polynom_degree + 1; k++)
                    {
                        sum += Math.Pow(pixels[j], k) * coeffs[k];
                    }
                    FitCurves[i][j] = sum * flx_max; ;
                    OrdersNorm[i][j] = orders[i][j] / FitCurves[i][j];
                }
            }
        }

        private static double[] polynom(double x)
        {
            double[] ans = new double[poly_degree + 1];
            ans[0] = 1.0;
            for (int i = 1; i < poly_degree+1; i++) ans[i] = x * ans[i - 1];
            return ans;
        }
    }
}
