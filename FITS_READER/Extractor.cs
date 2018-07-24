using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Extractor
    {
        private static Image image = null;
        
        public static double[][] Flux = null;
        public static double[][] Background = null;
        public static double[][] FluxDebiased = null;

        public static double[][] fluxes_min_init = null;
        public static double[][] fluxes_min_smoothed = null;

        public static void ExtractMode1(Image im, double[][] pos_ord, double[][] pos_min, int aperture_wing, int smooth_wing)
        {
            image = im;
            int pix_count = im.NAXIS2;
            int ord_count = pos_ord.Length;
            int min_count = pos_min.Length; // min_count = ord_count + 1

            int[][] pos_min_int = new int[min_count][];
            for (int i = 0; i < min_count; i++)
                pos_min_int[i] = new int[pix_count];

            for (int i = 0; i < min_count; i++)
                for (int j = 0; j < pix_count; j++)
                    pos_min_int[i][j] = (int)Math.Round(pos_min[i][j], 0);

            fluxes_min_init = new double[min_count][];
            for (int i = 0; i < min_count; i++) 
                fluxes_min_init[i] = new double[pix_count];

            for (int i = 0; i < min_count; i++)
                for (int j = 0; j < pix_count; j++)
                    fluxes_min_init[i][j] = image[pos_min_int[i][j], j];

            fluxes_min_smoothed = new double[min_count][];
            for (int i = 0; i < min_count; i++)
                fluxes_min_smoothed[i] = Filters.UniformFilter(fluxes_min_init[i], smooth_wing);
            
            Flux = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                Flux[i] = new double[pix_count];

            Background = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                Background[i] = new double[pix_count];

            FluxDebiased = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                FluxDebiased[i] = new double[pix_count];

            Console.Write("Extracted orders: ");
            for (int ord = 0; ord < ord_count; ord++)
            {
                Console.Write("[{0}]", ord);
                for (int i = 0; i < pix_count; i++)
                {
                    int n1, n2;

                    n1 = pos_min_int[ord][i];
                    n2 = pos_min_int[ord + 1][i];

                    if (aperture_wing != -1)
                    {
                        int ord_pos_int = (int)Math.Round(pos_ord[ord][i], 0);
                        n1 = ord_pos_int - aperture_wing;
                        n2 = ord_pos_int + aperture_wing;
                    }
                    
                    double sum = 0;
                    double kkk = (fluxes_min_smoothed[ord + 1][i] - fluxes_min_smoothed[ord][i]) /
                        (pos_min_int[ord + 1][i] - pos_min_int[ord][i]);
                    double bkg = 0;
                    for (int j = n1; j <= n2; j++)
                    {
                        sum += im[j, i];
                        bkg += kkk * (j - pos_min_int[ord][i]) + fluxes_min_smoothed[ord][i];
                    }
                    Flux[ord][i] = sum;
                    Background[ord][i] = bkg;
                    
                    FluxDebiased[ord][i] = Flux[ord][i] - bkg;
                    if (FluxDebiased[ord][i] < 0) FluxDebiased[ord][i] = 0;
                }
            }
            Console.Write("\r\n");
        }

        public static void ExtractMode2(Image im, Image im_bkg, double[][] pos_ord, double[][] pos_min, int aperture_wing)
        {
            int pix_count = im.NAXIS2;
            int ord_count = pos_ord.Length;
            int min_count = pos_min.Length;

            int[][] pos_min_int = new int[min_count][];
            for (int i = 0; i < min_count; i++)
                pos_min_int[i] = new int[pix_count];
            for (int i = 0; i < min_count; i++)
                for (int j = 0; j < pix_count; j++)
                    pos_min_int[i][j] = (int)Math.Round(pos_min[i][j], 0);

            fluxes_min_init = new double[min_count][];
            for (int i = 0; i < min_count; i++)
                fluxes_min_init[i] = new double[pix_count];
            for (int i = 0; i < min_count; i++)
                for (int j = 0; j < pix_count; j++)
                    fluxes_min_init[i][j] = im[pos_min_int[i][j], j];

            fluxes_min_smoothed = new double[min_count][];
            for (int i = 0; i < min_count; i++)
                fluxes_min_smoothed[i] = new double[pix_count];
            for (int i = 0; i < min_count; i++)
                for (int j = 0; j < pix_count; j++)
                    fluxes_min_smoothed[i][j] = im_bkg[pos_min_int[i][j], j];

            Flux = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                Flux[i] = new double[pix_count];

            Background = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                Background[i] = new double[pix_count];

            FluxDebiased = new double[ord_count][];
            for (int i = 0; i < ord_count; i++)
                FluxDebiased[i] = new double[pix_count];

            Console.Write("Extracted orders: ");
            for (int ord = 0; ord < ord_count; ord++)
            {
                Console.Write("[{0}]", ord);
                for (int i = 0; i < pix_count; i++)
                {
                    int n1, n2;

                    n1 = pos_min_int[ord][i];
                    n2 = pos_min_int[ord + 1][i];

                    if (aperture_wing != -1)
                    {
                        int ord_pos_int = (int)Math.Round(pos_ord[ord][i], 0);
                        n1 = ord_pos_int - aperture_wing;
                        n2 = ord_pos_int + aperture_wing;
                    }

                    double sum = 0;
                    double bkg = 0;
                    for (int j = n1; j <= n2; j++)
                    {
                        sum += im[j, i];
                        bkg += im_bkg[j, i];
                    }
                    Flux[ord][i] = sum;
                    Background[ord][i] = bkg;

                    FluxDebiased[ord][i] = Flux[ord][i] - bkg;
                    if (FluxDebiased[ord][i] < 0) FluxDebiased[ord][i] = 0;
                }
            }
            Console.Write("\r\n");
        }
    }
}
