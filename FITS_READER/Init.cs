using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Pipeline
{
    class IIDType
    {
        public int DH = 0;
        public int MM = 0;
        public double SS = 0;
        public string ToString()
        {
            return DH.ToString() + ":" + MM.ToString() + ":" + SS.ToString();
        }
    }

    class HItem
    {
        double dd;
        string ss;
        int ii;
        IIDType iid;
        bool bb;

        string type = "";

        public HItem(string type, string str)
        {
            var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            ci.NumberFormat.NumberDecimalSeparator = ".";
            
            this.type = type;
            if (type == "double")
            {
                this.dd = double.Parse(str, ci);
            }
            if (type == "int")
            {
                this.ii = int.Parse(str);
            }
            if (type == "string")
            {
                this.ss = str;
            }
            if (type == "iid")
            {
                string[] str_mas;
                string[] delims = new string[] { " ", "\t", "h", "s", "m", ":", ",", "-" };
                str_mas = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                iid = new IIDType();
                iid.DH = int.Parse(str_mas[0]);
                iid.MM = int.Parse(str_mas[1]);
                iid.SS = double.Parse(str_mas[2], ci);
            }
            if(type=="bool")
            {
                if (str.Trim().ToLower() == "yes") bb = true;
                else bb = false;
            }
        }

        public object Value
        {
            get
            {
                if (type == "double") return dd;
                if (type == "string") return ss;
                if (type == "int") return ii;
                if (type == "iid") return iid;
                if (type == "bool") return bb;
                return null;
            }
        }
    }

    class Init
    {
        static HItem[] items;

        static string[] labels = new string[] {
        "DIR_MAIN", "DIR_BIAS", "DIR_FLAT", "DIR_CLBR", "DIR_OBJ",
        "FMASK_BIAS", "FMASK_FLAT", "FMASK_CLBR", "FMASK_OBJ",
        "TRIM_ON", "TRIM_STR1", "TRIM_COL1", "TRIM_STR2", "TRIM_COL2",
        /*"TIME_STR", "DATE_STR", "TIME_TYPE", "TIME_ZONE", "EXPO_STR",
        "OBS_LAT", "OBS_LONG", "OBS_ALT",
        "OBJ_RA", "OBJ_DEC", "OBJ_RASTR", "OBJ_DESTR", "OBJ_INFITS",*/
        "WAVE_OO", "WAVE_OX", "WAVE_NINER", "WAVE_REJ", "WAVE_THRESH",
        "BKG_MODE", "BKG_SW", "BKG_OO", "BKG_OX", "BKG_STEP",
        "APER_WING"
        };
        static string[] types = new string[] {
        "string", "string", "string", "string", "string",
        "string", "string", "string", "string",
        "bool", "int", "int", "int", "int",
        /*"string", "string", "string", "int", "string",
        "iid", "iid", "double",
        "iid", "iid", "string", "string", "bool",*/
        "int", "int", "int", "double", "double",
        "int", "int", "int", "int", "int",
        "int"
        };

        static string error_string = "";

        public static void Initialize(string path)
        {
            StreamReader sr = new StreamReader(path);
            items = new HItem[labels.Length];

            string line;
            string[] str_mas;
            string[] delims = new string[] { " ", "\t" };
            
            line = sr.ReadLine();
            int labels_count = 0;
            while (line!=null)
            {
                str_mas = line.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < labels.Length; i++)
                {
                    if (str_mas[0] == labels[i])
                    {
                        labels_count++;
                        try
                        {
                            items[i] = new HItem(types[i], str_mas[1]);
                        }
                        catch
                        {
                            error_string += string.Format("Cannot read {0} value.\r\n", labels[i]);
                        }
                        break;
                    }
                }
                line = sr.ReadLine();
            }
            if (labels_count < labels.Length)
                error_string += "Not all keys has been found.\r\n";
        }

        public static object Value(string key)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (key == labels[i])
                {
                    return items[i].Value;
                }
            }
            return null;
        }

        public static string ErrorString
        {
            get
            {
                return error_string;
            }
        }
    }
}
