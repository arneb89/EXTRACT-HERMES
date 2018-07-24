using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using nom.tam.fits;
using nom.tam.util;
using nom.tam.image;

namespace Pipeline
{
    class Image
    {
        private int bitPix = 0;
        private int naxis = 0;
        private int rowsCount = 0;
        private int columnsCount = 0;
        private double bscale = 1;
        private double bzero = 0;
        private string imageType = null;

        private double[][] vals = null;

        public Image()
        {
            this.naxis = 0;
            this.rowsCount = 0;
            this.columnsCount = 0;
            this.vals = null;
        }

        public Image(int naxis1, int naxis2)
        {
            this.rowsCount = naxis1;
            this.columnsCount = naxis2;
            this.vals = new double[naxis1][];
            for (int i = 0; i < naxis1; i++)
            {
                this.vals[i] = new double[naxis2];
            }
        }

        public int NAXIS1
        {
            get
            {
                return this.rowsCount;
            }
        }

        public int NAXIS2
        {
            get
            {
                return this.columnsCount;
            }
        }

        public double this[int index1, int index2]
        {
            get
            {
                return this.vals[index1][index2];
            }
            set
            {
                this.vals[index1][index2] = value;
            }
        }

        public void LoadImage(string path)
        {
            Fits fits = new Fits(path);
            ImageHDU hdu = GetImageHDU(fits);
            int axis_num = 0;
            if (hdu.Axes.Length == 2)
            {
                axis_num = 2;
                this.rowsCount = hdu.Axes[0];
                this.columnsCount = hdu.Axes[1];
            }
            if (hdu.Axes.Length == 3)
            {
                axis_num = 3;
                this.rowsCount = hdu.Axes[1];
                this.columnsCount = hdu.Axes[2];
            }
            this.bscale = hdu.BScale;
            this.bzero = hdu.BZero;
            this.vals = new double[rowsCount][];
            for (int i = 0; i < rowsCount; i++)
                this.vals[i] = new double[columnsCount];
            GetImageData(hdu, axis_num);
            fits.Stream.Close();
        }

        public static ImageHDU GetImageHDU(Fits fits)
        {
            int i = 0;

            for (BasicHDU hdu = fits.getHDU(i); hdu != null; ++i)
            {
                if (hdu is ImageHDU)
                {
                    return (ImageHDU)hdu;
                }
            }

            return null;
        }

        public void GetImageData(ImageHDU hdu, int axis_num)
        {
            //double[,] result = new double[hdu.Axes[1], hdu.Axes[0]];
            Array[] a = null;
            
            if(axis_num==2)
                a = (Array[])hdu.Data.DataArray;
            if (axis_num == 3)
            {
                Array[] bb = (Array[])hdu.Data.DataArray;
                a = (Array[])bb[0];
            }

            switch (hdu.BitPix)
            {
                case 8:
                    {
                        byte[] b = new byte[columnsCount];
                        for (int y = 0; y < rowsCount; ++y)
                        {
                            a[y].CopyTo(b, 0);
                            for (int x = 0; x < columnsCount; ++x)
                            {
                                this.vals[y][x] = bzero + bscale * (double)b[x];
                            }
                        }
                    }
                    break;
                case 16:
                    {
                        short[] b = new short[columnsCount];

                        for (int y = 0; y < rowsCount; ++y)
                        {
                            a[y].CopyTo(b, 0);

                            for (int x = 0; x < columnsCount; ++x)
                            {
                                this.vals[y][x] = bzero + bscale * (double)b[x];
                            }
                        }
                    }
                    break;
                case 32:
                    {
                        int[] b = new int[columnsCount];
                        for (int y = 0; y < rowsCount; ++y)
                        {
                            a[y].CopyTo(b, 0);
                            for (int x = 0; x < columnsCount; ++x)
                            {
                                this.vals[y][x] = bzero + bscale * (double)b[x];
                            }
                        }
                    }
                    break;
                case -32:
                    {
                        float[] b = new float[columnsCount];
                        for (int y = 0; y < rowsCount; ++y)
                        {
                            a[y].CopyTo(b, 0);
                            for (int x = 0; x < columnsCount; ++x)
                            {
                                this.vals[y][x] = bzero + bscale * (double)b[x];
                            }
                        }
                    }
                    break;
                case -64:
                    {
                        double[] b = new double[columnsCount];
                        for (int y = 0; y < rowsCount; ++y)
                        {
                            a[y].CopyTo(b, 0);
                            for (int x = 0; x < columnsCount; ++x)
                            {
                                this.vals[y][x] = bzero + bscale * (double)b[x];
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Data type is not supported.");
            }
        }

        public void Tr()
        {
            double[][] vals_new = new double[this.columnsCount][];
            for (int i = 0; i < this.columnsCount; i++) 
                vals_new[i] = new double[this.rowsCount];

            for (int i = 0; i < this.rowsCount; i++)
            {
                for (int j = 0; j < this.columnsCount; j++)
                {
                    vals_new[j][i] = this.vals[i][j];
                }
            }

            this.vals = vals_new;
            int buff = this.rowsCount;
            this.rowsCount = this.columnsCount;
            this.columnsCount = buff;
        }


        /// <summary>
        /// Returns the subimage;
        /// </summary>
        /// <param name="str1">The first row in the subimage (included into subimage, indexing from 0)</param>
        /// <param name="col1">The first column in the subimage (included into subimage, indexing from 0)</param>
        /// <param name="str2">The last row in the subimage (included into subimage, indexing from 0)</param>
        /// <param name="col2">The last column in the subimage (included into subimage, indexing from 0)</param>
        /// <returns></returns>
        public Image Trim(int str1, int col1, int str2, int col2)
        {
            Image res = new Image(str2 - str1 + 1, col2 - col1 + 1);
            for (int i = 0; i < res.NAXIS1; i++)
            {
                for (int j = 0; j < res.NAXIS2; j++)
                {
                    res.vals[i][j] = this.vals[i + str1][j + col1];
                }
            }
            return res;
        }


        //public void LoadImage(string path)
        //{
        //    char[] hdu_symbols = new char[80];
        //    int hdu_number;
        //    StreamReader sr = new StreamReader(path);
        //    string str = null;
        //    hdu_number = 0;
        //    do
        //    {
        //        str = null;
        //        int n = sr.Read(hdu_symbols, 0, 80);
        //        for (int i = 0; i < hdu_symbols.Length; i++)
        //        {
        //            str += hdu_symbols[i];
        //        }
        //        this.CheckHDU(str);
        //        hdu_number++;
        //    } while (str.Substring(0, 8).Trim().ToLower() != "end");

        //    int header_length = hdu_number * 80;
        //    int nb;
        //    if (header_length % 2880 == 0) // может можно проще?
        //    {
        //        nb = header_length;
        //    }
        //    else
        //    {
        //        nb = (header_length / 2880 + 1) * 2880;
        //    }
        //    sr.Close();


        //    BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open));

        //    br.ReadBytes(nb);

        //    this.vals = new double[this.naxis1][];
        //    for (int i = 0; i < this.naxis1; i++) this.vals[i] = new double[this.naxis2];

        //    // Reading image;

        //    if (this.bitPix == -32)
        //    {
        //        Byte[] bytes = new byte[4];
        //        for (int i = 0; i < this.naxis1; i++)
        //        {
        //            for (int j = 0; j < this.naxis2; j++)
        //            {
        //                br.Read(bytes, 0, 4);
        //                Array.Reverse(bytes);
        //                this.vals[i][j] = (double)BitConverter.ToSingle(bytes, 0);
        //            }
        //        }
        //        if (bzero != 0 || bscale != 1)
        //        {
        //            for (int i = 0; i < this.naxis1; i++)
        //            {
        //                for (int j = 0; j < this.naxis2; j++)
        //                {
        //                    this.vals[i][j] = this.bscale * this.vals[i][j] + this.bscale;
        //                }
        //            }
        //        }
        //    }
        //    if (this.bitPix == 16)
        //    {
        //        Byte[] bytes = new byte[2];
        //        for (int i = 0; i < this.naxis1; i++)
        //        {
        //            for (int j = 0; j < this.naxis2; j++)
        //            {
        //                br.Read(bytes, 0, 2);
        //                Array.Reverse(bytes);
        //                short sh;
        //                sh = BitConverter.ToInt16(bytes, 0);
        //                this.vals[i][j] = (double)sh;
        //            }
        //        }
        //        if (bzero == 0)
        //        {
        //            for (int i = 0; i < this.naxis1; i++)
        //            {
        //                for (int j = 0; j < this.naxis2; j++)
        //                {
        //                    if(this.vals[i][j]<0)
        //                        this.vals[i][j] = this.vals[i][j] + 65536;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < this.naxis1; i++)
        //            {
        //                for (int j = 0; j < this.naxis2; j++)
        //                {
        //                    this.vals[i][j] = this.bscale * this.vals[i][j] + this.bzero;
        //                }
        //            }
        //        }
        //    }

        //    br.Close();
        //}

        //private void CheckHDU(string str)
        //{
        //    System.Globalization.NumberFormatInfo ni = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };
        //    string strDescript = str.Substring(0, 8).Trim().ToLower();
        //    int slashPosition = str.IndexOf("/");
        //    string strValue = "";
        //    if (slashPosition != -1)
        //    {
        //        strValue = str.Substring(10, slashPosition - 10);
        //    }
        //    else
        //    {
        //        strValue = str.Substring(10);
        //    }

        //    switch (strDescript)
        //    {
        //        case "bitpix":
        //            this.bitPix = int.Parse(strValue.Trim());
        //            break;
        //        case "naxis":
        //            this.naxis = int.Parse(strValue.Trim());
        //            break;
        //        case "naxis1":
        //            this.naxis1 = int.Parse(strValue.Trim());
        //            break;
        //        case "naxis2":
        //            this.naxis2 = int.Parse(strValue.Trim());
        //            break;
        //        case "imagetyp":
        //            this.imageType = strValue.Trim();
        //            break;
        //        case "bscale":
        //            this.bscale = double.Parse(strValue.Trim(), ni);
        //            break;
        //        case "bzero":
        //            this.bzero = double.Parse(strValue.Trim(), ni);
        //            break;
        //    }
        //}

        public double[] GetColumn(int col_index)
        {
            double[] column = new double[this.rowsCount];
            for (int i = 0; i < this.rowsCount; i++)
            {
                column[i] = this.vals[i][col_index];
            }
            return column;
        }

        public double[] GetRow(int row_index)
        {
            double[] row = new double[this.columnsCount];
            for (int i = 0; i < this.columnsCount; i++)
            {
                row[i] = this.vals[row_index][i];
            }
            return row;
        }

        public static Image operator +(Image im1, Image im2)
        {
            Image imRes = new Image(im1.rowsCount, im2.columnsCount);

            for (int i = 0; i < imRes.rowsCount; i++)
            {
                for (int j = 0; j < imRes.columnsCount; j++)
                {
                    imRes[i, j] = im1[i, j] + im2[i, j];
                }
            }
            return imRes;
        }

        public static void Save(string strFile)
        {
            
            float[][] x = new float[30][];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = new float[30];
                for (int j = 0; j < x[i].Length; j++) x[i][j] = 3;
            }
            
            bool t = typeof(Array).Equals(x.GetType());

            Fits f = new Fits();
            Header hdr=new Header();
            hdr.Bitpix=8;
            hdr.Simple=true;
            hdr.Naxes=2;
            hdr.AddValue("NAXIS1", 30, "");
            hdr.AddValue("NAXIS2", 30, "");
            
            FitsFactory.UseAsciiTables = false;
            
            Data d = FitsFactory.DataFactory(hdr);
            
            
            
            BasicHDU h = FitsFactory.HDUFactory(x);
            
           
            
            f.addHDU(h);

            FileStream os = new FileStream(strFile, FileMode.Create);

            f.Write(os);

        }

        public static Image operator -(Image im1, Image im2)
        {
            Image imRes = new Image(im1.rowsCount, im2.columnsCount);

            for (int i = 0; i < imRes.rowsCount; i++)
            {
                for (int j = 0; j < imRes.columnsCount; j++)
                {
                    imRes[i, j] = im1[i, j] - im2[i, j];
                }
            }
            return imRes;
        }

        public static Image operator /(Image im1, Image im2)
        {
            Image imRes = new Image(im1.rowsCount, im2.columnsCount);

            for (int i = 0; i < imRes.rowsCount; i++)
            {
                for (int j = 0; j < imRes.columnsCount; j++)
                {
                    imRes[i, j] = im1[i, j] / im2[i, j];
                }
            }
            return imRes;
        }

        public static Image operator *(Image im1, Image im2)
        {
            Image imRes = new Image(im1.rowsCount, im2.columnsCount);

            for (int i = 0; i < imRes.rowsCount; i++)
            {
                for (int j = 0; j < imRes.columnsCount; j++)
                {
                    imRes[i, j] = im1[i, j] * im2[i, j];
                }
            }
            return imRes;
        }
    }
}
