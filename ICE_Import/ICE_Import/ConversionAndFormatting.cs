using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    //you guys will probably come up with a much cleaner architecture than this
    //we should probably just have a switch that has a separate formatting algo for each instrument


    class ConversionAndFormatting
    {

        public static double convertToStrikeForCQGSymbol(double barVal, double tickIncrement, double tickDisplay,
            int idInstrument)
        {
            if (idInstrument == 39 || idInstrument == 40) //GLE or HE
            {
                return barVal * tickDisplay;
            }
            else if (idInstrument == 1 || idInstrument == 360)
            {
                return barVal * tickDisplay;
            }
            //else if (idInstrument == 101) //GLE or HE
            //{
            //    return convertToTickMovesDouble(barVal, tickIncrement, tickDisplay) / 100;
            //}
            else
            {
                return convertToTickMovesDouble(barVal, tickIncrement, tickDisplay);
            }
        }

        public static double convertToTickMovesDouble(double barVal, double tickIncrement, double tickDisplay)
        {
            if (tickDisplay == 0)
                return barVal;

            double fuzzyZero = tickIncrement / 1000;
            double positiveFuzzyZero = tickIncrement / 1000;

            if (barVal < 0)
                fuzzyZero = -tickIncrement / 1000;

            int nTicksInUnit = (int)(1 / tickIncrement + positiveFuzzyZero);

            if (nTicksInUnit == 0)
                return barVal / tickIncrement * tickDisplay;

            //nTicksInUnit = 1;

            int intPart = (int)(barVal + fuzzyZero);
            int nTicks = (int)((barVal + fuzzyZero) / tickIncrement + fuzzyZero);
            //Debug.WriteLine(barVal + "  " + tickIncrement + "  " + fuzzyZero + "  " + nTicks + "  ");

            int decPart = (int)((nTicks % nTicksInUnit) * tickDisplay + fuzzyZero);
            double fractPart = 0;

            // a hack for Eurodollar
            if (tickDisplay < 1)
                fractPart = (nTicks % nTicksInUnit) * tickDisplay - decPart;

            int decimalBase = 1;

            //if (tickDisplay/tickIncrement > 1)
            //    decimalBase = (int)tickDisplay;
            //else



            while (((nTicksInUnit - 1) * tickDisplay / decimalBase) >= 1)
                decimalBase *= 10;


            //if (decimalBase == 1 && tickDisplay > 0 && tickIncrement > 0 && tickDisplay > tickIncrement)
            //{
            //    decimalBase = (int)(tickDisplay / tickIncrement);
            //}


            return intPart * decimalBase + decPart + fractPart;


        }

        public static double convertToStrikeFromSpanData(String barVal, double tickIncrement, double tickDisplay,
            int idInstrument, DateTime currentFileDate)
        {
            if (idInstrument == 1 || idInstrument == 360) //USA=1 ULA=360
            {
                if (currentFileDate.CompareTo(new DateTime(2016, 3, 7)) >= 0)
                {
                    return Convert.ToDouble(barVal) / tickDisplay;
                }
                else
                {
                    return convertToTickMovesDoubleSpan(barVal, tickIncrement, 0);
                }
            }
            //else if (idInstrument == 101) //GLE or HE
            //{
            //    return convertToTickMovesDouble(barVal, tickIncrement, tickDisplay) / 100;
            //}
            else
            {
                return convertToTickMovesDoubleSpan(barVal, tickIncrement, tickDisplay);
            }
        }

        public static double convertToTickMovesDoubleSpan(String barVal, double tickIncrement, double tickDisplay)
        {
            if (tickDisplay == 0)
                return Convert.ToDouble(barVal);

            double fuzzyZero = tickIncrement / 1000;
            double positiveFuzzyZero = tickIncrement / 1000;

            int nTicksInUnit = (int)(1 / tickIncrement + positiveFuzzyZero);

            double maxFractUnits = 0;

            if (tickIncrement == 1 && tickDisplay > 0)
            {
                maxFractUnits = nTicksInUnit * tickDisplay;
            }
            else
            {
                maxFractUnits = (nTicksInUnit - 1) * tickDisplay;
            }





            int decimalBase = 1;
            while ((maxFractUnits + positiveFuzzyZero) / decimalBase >= 1)
                decimalBase *= 10;

            double displayVal = Convert.ToDouble(barVal);

            if (displayVal < 0)
                fuzzyZero = -tickIncrement / 1000;

            int intPart = (int)((displayVal + fuzzyZero) / decimalBase + fuzzyZero);
            double decPart = (displayVal - intPart * decimalBase) / tickDisplay * tickIncrement;

            double incrementFixTest = (displayVal - intPart * decimalBase) % (tickIncrement * decimalBase);

            double incrementFix = 0;

            if (incrementFixTest != 0)
                incrementFix = ((tickIncrement * decimalBase) - incrementFixTest) / decimalBase;



            //double fractPart = 0;

            // a hack for Eurodollar
            //if ( tickDisplay < 1 )
            //    fractPart = (nTicks % nTicksInUnit) * tickDisplay - decPart;


            return intPart + decPart + incrementFix;

            
        }

    }
}
