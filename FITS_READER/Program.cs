using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("*******************************************************");
            Console.WriteLine("*                  MERCATOR/HERMES                    *");
            Console.WriteLine("*            SPECTRA REDUCTION PIPELINE               *");
            Console.WriteLine("*******************************************************");
            Console.ForegroundColor = ConsoleColor.White;


            //Image.Save("file.fit");
            //goto STOP;

            //InitFile.ReadInitFile("INIT.dat");
            Init.Initialize("INIT.dat");
            if (Init.ErrorString != "")
            {
                Console.WriteLine("Error in INIT file!");
                Console.WriteLine(Init.ErrorString);
                goto STOP;
            }

            string dir_main = (string)Init.Value("DIR_MAIN");

            if (!Directory.Exists(dir_main))
            {
                try
                {
                    Directory.CreateDirectory(dir_main);
                }
                catch
                {
                    Console.WriteLine("Cannot create main directory.\r\n");
                    goto STOP;
                }
            }

            string[] bias_files = null, flat_files = null, thar_files = null, obj_files = null;
            Console.WriteLine("Searching for Bias, Flat, ThAr and Object files...");
            try
            {
                bias_files = ImageSelector.GetFilesByHDU(
                    (string)Init.Value("DIR_BIAS"), (string)Init.Value("FMASK_BIAS"), "IMAGETYP", "*");
                flat_files = ImageSelector.GetFilesByHDU(
                    (string)Init.Value("DIR_FLAT"), (string)Init.Value("FMASK_FLAT"), "IMAGETYP", "*");
                thar_files = ImageSelector.GetFilesByHDU(
                    (string)Init.Value("DIR_CLBR"), (string)Init.Value("FMASK_CLBR"), "IMAGETYP", "*");
                obj_files = ImageSelector.GetFilesByHDU(
                    (string)Init.Value("DIR_OBJ"), (string)Init.Value("FMASK_OBJ"), "IMAGETYP", "*");
            }
            catch
            {
                Console.WriteLine("Error in files searching...");
                goto STOP;
            }

            Console.WriteLine("Bias files number: {0}", bias_files.Length);
            Console.WriteLine("Flat files number: {0}", flat_files.Length);
            Console.WriteLine("ThAr files number: {0}", thar_files.Length);
            Console.WriteLine("Object files number: {0}", obj_files.Length);

            if (bias_files.Length == 0 || flat_files.Length == 0 ||
                thar_files.Length == 0 || obj_files.Length == 0)
            {
                Console.WriteLine("Some necessary files were not found...");
                goto STOP;
            }

            Image[] biases = new Image[bias_files.Length];
            Image[] flats = new Image[flat_files.Length];
            Image[] thars = new Image[thar_files.Length];
            Image[] objects = new Image[obj_files.Length];

            // Loading and averaging of bias images;
            Console.WriteLine("Loading Bias images...");
            for (int i = 0; i < bias_files.Length; i++)
            {
                Console.WriteLine("->" + bias_files[i]);
                biases[i] = new Image();
                biases[i].LoadImage(bias_files[i]);
                if ((bool)Init.Value("TRIM_ON"))
                    biases[i] = biases[i].Trim(
                        (int)Init.Value("TRIM_STR1"), (int)Init.Value("TRIM_COL1"),
                        (int)Init.Value("TRIM_STR2"), (int)Init.Value("TRIM_COL2"));
                //Console.WriteLine("MEAN = {0:0.00}; STDDEV = {1:0.00}",
                //    Statistics.Mean(biases[i]), Statistics.StdDev(biases[i]));
            }
            Console.WriteLine("Bias images averaging...");
            Image bias_aver = ImageCombinator.Median(biases);
            for (int i = 0; i < bias_files.Length; i++) biases[i] = null;

            // Loading and averaging of flat images;
            Console.WriteLine("Loading Flat images...");
            for (int i = 0; i < flat_files.Length; i++)
            {
                Console.WriteLine("->" + flat_files[i]);
                flats[i] = new Image();
                flats[i].LoadImage(flat_files[i]);
                if ((bool)Init.Value("TRIM_ON"))
                    flats[i] = flats[i].Trim(
                        (int)Init.Value("TRIM_STR1"), (int)Init.Value("TRIM_COL1"),
                        (int)Init.Value("TRIM_STR2"), (int)Init.Value("TRIM_COL2"));
            }
            Console.WriteLine("Flat images averaging...");
            Image flat_aver = ImageCombinator.Median(flats);
            for (int i = 0; i < flat_files.Length; i++) flats[i] = null;

            // Loading and averaging of Th-Ar images;
            Console.WriteLine("Loading Th-Ar images...");
            for (int i = 0; i < thar_files.Length; i++)
            {
                Console.WriteLine("->" + thar_files[i]);
                thars[i] = new Image();
                thars[i].LoadImage(thar_files[i]);
                if ((bool)Init.Value("TRIM_ON"))
                    thars[i] = thars[i].Trim(
                        (int)Init.Value("TRIM_STR1"), (int)Init.Value("TRIM_COL1"),
                        (int)Init.Value("TRIM_STR2"), (int)Init.Value("TRIM_COL2"));
            }
            Console.WriteLine("ThAr images averaging...");
            Image Thar_aver = ImageCombinator.Median(thars);
            for (int i = 0; i < thar_files.Length; i++) thars[i] = null;

            Console.WriteLine("Bias subtraction from averanged Flat image...");
            Image flat_aver_db = new Image();
            flat_aver_db = flat_aver - bias_aver;

            Console.WriteLine("Bias subtraction from averanged ThAr image...");
            Image Thar_aver_db = Thar_aver - bias_aver;

            //Console.WriteLine("Replasing pixels values:\r\n negative -> 1 for flat;\r\n negative -> 0 for Th-Ar;");
            for(int i=0; i<flat_aver_db.NAXIS1; i++)
                for (int j = 0; j < flat_aver_db.NAXIS2; j++)
                {
                    if (flat_aver_db[i, j] <= 0) flat_aver_db[i, j] = 1;
                    if (Thar_aver_db[i, j] < 0) Thar_aver_db[i, j] = 0;
                }

            Console.WriteLine("Search for order locations...");
            flat_aver_db.Tr();
            int polinim_degree_order = 3;
            Locator.Locate(ref flat_aver_db, polinim_degree_order);

            double[][] pos_ord = Locator.Ord_Pos;
            double[][] pos_min = Locator.Min_Pos;
            double[][] fluxes;

            // Flat spectra extraction;
            Console.WriteLine("Extraction of the flat spectra...");
            int bkg_mode = (int)Init.Value("BKG_MODE");
            int aperture_wing = (int)Init.Value("APER_WING");
            int bkg_oo = 0, bkg_ox = 0, bkg_sw = 0, bkg_step = 0;
            if (bkg_mode == 0)
            {
                bkg_sw = (int)Init.Value("BKG_SW");
                Extractor.ExtractMode1(flat_aver_db, pos_ord, pos_min, aperture_wing, bkg_sw);
            }
            if (bkg_mode == 1)
            {
                bkg_sw = (int)Init.Value("BKG_SW");
                bkg_oo = (int)Init.Value("BKG_OO");
                bkg_ox = (int)Init.Value("BKG_OX");
                bkg_step = (int)Init.Value("BKG_STEP");
                Image bkg_im = Background.Fit(flat_aver_db, pos_min, bkg_ox, bkg_oo, bkg_sw, bkg_step);
                Extractor.ExtractMode2(flat_aver_db, bkg_im, pos_ord, pos_min, aperture_wing);
            }
            
            fluxes = Extractor.FluxDebiased;
            Saver.SaveOrderedDistribution(fluxes, dir_main + "\\Flat_Orders.dat");
            Saver.SaveOrderedDistribution(Extractor.fluxes_min_init, dir_main + "\\BKG_FLAT.DAT");
            Saver.SaveOrderedDistribution(Extractor.fluxes_min_smoothed, dir_main + "\\BKG_SMOOTH_FLAT.DAT");

            // Flat spectra normalization;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Flat spectra normalization...");
            Console.ForegroundColor = ConsoleColor.White;

            Normator.Norm(fluxes, 12, 0, 13);
            Saver.SaveOrderedDistribution(Normator.OrdersNorm, dir_main + "\\Flat_Orders_Norm.dat");
            Saver.SaveOrderedDistribution(Normator.FitCurves, dir_main + "\\Flat_Orders_Fit.dat");

            // Th-Ar spectra extraction;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Extraction of the Th-Ar spectrum...");
            Console.ForegroundColor = ConsoleColor.White;
            Thar_aver_db.Tr();
            if (bkg_mode == 0)
            {
                Extractor.ExtractMode1(Thar_aver_db, pos_ord, pos_min, aperture_wing, bkg_sw);
            }
            if (bkg_mode == 1)
            {
                Image bkg_im = Background.Fit(flat_aver_db, pos_min, bkg_ox, bkg_oo, bkg_sw, bkg_step);
                Extractor.ExtractMode2(Thar_aver_db, bkg_im, pos_ord, pos_min, aperture_wing);
            }

            fluxes = Extractor.FluxDebiased;

            Saver.SaveOrderedDistribution(fluxes, dir_main + "\\thar_extacted.txt");

            // Wavelength calibration;
            double[][] lambdas;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Wavelength calibration...");
            Console.ForegroundColor = ConsoleColor.White;

            EcheData.LoadDispCurves("CalibrationCurves.dat");

            int oo = (int)Init.Value("WAVE_OO");
            int ox = (int)Init.Value("WAVE_OX");
            int iterNum = (int)Init.Value("WAVE_NINER");
            double cutLimit = (double)Init.Value("WAVE_REJ");
            double fluxLimit = (double)Init.Value("WAVE_THRESH");

            Console.WriteLine("OO = {0}; OX = {1}; Reject = {2}; IterNum = {3}; FluxLimit = {4}",
                oo, ox, cutLimit, iterNum, fluxLimit);
            WLCalibration.Calibrate(fluxes, oo, ox, cutLimit, iterNum, fluxLimit, (int)Init.Value("TRIM_STR1"));
            lambdas = WLCalibration.Lambdas;
            Saver.SaveOrderedDistribution(lambdas, dir_main + "\\lambdas.dat");

            // processing object images;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Processing object spectra...");
            Console.ForegroundColor = ConsoleColor.White;

            Image object_image = null;
            for (int i = 0; i < obj_files.Length; i++)
            {
                // Lading object image;
                Console.WriteLine("Loading Object image {0}", obj_files[i]);
                object_image = new Image();
                object_image.LoadImage(obj_files[i]);
                if ((bool)Init.Value("TRIM_ON"))
                    object_image = object_image.Trim(
                        (int)Init.Value("TRIM_STR1"), (int)Init.Value("TRIM_COL1"),
                        (int)Init.Value("TRIM_STR2"), (int)Init.Value("TRIM_COL2"));
                
                // Bias subtraction;
                Console.WriteLine("Bias subtraction...");
                object_image = object_image - bias_aver;
                for (int j = 0; j < object_image.NAXIS1; j++)
                    for (int k = 0; k < object_image.NAXIS2; k++)
                        if (object_image[j, k] < 0) object_image[j, k] = 0;

                object_image.Tr();
                // Cosmic rays removal;
                Smooth.DoSmooth(ref object_image);

                // Create directory for extracted spectra;
                string dirName = obj_files[i].Substring(0, obj_files[i].IndexOf(".fit"));
                int first = obj_files[i].LastIndexOf("\\");
                int last = obj_files[i].IndexOf(".fit");
                string fileName = dirName + "\\" + obj_files[i].Substring(first, last - first);
                Directory.CreateDirectory(dirName);

                Console.WriteLine("Extraction spectra from {0} ", obj_files[i].Substring(first));
                if (bkg_mode == 0)
                {
                    bkg_sw = (int)Init.Value("BKG_SW");
                    Extractor.ExtractMode1(object_image, pos_ord, pos_min, aperture_wing, bkg_sw);
                }
                if (bkg_mode == 1)
                {
                    Image bkg_im = Background.Fit(flat_aver_db, pos_min, bkg_ox, bkg_oo, bkg_sw, bkg_step);
                    Extractor.ExtractMode2(object_image, bkg_im, pos_ord, pos_min, aperture_wing);
                }
                fluxes = Extractor.FluxDebiased;
                //double[][] backgr = Extractor.Background;
                //Saver.SaveOrderedDistribution(backgr, fileName + "_bkg.dat");
                //Saver.SaveOrderedDistribution(fluxes, fileName + "_x.dat");
                for (int k = 0; k < fluxes.Length; k++)
                {
                    for (int j = 0; j < fluxes[0].Length; j++)
                    {
                        fluxes[k][j] = fluxes[k][j] / Normator.OrdersNorm[k][j];
                    }
                }

                Saver.SaveLambdaIntensTable(lambdas, fluxes, dirName + "\\" + "spectra_all.dat");

                for (int k = 0; k < fluxes.Length; k++)
                {
                    StreamWriter sw = new StreamWriter(fileName + 
                        string.Format("_{0:000}", k) + ".dat");
                    for (int j = 0; j < fluxes[k].Length; j++)
                    {
                        sw.WriteLine(
                            string.Format("{0:0000.0000}\t{1:0.00000E000}", 
                            lambdas[k][j], fluxes[k][j]).Replace(",", "."));
                    }
                    sw.Close();
                }
            }

            //output.Close();

        STOP:
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("End of the pipeline. Press any key to exit...");
            Console.Beep();
            Console.ReadKey();
        }
    }
}
