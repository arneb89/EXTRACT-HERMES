using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    class HelCorr
    {

        public static void DoCorrection(double obs_long,
            double obs_lat, double obs_alt, double ra2000, double dec2000, double jd, 
            ref double vdiurnal, ref double vbar, ref double vhel)
        {
            double xjd;
            int year = 0, month = 0, day = 0;
            double ut = 0;
            // covert JD to Gregorian calendar date;
            xjd = 2400000.0+jd; 
            DateConvertors.DayCnv(xjd, ref year, ref month, ref day, ref ut);
            // current epoch
            double epoch = year + month / 12.0 + day / 365.0;
            // precess ra2000 and dec2000 to current epoch, resulting ra is in degrees
            double ra = ra2000 * 15.0;
            double dec = dec2000;
            precess(ref ra, ref dec, 2000.0, epoch);
            // calculate heliocentric julian date
            double hjd = DateConvertors.helio_jd(jd, ra, dec);
            // DIURNAL VELOCITY (see IRAF task noao.astutil.rvcorrect)
            // convert geodetic latitude into geocentric latitude to correct
            // for rotation of earth
            double RADEG=180.0/Math.PI;
            double dlat = -(11.0 * 60.0 + 32.743) * Math.Sin(2 * obs_lat / RADEG)
                   + 1.1633 * Math.Sin(4 * obs_lat / RADEG) - 0.0026 * Math.Sin(6 * obs_lat / RADEG);
            double lat = obs_lat + dlat / 3600;

            // calculate distance of observer from earth center
            double r = 6378160.0 * (0.998327073 + 0.001676438 * Math.Cos(2 * lat / RADEG)
                - 0.00000351 * Math.Cos(4 * lat / RADEG) + 0.000000008 * Math.Cos(6 * lat / RADEG))
                + obs_alt;

            // calculate rotational velocity (perpendicular to the radius vector) in km/s
            // 23.934469591229 is the siderial day in hours for 1986
            double v = 2.0 * Math.PI * (r / 1000.0) / (23.934469591229 * 3600.0);

            // calculating local mean siderial time (see astronomical almanach)
            double tu=(jd-51545.0)/36525;
            double gmst = 6.697374558 + ut +
                (236.555367908 * (jd - 51545.0) + 0.093104 * tu * tu - 6.2e-6 * tu * tu * tu) / 3600;
            double lmst=(gmst-obs_long/15) % 24.0;

            // projection of rotational velocity along the line of sight
            vdiurnal=v*Math.Cos(lat/RADEG )*Math.Cos(dec/RADEG )*Math.Sin((ra-lmst*15)/RADEG );

            // BARICENTRIC and HELIOCENTRIC VELOCITIES
            double[] vh=new double[3];
            double[] vb=new double[3];  
            baryvel(xjd,0,ref vh, ref vb);

            // project to line of sight
            vbar=vb[0]*Math.Cos(dec/RADEG )*Math.Cos(ra/RADEG )+vb[1]*Math.Cos(dec/RADEG )*Math.Sin(ra/RADEG )
                +vb[2]*Math.Sin(dec/RADEG);
            vhel=vh[0]*Math.Cos(dec/RADEG )*Math.Cos(ra/RADEG )+vh[1]*Math.Cos(dec/RADEG )*Math.Sin(ra/RADEG ) 
                +vh[2]*Math.Sin(dec/RADEG);
        }
        // ТОЛЬКО ДЛЯ deq=0!!!
        private static void baryvel(double dje,double deq,ref double[] dvelh,ref double[] dvelb)
        {
            //Define constants
            double dc2pi = 2 * Math.PI;
            double cc2pi = 2 * Math.PI;
            double dc1 = 1.0;
            double dcto = 2415020.00;
            double dcjul = 36525.00;                     //days in Julian year
            double dcbes = 0.3130;
            double dctrop = 365.24219572;               //days in tropical year (...572 insig)
            double dc1900 = 1900.00;
            double AU = 1.4959787e8;

            //Constants dcfel(i,k) of fast changing elements.
            double[] dcfel1 = new double[]
                    {1.7400353e00, 6.2833195099091e02,  5.2796e-6,
                    6.2565836e00, 6.2830194572674e02, -2.6180e-6,
                    4.7199666e00, 8.3997091449254e03, -1.9780e-5,
                    1.9636505e-1, 8.4334662911720e03, -5.6044e-5,
                    4.1547339e00, 5.2993466764997e01,  5.8845e-6,
                    4.6524223e00, 2.1354275911213e01,  5.6797e-6,
                    4.2620486e00, 7.5025342197656e00,  5.5317e-6, 
                    1.4740694e00, 3.8377331909193e00,  5.6093e-6};
            double[,] dcfel = reform(dcfel1, 3, 8);

            //constants dceps and ccsel(i,k) of slowly changing elements.
            double[] dceps = new double[] { 4.093198e-1, -2.271110e-4, -2.860401e-8 };
            double[] ccsel1 = new double[] {1.675104E-2, -4.179579E-5, -1.260516E-7
                ,2.220221E-1,  2.809917E-2,  1.852532E-5
                ,1.589963E00,  3.418075E-2,  1.430200E-5 
                ,2.994089E00,  2.590824E-2,  4.155840E-6 
                ,8.155457E-1,  2.486352E-2,  6.836840E-6 
                ,1.735614E00,  1.763719E-2,  6.370440E-6 
                ,1.968564E00,  1.524020E-2, -2.517152E-6 
                ,1.282417E00,  8.703393E-3,  2.289292E-5 
                ,2.280820E00,  1.918010E-2,  4.484520E-6 
                ,4.833473E-2,  1.641773E-4, -4.654200E-7 
                ,5.589232E-2, -3.455092E-4, -7.388560E-7 
                ,4.634443E-2, -2.658234E-5,  7.757000E-8 
                ,8.997041E-3,  6.329728E-6, -1.939256E-9 
                ,2.284178E-2, -9.941590E-5,  6.787400E-8 
                ,4.350267E-2, -6.839749E-5, -2.714956E-7 
                ,1.348204E-2,  1.091504E-5,  6.903760E-7 
                ,3.106570E-2, -1.665665E-4, -1.590188E-7 };
            double[,] ccsel = reform(ccsel1, 3, 17);

            // Constants of the arguments of the short-period perturbations.
            double[] dcargs1 = new double[]
                {5.0974222e0, -7.8604195454652e2
                ,3.9584962e0, -5.7533848094674e2 
                ,1.6338070e0, -1.1506769618935e3 
                ,2.5487111e0, -3.9302097727326e2 
                ,4.9255514e0, -5.8849265665348e2 
                ,1.3363463e0, -5.5076098609303e2 
                ,1.6072053e0, -5.2237501616674e2 
                ,1.3629480e0, -1.1790629318198e3 
                ,5.5657014e0, -1.0977134971135e3 
                ,5.0708205e0, -1.5774000881978e2 
                ,3.9318944e0,  5.2963464780000e1 
                ,4.8989497e0,  3.9809289073258e1 
                ,1.3097446e0,  7.7540959633708e1 
                ,3.5147141e0,  7.9618578146517e1 
                ,3.5413158e0, -5.4868336758022e2 };
            double[,] dcargs = reform(dcargs1, 2, 15);

            // Amplitudes ccamps(n,k) of the short-period perturbations.
            double[] ccamps1 = new double[] 
                {-2.279594E-5,  1.407414E-5,  8.273188E-6,  1.340565E-5, -2.490817E-7 
                ,-3.494537E-5,  2.860401E-7,  1.289448E-7,  1.627237E-5, -1.823138E-7 
                , 6.593466E-7,  1.322572E-5,  9.258695E-6, -4.674248E-7, -3.646275E-7 
                , 1.140767E-5, -2.049792E-5, -4.747930E-6, -2.638763E-6, -1.245408E-7 
                , 9.516893E-6, -2.748894E-6, -1.319381E-6, -4.549908E-6, -1.864821E-7 
                , 7.310990E-6, -1.924710E-6, -8.772849E-7, -3.334143E-6, -1.745256E-7 
                ,-2.603449E-6,  7.359472E-6,  3.168357E-6,  1.119056E-6, -1.655307E-7 
                ,-3.228859E-6,  1.308997E-7,  1.013137E-7,  2.403899E-6, -3.736225E-7 
                , 3.442177E-7,  2.671323E-6,  1.832858E-6, -2.394688E-7, -3.478444E-7 
                , 8.702406E-6, -8.421214E-6, -1.372341E-6, -1.455234E-6, -4.998479E-8 
                ,-1.488378E-6, -1.251789E-5,  5.226868E-7, -2.049301E-7,  0.0E0 
                ,-8.043059E-6, -2.991300E-6,  1.473654E-7, -3.154542E-7,  0.0E0 
                , 3.699128E-6, -3.316126E-6,  2.901257E-7,  3.407826E-7,  0.0E0 
                , 2.550120E-6, -1.241123E-6,  9.901116E-8,  2.210482E-7,  0.0E0 
                ,-6.351059E-7,  2.341650E-6,  1.061492E-6,  2.878231E-7,  0.0E0 };
            double[,] ccamps = reform(ccamps1, 5, 15);

            // Constants csec3 and ccsec(n,k) of the secular perturbations in longitude.
            double ccsec3 = -7.757020E-8;
            double[] ccsec1 = new double[]
                {1.289600E-6, 5.550147E-1, 2.076942E00 
                ,3.102810E-5, 4.035027E00, 3.525565E-1 
                ,9.124190E-6, 9.990265E-1, 2.622706E00 
                ,9.793240E-7, 5.508259E00, 1.559103E01 };
            double[,] ccsec = reform(ccsec1, 3, 4);

            // Sidereal rates.
            double dcsld = 1.990987E-7;           // sidereal rate in longitude
            double ccsgd = 1.990969E-7;           // sidereal rate in mean anomaly

            //Constants used in the calculation of the lunar contribution.
            double cckm = 3.122140E-5;
            double ccmld = 2.661699E-6;
            double ccfdi = 2.399485E-7;

            // Constants dcargm(i,k) of the arguments of the perturbations of the motion
            // of the moon.
            double[] dcargm1 = new double[]
                {5.1679830e0,  8.3286911095275e3
                ,5.4913150e0, -7.2140632838100e3 
                ,5.9598530e0,  1.5542754389685e4 };
            double[,] dcargm = reform(dcargm1, 2, 3);

            // Amplitudes ccampm(n,k) of the perturbations of the moon.
            double[] ccampm1 = new double[]
                {1.097594E-1, 2.896773E-7, 5.450474E-2,  1.438491E-7 
                ,-2.223581E-2, 5.083103E-8, 1.002548E-2, -2.291823E-8 
                , 1.148966E-2, 5.658888E-8, 8.249439E-3,  4.063015E-8};
            double[,] ccampm = reform(ccampm1, 4, 3);

            // ccpamv(k)=a*m*dl,dt (planets), dc1mme=1-mass(earth+moon)
            double[] ccpamv = new double[] { 8.326827E-11, 1.843484E-11, 1.988712E-12, 1.881276E-12 };
            double dc1mme = 0.99999696;

            // Time arguments.
            double dt = (dje - dcto) / dcjul;
            double[] tvec = new double[] { 1e0, dt, dt * dt };

            // Values of all elements for the instant(aneous?) dje.
            double[] temp = (mat_mult(tvec, dcfel));//% dc2pi;
            for (int i = 0; i < temp.Length; i++) temp[i] = temp[i] % dc2pi;
            double dml = temp[0];
            double[] forbel = new double[7];
            for (int i = 0; i < 7; i++) forbel[i] = temp[i + 1];
            double g = forbel[0];                         //old fortran equivalence

            double deps=0;
            for (int i = 0; i < tvec.Length; i++) deps+= tvec[i] * dceps[i];
            deps = deps % dc2pi;

            
            double[] sorbel = mat_mult(tvec, ccsel);
            for (int i = 0; i < sorbel.Length; i++) sorbel[i] = sorbel[i] % dc2pi;
            double e = sorbel[0];                         //old fortran equivalence

            // Secular perturbations in longitude.
            double dummy=Math.Cos(2.0);
            double[] tvec_part = new double[] { tvec[0], tvec[1] };
            double[,] ccsec_part = new double[ccsec.GetLength(0),2];

            for (int i = 0; i < ccsec.GetLength(0); i++)
            {
                ccsec_part[i, 0] = ccsec[i, 1]; ccsec_part[i, 1] = ccsec[i, 2];
            }

            double[] sn = mat_mult(tvec_part, ccsec_part);
            for (int i = 0; i < sn.Length; i++)
            {
                sn[i] = sn[i] % cc2pi;
                sn[i] = Math.Sin(sn[i]);
            }


            // Periodic perturbations of the emb (earth-moon barycenter).
            double pertl = 0;
            for (int i = 0; i < sn.Length; i++) pertl += ccsec[i, 0] * sn[i];
            pertl += dt * ccsec3 * sn[2];
            double pertld = 0.0;
            double pertr = 0.0;
            double pertrd = 0.0;
            for(int k=0; k<=14; k++)
            {
                double a1 = (dcargs[k, 0] + dt * dcargs[k, 1]) % dc2pi;
                double cosa = Math.Cos(a1);
                double sina = Math.Sin(a1);
                pertl = pertl + ccamps[k, 0] * cosa + ccamps[k, 1] * sina;
                pertr = pertr + ccamps[k, 2] * cosa + ccamps[k, 3] * sina;
                if (k < 11)
                {
                    pertld = pertld + (ccamps[k, 1] * cosa - ccamps[k, 0] * sina) * ccamps[k, 4];
                    pertrd = pertrd + (ccamps[k, 3] * cosa - ccamps[k, 2] * sina) * ccamps[k, 4];
                }
            }

            // Elliptic part of the motion of the emb.
            double phi = (e*e/4.0)*(((8.0/e)-e)*Math.Sin(g) +5*Math.Sin(2*g) +(13/3.0)*e*Math.Sin(3*g));
            double f = g + phi;
            double sinf = Math.Sin(f);
            double cosf = Math.Cos(f);
            double dpsi = (dc1 - e*e) / (dc1 + e*cosf);
            double phid = 2*e*ccsgd*((1 + 1.5*e*e)*cosf + e*(1.25 - 0.5*sinf*sinf));
            double psid = ccsgd*e*sinf / Math.Sqrt(dc1 - e*e);

            //// Perturbed heliocentric motion of the emb.
            double d1pdro = dc1+pertr;
            double drd = d1pdro * (psid + dpsi*pertrd);
            double drld = d1pdro*dpsi * (dcsld+phid+pertld);
            double dtl = (dml + phi + pertl) % dc2pi;
            double dsinls = Math.Sin(dtl);
            double dcosls = Math.Cos(dtl);
            double dxhd = drd * dcosls - drld * dsinls;
            double dyhd = drd * dsinls + drld * dcosls;

            // Influence of eccentricity, evection and variation on the geocentric
            // motion of the moon.
            pertl = 0.0;
            pertld = 0.0;
            double pertp = 0.0;
            double pertpd = 0.0;
            for(int k = 0; k<=2; k++)
            {
                double a1 = (dcargm[k,0] + dt*dcargm[k,1]) % dc2pi;
                double sina = Math.Sin(a1);
                double cosa = Math.Cos(a1);
                pertl = pertl + ccampm[k,0]*sina;
                pertld = pertld + ccampm[k,1]*cosa;
                pertp = pertp + ccampm[k,2]*cosa;
                pertpd = pertpd - ccampm[k,3]*sina;
            }

            // Heliocentric motion of the earth.
            double tl = forbel[1] + pertl;
            double sinlm = Math.Sin(tl);
            double coslm = Math.Cos(tl);
            double sigma = cckm / (1.0 + pertp);
            double a = sigma*(ccmld + pertld);
            double b = sigma*pertpd;
            dxhd = dxhd + a*sinlm + b*coslm;
            dyhd = dyhd - a*coslm + b*sinlm;
            double dzhd= -sigma*ccfdi*Math.Cos(forbel[2]);

            //// Barycentric motion of the earth.
            double dxbd = dxhd*dc1mme;
            double dybd = dyhd*dc1mme;
            double dzbd = dzhd*dc1mme;
            for(int k=0; k<=3; k++)
            {
                double plon = forbel[k+3];
                double pomg = sorbel[k+1];
                double pecc = sorbel[k+9];
                tl = (plon + 2.0*pecc*Math.Sin(plon-pomg)) % cc2pi;
                dxbd = dxbd + ccpamv[k]*(Math.Sin(tl) + pecc*Math.Sin(pomg));
                dybd = dybd - ccpamv[k]*(Math.Cos(tl) + pecc*Math.Cos(pomg));
                dzbd = dzbd - ccpamv[k]*sorbel[k+13]*Math.Cos(plon - sorbel[k+5]);
            }

            // Transition to mean equator of date.
            double dcosep = Math.Cos(deps);
            double dsinep = Math.Sin(deps);
            double dyahd = dcosep * dyhd - dsinep * dzhd;
            double dzahd = dsinep * dyhd + dcosep * dzhd;
            double dyabd = dcosep * dybd - dsinep * dzbd;
            double dzabd = dsinep * dybd + dcosep * dzbd;

            // Epoch of mean equinox (deq) of zero implies that we should use
            // Julian ephemeris date (dje) as epoch of mean equinox.
            if( deq == 0)
            {
                dvelh[0] = AU * dxhd; dvelh[1] = AU * dyahd; dvelh[2] = AU * dzahd;
                dvelb[0] = AU * dxbd; dvelb[1] = AU * dyabd; dvelb[2] = AU * dzabd;
                return;
            }

            // General precession from epoch dje to deq.
            //deqdat = (dje-dcto-dcbes) / dctrop + dc1900;
            //prema = premat(deqdat,deq, true);

            //dvelh = AU * ( prema # [dxhd, dyahd, dzahd] )
            //dvelb = AU * ( prema # [dxbd, dyabd, dzabd] )
        }

        private static double[,] reform(double[] mas, int cols, int rows)
        {
            double[,] res=new double[rows, cols];
            int k=0;
            for(int i=0; i<rows; i++)
            {
                for(int j=0; j<cols; j++)
                {
                    res[i,j]=mas[k];
                    k++;
                }
            }
            return res;
        }

        private static double[] mat_mult(double[] v, double[,] m)
        {
            double[] res = new double[m.GetLength(0)];
            for(int i=0; i<m.GetLength(0); i++)
            {
                double sum=0; 
                for(int j=0; j<v.Length; j++)
                {
                    sum+=v[j]*m[i,j];
                }
                res[i]=sum;
            }
            return res;
        }

        private static double[,] mat_mult(double[] v1, double[] v2)
        {
            double[,] res=new double[v1.Length, v1.Length];
            for(int i=0; i<v1.Length; i++)
            {
                for(int j=0; j<v1.Length; j++)
                {
                    res[j,i]=v1[i]*v2[j];
                }
            }
            return res;
        }
        

        

        private static void precess(ref double ra, ref double dec, 
             double equinox1,  double equinox2)
        {
            double ra_rad = ra * Math.PI / 180.0;
            double dec_rad = dec * Math.PI / 180.0;

            double a = Math.Cos(dec_rad);

            double[] x = new double[] { 
                a * Math.Cos(ra_rad), a * Math.Sin(ra_rad), Math.Sin(dec_rad) };

            double[,] r=premat(equinox1, equinox2, true);

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

        private static double[,] premat(double equinox1, double equinox2, bool FK4)
        {
            double deg_to_rad = Math.PI / 180.0;
            double sec_to_rad=deg_to_rad/3600.0;
            double t = 0.001*( equinox2 - equinox1);
            double a,b,c;
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

            double sina = Math.Sin(a); double  sinb = Math.Sin(b); double sinc = Math.Sin(c);
            double cosa = Math.Cos(a); double  cosb = Math.Cos(b); double cosc = Math.Cos(c);

            double[,] r = new double[3,3];
            r[0, 0] = cosa * cosb * cosc - sina * sinb; r[0, 1] = sina * cosb + cosa * sinb * cosc; r[0, 2] = cosa * sinc;
            r[1, 0] = -cosa * sinb - sina * cosb * cosc; r[1, 1] = cosa * cosb - sina * sinb * cosc; r[1, 2] = -sina * sinc;
            r[2, 0] = -cosb * sinc; r[2, 1] = -sinb * sinc; r[2, 2] = cosc;

            return r;
        }
    }
}
