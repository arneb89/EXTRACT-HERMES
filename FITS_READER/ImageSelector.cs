using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class ImageSelector
    {
        static public string[] GetFilesByHDU(string pathToFolder, string searchPattern, string hdu_string, string hdu_value)
        {
            string[] files = Directory.GetFiles(pathToFolder, searchPattern);

            if (hdu_value == "*")
                return files;

            string[] right_files0 = new string[files.Length];
            
            char[] hdu_symbols = new char[80];
            int right_files_number = 0;
            int ii = 0;

            for (int i = 0; i < files.Length; i++)
            {
                StreamReader sr = new StreamReader(files[i]);
                string str = null;
                int line = 0;
                do
                {
                    str = null;
                    
                    int n = sr.Read(hdu_symbols, 0, 80);
                    line++;
                    for (int j = 0; j < hdu_symbols.Length; j++)
                    {
                        str += hdu_symbols[j];
                    }
                    if (line == 1 && str.Substring(0, 8).Trim().ToLower() != "simple")
                    {
                        break;
                    }

                    string strDescript = str.Substring(0, 8).Trim().ToLower();
                    int slashPosition = str.IndexOf("/");
                    string strValue = "";
                    if (slashPosition != -1)
                    {
                        strValue = str.Substring(10, slashPosition - 10);
                    }
                    else
                    {
                        strValue = str.Substring(10);
                    }

                    if (strDescript == hdu_string && strValue.Replace("'", "").Trim() == hdu_value)
                    {
                        right_files_number++;
                        right_files0[ii] = files[i];
                        ii++;
                        break;
                    }
                } while (str.Substring(0, 8).Trim().ToLower() != "end");
                sr.Close();
            }

            string[] rightFiles = new string[right_files_number];
            for (int i = 0; i < right_files_number; i++)
            {
                rightFiles[i] = right_files0[i];
            }
            return rightFiles;
        }
    }
}
