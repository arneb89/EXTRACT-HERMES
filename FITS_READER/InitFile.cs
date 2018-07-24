using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class InitFile
    {
        public static string DirectoryMain;
        public static string DirectoryBias;
        public static string DirectoryFlat;
        public static string DirectoryObject;
        public static string DirectoryCalibr;
        public static string FMaskBias;
        public static string FMaskFlat;
        public static string FMaskObject;
        public static string FMaskCalibr;

        public static string ErrorString;

        private static int DescrNumber = 9;

        public static void ReadInitFile(string init_file)
        {
            StreamReader sr = new StreamReader(init_file);
            ErrorString = "";
            string str;
            string descr;
            string value;
            string[] strMas;
            int checkSum = 0;
            str=sr.ReadLine();
            do
            {
                strMas=str.Split(new string[]{" ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
                if (strMas.Length == 0) descr = "";
                else descr = strMas[0].Trim();
                switch (descr)
                {
                    case "DIR_MAIN":
                        if (strMas.Length < 2) 
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            DirectoryMain = value;
                            checkSum++;
                            if (!Directory.Exists(DirectoryMain))
                            {
                                try
                                {
                                    Directory.CreateDirectory(DirectoryMain);
                                }
                                catch
                                {
                                    ErrorString += "Cannot create main directory\r\n";
                                }
                            }
                        }
                        break;
                    case "DIR_BIAS":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            DirectoryBias = value;
                            checkSum++;
                        }
                        break;
                    case "DIR_FLAT":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            DirectoryFlat = value;
                            checkSum++;
                        }
                        break;
                    case "DIR_OBJ":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            DirectoryObject = value;
                            checkSum++;
                        }
                        break;
                    case "DIR_CLBR":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            DirectoryCalibr = value;
                            checkSum++;
                        }
                        break;
                    case "FMASK_BIAS":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            FMaskBias = value;
                            checkSum++;
                        }
                        break;
                    case "FMASK_FLAT":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            FMaskFlat = value;
                            checkSum++;
                        }
                        break;
                    case "FMASK_CLBR":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            FMaskCalibr = value;
                            checkSum++;
                        }
                        break;
                    case "FMASK_OBJ":
                        if (strMas.Length < 2)
                            ErrorString += string.Format("No value for {0}\r\n", descr);
                        else
                        {
                            value = strMas[1];
                            FMaskObject = value;
                            checkSum++;
                        }
                        break;
                }
                str = sr.ReadLine();
            } while (str != null);
            if (checkSum != DescrNumber) 
                ErrorString += "Some parameters has not been found";
        }


    }
}
