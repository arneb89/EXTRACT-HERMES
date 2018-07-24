using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class EcheData
    {
        public static int[] ord_mid_pos = null;

        public static int x_dc_0;

        public static double[][] disp_coeffs = null;

        public static int polinom_degree;

        public static void LoadOrdMidPos(string path_ord_mid_pos, int col_start)
        {
            StreamReader sr_mid_pos = new StreamReader(path_ord_mid_pos);
            string str;
            string[] strMas;
            str = sr_mid_pos.ReadLine();
            string[] delims = new string[] { " ", "\t" };
            strMas = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            int pos_count = int.Parse(strMas[0]);
            ord_mid_pos = new int[pos_count];
            for (int i = 0; i < pos_count; i++)
            {
                str = sr_mid_pos.ReadLine();
                strMas = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                ord_mid_pos[i] = int.Parse(str) - col_start;
            }
        }

        public static void LoadDispCurves(string path)
        {
            StreamReader sr=new StreamReader(path);
            string str;
            string[] strMas;
            
            str = sr.ReadLine();
            strMas = str.Split(new string[] { " ", "\t" }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            int rows_number = int.Parse(strMas[0]);
            int cols_number = int.Parse(strMas[1]);
            int x_dc_0 = int.Parse(strMas[2]);

            polinom_degree = cols_number - 1;
            disp_coeffs = new double[rows_number][];
            for (int i = 0; i < rows_number; i++) 
                disp_coeffs[i] = new double[cols_number];

            for (int i = 0; i < rows_number; i++)
            {
                str = sr.ReadLine();
                strMas = str.Split(new string[] { " ", "\t" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cols_number; j++)
                {
                    disp_coeffs[i][j] = double.Parse(strMas[j], 
                        System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            sr.Close();
        }

        public static double GetWL(int order, double pixel, int x_trim_left)
        {
            double sum = 0;
            for (int i = 0; i < polinom_degree + 1; i++)
            {
                sum += Math.Pow(pixel + (x_trim_left - x_dc_0), i) * disp_coeffs[order][i];
            }
            return sum;
        }

        public static void IdentOrders(ref double[][] pos_ord, ref double[][] pos_min, int column)
        {
            double[][] pos_ord_1 = new double[ord_mid_pos.Length][];
            for (int i = 0; i < pos_ord_1.Length; i++)
                pos_ord_1[i] = new double[pos_ord[0].Length];
            
            double[][] pos_min_1 = new double[ord_mid_pos.Length + 1][];
            for (int i = 0; i < pos_min_1.Length; i++)
                pos_min_1[i] = new double[pos_min[0].Length];

            int shift = 0;

            int ident_orders_count = 0;

            for (int i = 0; i < pos_ord_1.Length; i++)
            {
                int k_min = 0;
                double diff_min = double.MaxValue;
                double diff;
                for (int j = 0; j < pos_ord.Length; j++)
                {
                    diff = Math.Abs(pos_ord[j][column] - ord_mid_pos[i]);
                    if (diff < diff_min)
                    {
                        k_min = j;
                        diff_min = diff;
                        if (i == 0) shift = j;
                    }
                }
                if (i > 0)
                {
                    if (pos_ord[k_min][column] == pos_ord_1[i - 1][column])
                    {
                        break;
                    }
                }
                ident_orders_count++;
                for (int j = 0; j < pos_ord_1[i].Length; j++)
                {
                    pos_ord_1[i][j] = pos_ord[k_min][j];
                }
            }

            Array.Resize(ref pos_ord_1, ident_orders_count);
            Array.Resize(ref pos_min_1, ident_orders_count + 1);

            for (int i = 0; i < pos_min_1.Length; i++)
            {
                for (int j = 0; j < pos_min_1[i].Length; j++)
                {
                    pos_min_1[i][j] = pos_min[i + shift - 1][j];
                }
            }

            pos_min = pos_min_1;
            pos_ord = pos_ord_1;

            double[] pos_ord_slice = new double[pos_ord.Length];
            for (int i = 0; i < pos_ord_slice.Length; i++)
            {
                pos_ord_slice[i] = pos_ord[i][column];
            }
            double[] pos_min_slice = new double[pos_min.Length];
            for (int i = 0; i < pos_min_slice.Length; i++)
            {
                pos_min_slice[i] = pos_min[i][column];
            }

            Saver.SaveColumn(pos_ord_slice, InitFile.DirectoryMain + "\\orders_ident.dat");
            Saver.SaveColumn(pos_min_slice, InitFile.DirectoryMain + "\\minima_ident.dat");
        }

        public static int[] IdentOrdersInSlice(double[] pos_ord)
        {
            int[] mask = new int[ord_mid_pos.Length];
            
            for (int i = 0; i < ord_mid_pos.Length; i++)
            {
                double diff = double.MaxValue;
                for (int j = 0; j < pos_ord.Length; j++)
                {
                    if(diff>Math.Abs(pos_ord[j]-ord_mid_pos[i]))
                    {
                        diff = Math.Abs(pos_ord[j] - ord_mid_pos[i]);
                        mask[i] = j;
                    }
                }
            }
            return mask;
        }
    }
}
