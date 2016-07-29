using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ICE_Import
{
    static class Utilities
    {
        private static char[] ContractTypes = { 'F', 'C', 'P' };

        /// <summary>
        /// Generates the CQG symbol from span.
        /// </summary>
        /// <param name="contractType">Type of the contract 'F'=future, 'C'=call, 'P'=put.</param>
        /// <param name="underlyingSymbol">The underlying symbol.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns>cqgSymbol</returns>
        public static string GenerateCQGSymbolFromSpan(
            char contractType,
            string underlyingSymbol,
            char month,
            int year)
        {
            Debug.Assert(ContractTypes.Contains(contractType));

            StringBuilder cqgSymbol = new StringBuilder();

            cqgSymbol.Append(contractType);

            cqgSymbol.Append(".");

            cqgSymbol.Append(underlyingSymbol);

            cqgSymbol.Append(month);

            cqgSymbol.Append(year % 100);

            return cqgSymbol.ToString();
        }

        /// <summary>
        /// Generates the option CQG symbol from span.
        /// </summary>
        /// <param name="contractType">Type of the contract 'F'=future, 'C'=call, 'P'=put.</param>
        /// <param name="underlyingSymbol">The underlying symbol.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <param name="optionStrikePrice">The option strike price.</param>
        /// <param name="optionStrikeIncrement">The option strike increment.</param>
        /// <param name="optionStrikeDisplay">The option strike display.</param>
        /// <param name="instrumentId">The instrument identifier.</param>
        /// <returns></returns>
        public static string GenerateOptionCQGSymbolFromSpan(
            char contractType,
            string underlyingSymbol,
            char month,
            int year, 
            double strikeToCQG)
        {
            Debug.Assert(ContractTypes.Contains(contractType));

            StringBuilder cqgSymbol = new StringBuilder();

            cqgSymbol.Append(contractType);

            cqgSymbol.Append(".US.");

            cqgSymbol.Append(underlyingSymbol);

            cqgSymbol.Append(month);

            cqgSymbol.Append(year % 100);

            cqgSymbol.Append(strikeToCQG);

            return cqgSymbol.ToString();
        }

        public static char MonthToMonthCode(int month)
        {
            Debug.Assert(month >= 1 && month <= 12);
            return Convert.ToChar(((MonthCodes)month).ToString());
        }

        public static string MonthToStringMonthCode(int month)
        {
            Debug.Assert(month >= 1 && month <= 12);
            return Convert.ToString(((MonthStringCodes)month).ToString());
        }

        public static double NormalizePrice(double? price)
        {
            return price.GetValueOrDefault() / 1000.0;
        }

        public static double NormalizePrice(decimal? price)
        {
            return (double)price.GetValueOrDefault() / 1000.0;
        }
    }
}
