using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class RobustFitter
    {
        public static double[] Coeffs;

        public static void FitPolynom(double[] xx, double[] yy, int degree, int iterNum)
        {
            int dataNum = xx.Length;
            double[] c = new double[degree + 1];
            double[] b = new double[degree + 1];
            double[][] r = new double[degree + 1][];

            double[] diagR = new double[dataNum];
            double[] res = new double[dataNum];

            double[][] g = new double[dataNum][];
            for (int i = 0; i < dataNum; i++) g[i] = new double[degree + 1];
            for (int i = 0; i < r.Length; i++) r[i] = new double[degree + 1];

            for (int i = 0; i < dataNum; i++)
            {
                for (int j = 0; j < degree + 1; j++)
                {
                    g[i][j] = Math.Pow(xx[i], j);
                }
            }

            for (int iter = 0; iter < iterNum; iter++)
            {
                if (iter == 0)
                {
                    for (int i = 0; i < dataNum; i++) diagR[i] = 1.0;
                }
                else
                {
                    for (int i = 0; i < dataNum; i++) diagR[i] = res[i];
                }

                for (int l = 0; l < degree + 1; l++)
                {
                    double sum = 0;
                    for (int i = 0; i < dataNum; i++)
                    {
                        sum = sum + g[i][l] * diagR[i] * yy[i];
                    }
                    b[l] = sum;
                }

                for (int l = 0; l < degree + 1; l++)
                {
                    for (int k = 0; k <= l; k++)
                    {
                        double sum = 0;
                        for (int i = 0; i < dataNum; i++)
                        {
                            sum = sum + g[i][l] * diagR[i] * g[i][k];
                        }
                        r[l][k] = sum;
                        r[k][l] = sum;
                    }
                }
                c = SolveWithGaussMethod(r, b);
                
                for (int i = 0; i < dataNum; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < degree + 1; j++)
                    {
                        sum += g[i][j] * c[j];
                    }
                    res[i] = Math.Abs(sum - yy[i]);
                }
            }
            Coeffs = c;
        }

        private static double[] SolveWithGaussMethod(double[][] m, double[] l)
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

        public static double FuncValue(double x)
        {
            double sum = 0;
            for (int i = 0; i < Coeffs.Length; i++)
            {
                sum += Coeffs[i] * Math.Pow(x, i);
            }
            return sum;
        }
    }
}
