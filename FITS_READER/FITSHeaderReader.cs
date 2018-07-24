using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pipeline
{
    class FITSHeaderReader
    {
        public string errorString = "";

        public string Read(string file, string label)
        {
            char[] hdu_symbols = new char[80];
            label = label.Trim().ToLower();
            string strValue = "";
            StreamReader sr = new StreamReader(file);
            string str = null;

            do
            {
                // read a string of length 80;
                str = null;
                int n = sr.Read(hdu_symbols, 0, 80);
                for (int j = 0; j < hdu_symbols.Length; j++)
                {
                    str += hdu_symbols[j];
                }

                // read descriptor and its value;
                string strDescript = str.Substring(0, 8).Trim().ToLower();
                int slashPosition = str.IndexOf("/");
                
                if (slashPosition != -1)
                {
                    strValue = str.Substring(10, slashPosition - 10);
                }
                else
                {
                    strValue = str.Substring(10);
                }
                if (strDescript == label)
                {
                    strValue = strValue.Replace("'", "");
                    return strValue;
                }

            } while (str.Substring(0, 8).Trim().ToLower() != "end");
            sr.Close();

            errorString += string.Format("No line with label {0} has been found in {1}", 
                label, file);

            return strValue;
        }

        public IIDType ReadAsIID(string file, string label)
        {
            string val=this.Read(file, label);
            HItem item = new HItem("iid", val);
            return (IIDType)item.Value;
        }

        public double ReadAsDouble(string file, string label)
        {
            string val = this.Read(file, label);
            HItem item = new HItem("double", val);
            return (double)item.Value;
        }

        public string ErrorString
        {
            get
            {
                return errorString;
            }
        }
    }
}
