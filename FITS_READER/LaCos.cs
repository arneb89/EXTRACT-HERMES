using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class LaCos
    {
        Image im = null;
        public void LoadImage(Image image)
        {
            this.im = image;
        }
        public void Cleaning(double gain, double readnoise, double sigclip, double sigfrac, double objlim, int niter)
        {
            //double[][] kernel = new double[3][]{{0, -1, 0}, {-1, 4, -1}, {0, -1, 0}};

            //for (int i = 0; i < niter; i++)
            //{

            //}
        }
    }
}
