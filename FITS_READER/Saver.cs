using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class Saver
    {
        public static void SaveOrderedDistribution(double[][] x, string file_name)
        {
            StreamWriter sw = new StreamWriter(file_name);
            for (int i = 0; i < x[0].Length; i++)
            {
                sw.Write((i + 1).ToString() + "\t");
                for (int j = 0; j < x.Length; j++)
                {
                    sw.Write(x[j][i].ToString().Replace(",", ".") + "\t");
                }
                sw.Write("\r\n");
            }
            sw.Close();
        }

        public static void SaveOrderedDistribution(double[][] x, double[][] y, string file_name)
        {
            StreamWriter sw = new StreamWriter(file_name);
            for (int i = 0; i < x[0].Length; i++)
            {
                sw.Write((i + 1).ToString() + "\t");
                for (int j = 0; j < x.Length; j++)
                {
                    sw.Write(x[j][i].ToString().Replace(",", ".") + "\t" + 
                        y[j][i].ToString().Replace(",", ".") + "\t");
                }
                sw.Write("\r\n");
            }
            sw.Close();
        }

        public static void SaveColumn(double[] x, string file_name)
        {
            StreamWriter sw = new StreamWriter(file_name);
            for (int i = 0; i < x.Length; i++)
                sw.WriteLine(x[i].ToString().Replace(",", "."));
            sw.Close();
        }

        public static void SaveLambdaIntensTable(double[][] lambdas, double[][] intes, string file_name)
        {
            StreamWriter sw = new StreamWriter(file_name);
            for (int i = 0; i < lambdas[0].Length; i++)
            {
                for (int j = 0; j < lambdas.Length; j++)
                {
                    sw.Write(string.Format("{0:0000.0000}\t{1:0.000000E000}\t", 
                        lambdas[j][i], intes[j][i]).Replace(",", "."));
                }
                sw.Write("\r\n");
            }
            sw.Close();
        }
    }
}
