using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Statistics
    {
        public static double Median(double[] array)
        {
            double median;
            double temp = 0; // временная переменная для хранения элемента массива
            bool exit = false; // болевая переменная для выхода из цикла, если массив отсортирован

            while (!exit)
            {
                exit = true;
                for (int i = 0; i < array.Length - 1; i++)
                {
                    //сортировка пузырьком по возрастанию - знак >
                    //сортировка пузырьком по убыванию - знак <
                    if (array[i] < array[i + 1]) // сравниваем два соседних элемента
                    {
                        // выполняем перестановку элементов массива
                        temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        exit = false;
                    }
                }
            }

            if (array.Length % 2 == 0) median = (array[array.Length / 2] + array[array.Length / 2 - 1]) * 0.5;
            else median = array[array.Length / 2];

            return median;
        }

        public static double Mean(Image img)
        {
            double res = 0;
            for (int i = 0; i < img.NAXIS1; i++)
            {
                for (int j = 0; j < img.NAXIS2; j++)
                {
                    res += img[i,j];
                }
            }
            res = res / img.NAXIS1 / img.NAXIS2;
            return res;
        }

        public static double StdDev(Image img)
        {
            double res = 0;
            double mean=Mean(img);
            for (int i = 0; i < img.NAXIS1; i++)
            {
                for (int j = 0; j < img.NAXIS2; j++)
                {
                    res += Math.Pow(img[i,j] - mean, 2);
                }
            }
            res = Math.Sqrt(res / img.NAXIS1 / img.NAXIS2);
            return res;
        }
    }
}
