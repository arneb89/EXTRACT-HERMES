using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipeline
{
    class ImageCombinator
    {
        public static Image Median(Image[] images)
        {
            Image median_image = new Image(images[0].NAXIS1, images[0].NAXIS2);

            Parallel.For(0, median_image.NAXIS1, i =>
            {
                double[] vals = new double[images.Length];
                //for (int i = 0; i < median_image.NAXIS1; i++)
                //{
                for (int j = 0; j < median_image.NAXIS2; j++)
                {
                    for (int k = 0; k < images.Length; k++)
                    {
                        vals[k] = images[k][i, j];
                    }
                    Sort(ref vals);
                    if (vals.Length % 2 == 0)
                        median_image[i, j] = (vals[vals.Length / 2] + vals[vals.Length / 2 - 1]) * 0.5;
                    else
                        median_image[i, j] = vals[vals.Length / 2];
                }
                //}
            });
            return median_image;
        }

        private static void Sort(ref double[] array)
        {
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
        }
    }
}
