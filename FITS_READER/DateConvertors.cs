using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class DateConvertors
    {
        /// <summary>
        /// Converts Gregorian dates to Julian days
        /// </summary>
        /// <param name="yr">Year</param>
        /// <param name="mn">Month (1-12)</param>
        /// <param name="day">Day (1-31)</param>
        /// <param name="hr">Hours and fractions of hours of universal time (U.T.)</param>
        /// <returns></returns>
        public static double JDCnv(int yr, int mn, int day, double hr)
        {
            long yr1 = (long)yr, mn1 = (long)mn,  day1 = (long)day;	    // Make sure integral
            long L = (mn1 - 14) / 12;		                            // In leap years, -1 for Jan, Feb, else 0
            double julian = day1 - 32075 + 1461 * (yr1 + 4800 + L) / 4 +
                    367 * (mn1 - 2 - L * 12) / 12 - 3 * ((yr1 + 4900 + L) / 100) / 4;
            julian = (double)julian + (hr / 24.0) - 0.5;
            return julian;
        }

        /// <summary>
        /// Converts Julian dates to Gregorian calendar dates
        /// </summary>
        /// <param name="xjd">Julian date, positive double precision scalar or vector</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="ut">Hours and fractions of hours of universal time (U.T.)</param>
        public static void DayCnv(double xjd, ref int year, ref int month, ref int day, ref double ut)
        {
            // Adjustment needed because Julian day starts at noon, calendar day at midnight
            long jd = (long)xjd;            // Truncate to integral day
            double frac = xjd - jd + 0.5;   // Fractional part of calendar day
            if (frac >= 1.0)                // Is it really the next calendar day?
            {
                frac = frac - 1.0;
                jd = jd + 1;
            }

            ut = frac * 24.0;
            long l = jd + 68569;
            long n = 4 * l / 146097;
            l = l - (146097 * n + 3) / 4;
            year = (int)(4000 * (l + 1) / 1461001);
            l = l - 1461 * year / 4 + 31;        //1461 = 365.25 * 4
            month = (int)(80 * l / 2447);
            day = (int)(l - 2447 * month / 80);
            l = month / 11;
            month = (int)(month + 2 - 12 * l);
            year = (int)(100 * (n - 49) + year + l);
        }

        public static double helio_jd(double date, double ra, double dec)
        {
            double radeg = 180.0 / Math.PI;

            Precess.Correct(ref ra, ref dec, 2000.0, 1950.0);

            double delta_t = (date - 33282.42345905) / 36525.0;
            double epsilon_sec = 44.836 - 46.8495 * delta_t -
                0.00429 * delta_t * delta_t + 0.00181 * delta_t * delta_t * delta_t;
            double epsilon = (23.433333 + epsilon_sec / 3600.0) / radeg;
            double ra1 = ra / radeg;
            double dec1 = dec / radeg;

            double x = 0, y = 0, z = 0;
            XYZ(date, ref x, ref y, ref z);

            //Find extra distance light must travel in AU, multiply by 1.49598e13 cm/AU,
            //and divide by the speed of light, and multiply by 86400 second/year

            double time = -499.00522 * (Math.Cos(dec1) * Math.Cos(ra1) * x +
                 (Math.Tan(epsilon) * Math.Sin(dec1) + Math.Cos(dec1) * Math.Sin(ra1)) * y);

            return date + time / 86400.0;
        }

        private static void XYZ(double date, ref double x, ref double y, ref double z)
        {
            double picon = Math.PI / 180.0;
            double t = (date - 15020.0) / 36525.0;  //Relative Julian century from 1900

            // NOTE: longitude arguments below are given in *equinox* of date.
            //   Precess these to equinox 1950 to give everything an even footing.
            //   Compute argument of precession from equinox of date back to 1950
            double pp = (1.396041 + 0.000308 * (t + 0.5)) * (t - 0.499998);

            // Compute mean solar longitude, precessed back to 1950
            double el = 279.696678 + 36000.76892 * t + 0.000303 * t * t - pp;

            // Compute Mean longitude of the Moon
            double c = 270.434164 + 480960.0 * t + 307.883142 * t - 0.001133 * t * t - pp;

            // Compute longitude of Moon's ascending node
            double n = 259.183275 - 1800.0 * t - 134.142008 * t + 0.002078 * t * t - pp;

            // Compute mean solar anomaly
            double g = 358.475833 + 35999.04975 * t - 0.00015 * t * t;

            // Compute the mean jupiter anomaly
            double j = 225.444651 + 2880.0 * t + 154.906654 * t * t;

            // Compute mean anomaly of Venus
            double v = 212.603219 + 58320.0 * t + 197.803875 * t + 0.001286 * t * t;

            // Compute mean anomaly of Mars
            double m = 319.529425 + 19080.0 * t + 59.8585 * t + 0.000181 * t * t;

            // Convert degrees to radians for trig functions
            el = el * picon;
            g = g * picon;
            j = j * picon;
            c = c * picon;
            v = v * picon;
            n = n * picon;
            m = m * picon;

            // Calculate X,Y,Z using trigonometric series
            x = 0.999860 * Math.Cos(el)
                - 0.025127 * Math.Cos(g - el)
                + 0.008374 * Math.Cos(g + el)
                + 0.000105 * Math.Cos(g + g + el)
                + 0.000063 * t * Math.Cos(g - el)
                + 0.000035 * Math.Cos(g + g - el)
                - 0.000026 * Math.Sin(g - el - j)
                - 0.000021 * t * Math.Cos(g + el)
                + 0.000018 * Math.Sin(2.0 * g + el - 2.0 * v)
                + 0.000017 * Math.Cos(c)
                - 0.000014 * Math.Cos(c - 2.0 * el)
                + 0.000012 * Math.Cos(4.0 * g + el - 8.0 * m + 3.0 * j)
                - 0.000012 * Math.Cos(4.0 * g - el - 8.0 * m + 3.0 * j)
                - 0.000012 * Math.Cos(g + el - v)
                + 0.000011 * Math.Cos(2.0 * g + el - 2.0 * v)
                + 0.000011 * Math.Cos(2.0 * g - el - 2.0 * j);


            y = 0.917308d * Math.Sin(el)
                + 0.023053d * Math.Sin(g - el)
                + 0.007683d * Math.Sin(g + el)
                + 0.000097d * Math.Sin(g + g + el)
                - 0.000057d * t * Math.Sin(g - el)
                - 0.000032d * Math.Sin(g + g - el)
                - 0.000024d * Math.Cos(g - el - j)
                - 0.000019d * t * Math.Sin(g + el)
                - 0.000017d * Math.Cos(2.0 * g + el - 2.0 * v)
                + 0.000016d * Math.Sin(c)
                + 0.000013d * Math.Sin(c - 2.0 * el)
                + 0.000011d * Math.Sin(4.0 * g + el - 8.0 * m + 3.0 * j)
                + 0.000011d * Math.Sin(4.0 * g - el - 8.0 * m + 3.0 * j)
                - 0.000011d * Math.Sin(g + el - v)
                + 0.000010d * Math.Sin(2.0 * g + el - 2.0 * v)
                - 0.000010d * Math.Sin(2.0 * g - el - 2.0 * j);


            z = 0.397825 * Math.Sin(el)
                + 0.009998 * Math.Sin(g - el)
                + 0.003332 * Math.Sin(g + el)
                + 0.000042 * Math.Sin(g + g + el)
                - 0.000025 * t * Math.Sin(g - el)
                - 0.000014 * Math.Sin(g + g - el)
                - 0.000010 * Math.Cos(g - el - j);
        }
    }
}
