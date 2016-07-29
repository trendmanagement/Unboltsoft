using System;
using System.Collections.Generic;

namespace ICE_Import
{
    // TODO: you guys will probably come up with a much cleaner architecture than this
    //       we should probably just have a switch that has a separate formatting algo for each instrument

    class ConversionAndFormatting
    {
        static HashSet<long> hardcoresIDforCQG = new HashSet<long>() { 39, 40, 1, 360 }; //GLE or HE
        static HashSet<long> hardcoresIDforSpanData = new HashSet<long>() { 1, 360 }; //USA = 1 ULA = 360

        public static double convertToStrikeForCQGSymbol(
            double barVal,
            double tickIncrement,
            double tickDisplay,
            long idInstrument)
        {
            if (hardcoresIDforCQG.Contains(idInstrument))
            {
                return barVal * tickDisplay;
            }
            else
            {
                return convertToTickMovesDouble(barVal, tickIncrement, tickDisplay);
            }
        }

        public static double ConvertToDouble(double barValue)
        {
            if(barValue > 1)
            {
                int factor = NormalizeLg(barValue);
                double normalizePriseConst = Math.Pow(10, factor);
                return barValue / normalizePriseConst;
            }
            else
            {
                return barValue;
            }
        }

        public static int NormalizeLg(double price)
        {
            double lg = Math.Log10(price);
            int normalize = (int)Math.Ceiling(lg);
            if(normalize/lg == 1)
            {
                return normalize;
            }
            else
            {
                return normalize - 1;
            }
        }

        public static double convertToTickMovesDouble(
            double barVal, 
            double tickIncrement, 
            double tickDisplay)
        {
            if (tickDisplay == 0)
                return barVal;

            double fuzzyZero = (barVal < 0)? -tickIncrement / (int)ParsedData.NormalizeConst : tickIncrement / (int)ParsedData.NormalizeConst;
            double positiveFuzzyZero = tickIncrement / (int)ParsedData.NormalizeConst;

            int nTicksInUnit = (int)(1 / tickIncrement + positiveFuzzyZero);

            if (nTicksInUnit == 0)
                return barVal / tickIncrement * tickDisplay;

            int intPart = (int)(barVal + fuzzyZero);
            int nTicks = (int)((barVal + fuzzyZero) / tickIncrement + fuzzyZero);

            int decPart = (int)((nTicks % nTicksInUnit) * tickDisplay + fuzzyZero);
            double fractPart = (tickDisplay < 1) ? (nTicks % nTicksInUnit) * tickDisplay - decPart : 0;

            int decimalBase = 1;

            while (((nTicksInUnit - 1) * tickDisplay / decimalBase) >= 1)
                decimalBase *= 10;

            return intPart * decimalBase + decPart + fractPart;
        }

        public static double convertToTickMovesDouble(
            string barVal, 
            double tickIncrement, 
            double tickDisplay,
            int idInstrument, 
            DateTime currentFileDate)
        {
            if (hardcoresIDforSpanData.Contains(idInstrument)) 
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
            else
            {
                return convertToTickMovesDoubleSpan(barVal, tickIncrement, tickDisplay);
            }
        }

        public static double convertToTickMovesDoubleSpan(
            string barVal, 
            double tickIncrement, 
            double tickDisplay)
        {
            if (tickDisplay == 0)
                return Convert.ToDouble(barVal);

            double fuzzyZero = tickIncrement / (int)ParsedData.NormalizeConst;
            double positiveFuzzyZero = tickIncrement / (int)ParsedData.NormalizeConst;

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
                fuzzyZero = -tickIncrement / (int)ParsedData.NormalizeConst;

            int intPart = (int)((displayVal + fuzzyZero) / decimalBase + fuzzyZero);
            double decPart = (displayVal - intPart * decimalBase) / tickDisplay * tickIncrement;

            double incrementFixTest = (displayVal - intPart * decimalBase) % (tickIncrement * decimalBase);

            double incrementFix = 0;

            if (incrementFixTest != 0)
                incrementFix = ((tickIncrement * decimalBase) - incrementFixTest) / decimalBase;

            return intPart + decPart + incrementFix;
        }
    }
}
