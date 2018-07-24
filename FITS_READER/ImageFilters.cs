using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class ImageFilters
    {
        public static Image Convolve(double[][] kernel, Image image)
        {
            int wingH = (kernel.Length - 1) / 2;
            int wingV = (kernel[0].Length - 1) / 2;
            Image convImage = new Image(image.NAXIS1, image.NAXIS2);
            for (int i = wingV; i <= convImage.NAXIS1 - wingV; i++)
            {
                for (int j = wingH; j <= convImage.NAXIS2 - wingH; j++)
                {
                    double sum = 0;
                    int i2 = 0, j2 = 0;
                    for (int i1 = i - wingV; i1 < i + wingV; i1++)
                    {
                        j2 = 0;
                        for (int j1 = j - wingH; j1 < j + wingH; j1++)
                        {
                            sum += image[i1, j1] * kernel[i2][j2];
                            j2++;
                        }
                        i2++;
                    }
                    convImage[i, j] = sum;
                }
            }
            return convImage;
        }
    }
}
