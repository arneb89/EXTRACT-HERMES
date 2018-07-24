using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class Precess
    {
        public static void Correct(ref double ra, ref double dec, 
             double equinox1,  double equinox2)
        {        
            double ra_rad = ra * Math.PI / 180.0;
            double dec_rad = dec * Math.PI / 180.0;

            double a = Math.Cos(dec_rad);

            double[] x = new double[] { 
                a * Math.Cos(ra_rad), a * Math.Sin(ra_rad), Math.Sin(dec_rad) };

            double[,] r = premat(equinox1, equinox2, true);

            double[] x2 = new double[3];

            for (int i = 0; i < 3; i++)
            {
                double sum = 0;
                for (int k = 0; k < 3; k++)
                {
                    sum += r[k, i] * x[k];
                }
                x2[i] = sum;
            }

            ra_rad = atan(x2[1], x2[0]);
            dec_rad = Math.Asin(x2[2]);

            ra = ra_rad * 180.0 / Math.PI;
            if (ra < 0) ra = 360.0 - ra;
            dec = dec_rad * 180.0 / Math.PI;
        }

        public static void FromJ2000ToJ1950(double ra, double dec, ref double ra_1950, ref double dec_1950)
        {
         //   double radeg = 180.0/Math.PI;
         //   double sec_to_radian = 1.0/radeg/3600.0;

         //   double[,] M = new double[]{ {+0.9999256795, -0.0111814828, -0.0048590040,  
         //-0.000551,  -0.238560,     +0.435730     }, 
         //{ +0.0111814828, +0.9999374849, -0.0000271557,   
         //+0.238509,     -0.002667,      -0.008541     }, 
         //{ +0.0048590039, -0.0000271771, +0.9999881946 , 
         //-0.435614,      +0.012254,      +0.002117      }, 
         //{ -0.00000242389840, +0.00000002710544, +0.00000001177742, 
         //+0.99990432,    -0.01118145,    -0.00485852    }, 
         //{ -0.00000002710544, -0.00000242392702, +0.00000000006585, 
         //+0.01118145,     +0.99991613,    -0.00002716    }, 
         //{ -0.00000001177742, +0.00000000006585,-0.00000242404995, 
         //+0.00485852,   -0.00002717,    +0.99996684} };

         //   double[] A_dot = new double[]{ 1.244, -1.579, -0.660 };            //in arc seconds per century
         //   for(int i=0; i<A_dot.Length; i++) A_dot[i]=1e-3*A_dot[i]; 

         //   double ra_rad = ra/radeg; dec_rad = dec/radeg;
         //   double cosra =  Math.Cos( ra_rad ); sinra = Math.Sin( ra_rad );
         //   double cosdec = Math.Cos( dec_rad ); sindec = Math.Sin( dec_rad );

         //   double dec_1950 = dec*0.0;
         //   double ra_1950 = ra*0.0;

         //   for (int i = 0; i <= N - 1; i++)
         //   {

                // Following statement moved inside loop in Feb 2000.

                //double[] A = new double[] { -1.62557, -0.31919, -0.13843 };
                //for (int k = 0; k < A.Length; k++) A[k] = A[k] * 1e-6; // ;in radians


                //double[] r0 = new double[] { cosra[i] * cosdec[i], sinra[i] * cosdec[i], sindec[i] };

                //if keyword_set(mu_radec) then begin

                //mu_a = mu_radec[ 0, i ]
                //mu_d = mu_radec[ 1, i ]
                //r0_dot = [ -mu_a*sinra[i]*cosdec[i] - mu_d*cosra[i]*sindec[i] , $ ;Velocity vector
                //            mu_a*cosra[i]*cosdec[i] - mu_d*sinra[i]*sindec[i] , $
                //            mu_d*cosdec[i] ] + 21.095d * rad_vel[i] * parallax[i] * r0

                //endif else 

                //     double[] r0_dot = new double[]{0.0, 0.0, 0.0};

                //     double[] R_0 = new double[]{ r0, r0_dot };
                //  R_1 =  M # R_0

                // ; Include the effects of the E-terms of aberration to form r and r_dot.

                // r1 = R_1[0:2]  
                // r1_dot = R_1[3:5] 

                // if ~keyword_set(Mu_radec) then begin
                //        r1 = r1 + sec_to_radian * r1_dot * (epoch - 1950.0d)/100.
                //        A = A + sec_to_radian * A_dot * (epoch - 1950.0d)/100.
                // endif

                // x1 = R_1[0]   &   y1 = R_1[1]    &  z1 = R_1[2]
                // rmag = sqrt( x1^2 + y1^2 + z1^2 )


                // s1 = r1/rmag    & s1_dot = r1_dot/rmag

                // s = s1
                // for j = 0,2 do begin
                //    r = s1 + A - (total(s * A))*s
                //    s = r/rmag
                // endfor 
                // x = r[0]          & y = r[1]     &  z = r[2]  
                // r2 = x^2 + y^2 + z^2
                // rmag = sqrt( r2 )

                // if keyword_set(Mu_radec) then begin
                //         r_dot = s1_dot + A_dot - ( total( s * A_dot))*s
                //         x_dot = r_dot[0]  & y_dot= r_dot[1]  &  z_dot = r_dot[2]
                //         mu_radec[0,i] = ( x*y_dot - y*x_dot) / ( x^2 + y^2)
                //         mu_radec[1,i] = ( z_dot* (x^2 + y^2) - z*(x*x_dot + y*y_dot) ) /  $
                //                     ( r2*sqrt( x^2 + y^2) )
                // endif

                // dec_1950[i] = asin( z / rmag)
                // ra_1950[i] = atan( y, x)

                //  if parallax[i] GT 0. then begin
                //      rad_vel[i] = ( x*x_dot + y*y_dot + z*z_dot )/ (21.095*Parallax[i]*rmag)
                //      parallax[i] = parallax[i] / rmag
                //  endif
                // }

                // neg = where( ra_1950 LT 0, NNeg )
                // if Nneg GT 0 then ra_1950[neg] = ra_1950[neg] + 2.D*!DPI

                // ra_1950 = ra_1950*radeg & dec_1950 = dec_1950*radeg

                //; Make output scalar if input was scalar

                // sz = size(ra)
                // if sz[0] EQ 0 then begin
                //        ra_1950 = ra_1950[0]     &      dec_1950 = dec_1950[0]
                // endif
            //}
        }

        private static double[,] premat(double equinox1, double equinox2, bool FK4)
        {
            double deg_to_rad = Math.PI / 180.0;
            double sec_to_rad = deg_to_rad / 3600.0;
            double t = 0.001 * (equinox2 - equinox1);
            double a, b, c;
            double st = 0;
            if (!FK4)
            {
                st = 0.001 * (equinox1 - 2000.0);
                //  Compute 3 rotation angles
                a = sec_to_rad * t * (23062.181 + st * (139.656 + 0.0139 * st)
                    + t * (30.188 - 0.344 * st + 17.998 * t));
                b = sec_to_rad * t * t * (79.280 + 0.410 * st + 0.205 * t) + a;
                c = sec_to_rad * t * (20043.109 - st * (85.33 + 0.217 * st)
                    + t * (-42.665 - 0.217 * st - 41.833 * t));
            }
            else
            {
                st = 0.001 * (equinox1 - 1900.0);
                //  Compute 3 rotation angles
                a = sec_to_rad * t * (23042.53 + st * (139.75 + 0.06 * st)
                    + t * (30.23 - 0.27 * st + 18.0 * t));
                b = sec_to_rad * t * t * (79.27 + 0.66 * st + 0.32 * t) + a;
                c = sec_to_rad * t * (20046.85 - st * (85.33 + 0.37 * st)
                    + t * (-42.67 - 0.37 * st - 41.8 * t));
            }

            double sina = Math.Sin(a); double sinb = Math.Sin(b); double sinc = Math.Sin(c);
            double cosa = Math.Cos(a); double cosb = Math.Cos(b); double cosc = Math.Cos(c);

            double[,] r = new double[3, 3];
            r[0, 0] = cosa * cosb * cosc - sina * sinb; r[0, 1] = sina * cosb + cosa * sinb * cosc; r[0, 2] = cosa * sinc;
            r[1, 0] = -cosa * sinb - sina * cosb * cosc; r[1, 1] = cosa * cosb - sina * sinb * cosc; r[1, 2] = -sina * sinc;
            r[2, 0] = -cosb * sinc; r[2, 1] = -sinb * sinc; r[2, 2] = cosc;

            return r;
        }

        private static double atan(double y, double x)
        {
            double res = 0;
            if (x > 0 && y >= 0) res = Math.Atan(y / x);
            if (x < 0 && y >= 0) res = Math.PI - Math.Atan(y / x);
            if (x < 0 && y < 0) res = Math.PI + Math.Atan(y / x);
            if (x > 0 && y < 0) res = 2 * Math.PI - Math.Atan(y / x);
            if (x == 0 && y > 0) res = 0.5 * Math.PI;
            if (x == 0 && y < 0) res = 1.5 * Math.PI;
            return res;
        }
    }
}
