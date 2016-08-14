using System;

namespace ICE_Import
{
    class OptionCalcs
    {
        public static double CalculateOptionVolatilityNR(char callPutFlag, double S, double X, double T, double r, double currentOptionPrice, double epsilon)
        {
            double vi = 0, ci, vegai, prevVi = 0;
            double b = 0; //for futures b = 0;

            try
            {
                if (T == 0)
                {
                    T = 0.0001;
                }

                vi = Math.Sqrt(Math.Abs(Math.Log(S / X) + r * T) * 2 / T);



                ci = blackScholes(callPutFlag, S, X, T, r, vi);
                vegai = gVega(S, X, T, r, b, vi);


                int maxIter = 100;
                int i = 0;

                prevVi = vi;

                double priceDifference = Math.Abs(currentOptionPrice - ci);
                double smallestPriceDifference = priceDifference;

                while (priceDifference > (epsilon / 10) && i < maxIter)
                {
                    //if (vi <= prevVi && vi > 0 && ci > 0)

                    if (priceDifference < smallestPriceDifference
                        && vi <= prevVi
                        && vi > 0 && ci > 0)
                    {
                        prevVi = vi;
                        smallestPriceDifference = priceDifference;
                    }


                    
                    vi = Math.Abs(vi - (ci - currentOptionPrice) / vegai);
                    

                    ci = blackScholes(callPutFlag, S, X, T, r, vi);

                    priceDifference = Math.Abs(currentOptionPrice - ci);

                    if (vi <= 0 || double.IsInfinity(vi) || double.IsNaN(vi))
                    {
                        vi = prevVi;

                        break;
                    }

                    vegai = gVega(S, X, T, r, b, vi);                   

                    i++;
                }

                if (i == maxIter)
                {
                    vi = prevVi;
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            vi = double.IsInfinity(vi) || vi < 0 || double.IsNaN(vi) ? 0 : vi;

            return vi;
        }

        // The Black and Scholes (1973) Stock option formula
        private static double blackScholes(char CallPutFlag, double S, double X, double T, double r, double v)
        {
            try
            {
                if (T == 0)
                {
                    T = 0.0001;
                }

                double d1, d2;

                d1 = (Math.Log(S / X) + (r + v * v / 2) * T) / (v * Math.Sqrt(T));
                d2 = d1 - v * Math.Sqrt(T);

                if (CallPutFlag == 'c' || CallPutFlag == 'C')
                {
                    return S * CND(d1) - X * Math.Exp(-r * T) * CND(d2);
                }
                else
                {
                    return X * Math.Exp(-r * T) * CND(-d2) - S * CND(-d1);
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        // The cumulative normal distribution function 
        private static double CND(double X)
        {
            try
            {
                double L, K, w;
                double a1 = 0.31938153,
                    a2 = -0.356563782,
                    a3 = 1.781477937,
                    a4 = -1.821255978,
                    a5 = 1.330274429;

                L = Math.Abs(X);
                K = 1.0 / (1.0 + 0.2316419 * L);
                w = 1.0 - 1.0 / Math.Sqrt(2.0 * Math.PI) * Math.Exp(-L * L / 2) * (a1 * K + a2 * K * K + a3
                    * Math.Pow(K, 3) + a4 * Math.Pow(K, 4) + a5 * Math.Pow(K, 5));

                if (X < 0.0)
                {
                    w = 1.0 - w;
                }
                return w;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static double ND(double X)
        {
            try
            {
                return Math.Exp(-X * X / 2) / Math.Sqrt(2 * Math.PI);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        // Vega for the generalized Black and Scholes formula
        private static double gVega(double S, double X, double T, double r, double b, double v)
        {
            try
            {
                if (T == 0)
                {
                    T = 0.0001;
                }

                double d1 = (Math.Log(S / X) + (b + Math.Pow(v, 2) / 2) * T) / (v * Math.Sqrt(T));
                return S * Math.Exp((b - r) * T) * ND(d1) * Math.Sqrt(T);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
