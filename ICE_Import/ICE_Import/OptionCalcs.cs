using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    class OptionCalcs
    {
        // The generalized Black and Scholes formula
        //
        // S - stock price
        // X - strike price of option
        // r - risk-free interest rate
        // T - time to expiration in years
        // v - volatility of the relative price change of the underlying stock price
        // b - cost-of-carry
        //     b = r        --> Black-Scholes stock option model
        //     b = 0        --> Black futures option model
        //     b = r-q      --> Merton stock option model with continuous dividend yield q
        //     b = r - r(f) --> Garman-Kohlhagen currency option model, where r(f) is the risk-free rate of the foreign currency
        //
        // Examples:
        // a) currency option
        //    T = 0.5, 6 month to expiry
        //    S = 1.56, USD/DEM exchange rate is 1.56
        //    X = 1.6,  strike is 1.60
        //    r = 0.06, domestic interest rate in Germany is 6% per annum
        //    r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
        //    v = 0.12, volatility is 12% per annum
        //    c = 0.0291

        public static double calculateOptionVolatility(char callPutFlag, double S, double X, double T, double r, double currentOptionPrice)
        {
            double tempV = 0.5;
            try
            {
                if (T == 0)
                {
                    T = 0.0001;
                }

                int i = 0;

                double volInc = 0.5;
                double priceTest;

                int maxIter = 100;

                while (i < maxIter)
                {
                    priceTest = blackScholes(callPutFlag, S, X, T, r, tempV);

                    if (Math.Abs(currentOptionPrice - priceTest) < 0.01)
                    {
                        return tempV;
                    }
                    else if (priceTest - currentOptionPrice > 0)
                    {
                        volInc /= 2;
                        tempV -= volInc;
                    }
                    else
                    {
                        volInc /= 2;
                        tempV += volInc;
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                throw(ex);
            }

            return tempV;
        }

        public static double calculateOptionVolatilityNR(char callPutFlag, double S, double X, double T, double r, double currentOptionPrice, double tickSize)
        {
            return calculateOptionVolatilityNRCalc(callPutFlag, S, X, T, r, currentOptionPrice,
                tickSize);
        }

        public static double calculateOptionVolatilityNR(char callPutFlag, double S, double X, double T, double r, double currentOptionPrice)
        {
            return calculateOptionVolatilityNRCalc(callPutFlag, S, X, T, r, currentOptionPrice,
                0.0001);
        }

        public static double calculateOptionVolatilityNRCalc(char callPutFlag, double S, double X, double T, double r, double currentOptionPrice, double epsilon)
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
        public static double blackScholes(char CallPutFlag, double S, double X, double T, double r, double v)
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
        public static double CND(double X)
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

        public static double ND(double X)
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
        public static double gVega(double S, double X, double T, double r, double b, double v)
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

        // Gamma for the generalized Black and Scholes formula
        public static double gGamma(double S, double X, double T, double r, double b, double v)
        {
            if (T == 0)
            {
                T = 0.0001;
            }

            double d1 = (Math.Log(S / X) + (b + Math.Pow(v, 2) / 2) * T) / (v * Math.Sqrt(T));

            return Math.Exp((b - r) * T) * ND(d1) / (S * v * Math.Sqrt(T));
        }

        // Theta for the generalized Black and Scholes formula
        public static double gTheta(char CallPutFlag, double S, double X, double T, double r, double b, double v)
        {
            if (T == 0)
            {
                T = 0.0001;
            }

            double d1, d2;

            d1 = (Math.Log(S / X) + (b + v * v / 2) * T) / (v * Math.Sqrt(T));
            d2 = d1 - v * Math.Sqrt(T);

            if (CallPutFlag == 'c' || CallPutFlag == 'C')
            {
                return -S * Math.Exp((b - r) * T) * ND(d1) * v / (2 * Math.Sqrt(T)) - (b - r)
                    * S * Math.Exp((b - r) * T) * CND(d1) - r * X * Math.Exp(-r * T) * CND(d2);
            }
            else
            {
                return -S * Math.Exp((b - r) * T) * ND(d1) * v / (2 * Math.Sqrt(T)) + (b - r)
                    * S * Math.Exp((b - r) * T) * CND(-d1) + r * X * Math.Exp(-r * T) * CND(-d2);
            }
        }

        // Delta for the generalized Black and Scholes formula
        public static double gDelta(char CallPutFlag, double S, double X, double T, double r, double b, double v)
        {
            if (T == 0)
            {
                T = 0.0001;
            }

            double d1 = (Math.Log(S / X) + (b + Math.Pow(v, 2) / 2) * T) / (v * Math.Sqrt(T));

            if (CallPutFlag == 'c' || CallPutFlag == 'C')
            {
                return Math.Exp((b - r) * T) * CND(d1);
            }
            else
            {
                return Math.Exp((b - r) * T) * (CND(d1) - 1);
            }
        }

    }
}
