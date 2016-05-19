using System.Text;

namespace ICE_Import
{
    class Utilities
    {
        /// <summary>
        /// Generates the CQG symbol from span.
        /// </summary>
        /// <param name="contractType">Type of the contract 'F'=future, 'C'=call, 'P'=put.</param>
        /// <param name="underlyingSymbol">The underlying symbol.</param>
        /// <param name="month">The month code, monthCodes = ['F','G','H','J','K','M','N','Q','U','V','X','Z'];</param>
        /// <param name="year">The year.</param>
        /// <returns>cqgSymbol</returns>
        public string generateCQGSymbolFromSpan(char contractType, string underlyingSymbol, char month, int year)
        {
            StringBuilder cqgSymbol = new StringBuilder();

            cqgSymbol.Append(contractType);

            cqgSymbol.Append(".");

            cqgSymbol.Append(underlyingSymbol);

            cqgSymbol.Append(month);

            cqgSymbol.Append((year % 100));

            return cqgSymbol.ToString();
        }


        /// <summary>
        /// Generates the option CQG symbol from span.
        /// </summary>
        /// <param name="contractType">Type of the contract 'F'=future, 'C'=call, 'P'=put.</param>
        /// <param name="underlyingSymbol">The underlying symbol.</param>
        /// <param name="month">The month code, monthCodes = ['F','G','H','J','K','M','N','Q','U','V','X','Z'];</param>
        /// <param name="year">The year.</param>
        /// <param name="optionStrikePrice">The option strike price.</param>
        /// <param name="optionStrikeIncrement">The option strike increment.</param>
        /// <param name="optionStrikeDisplay">The option strike display.</param>
        /// <param name="instrumentId">The instrument identifier.</param>
        /// <returns></returns>
        public string generateOptionCQGSymbolFromSpan(char contractType, string underlyingSymbol, char month, int year, 
            double optionStrikePrice, double optionStrikeIncrement, double optionStrikeDisplay, int instrumentId)
        {
            StringBuilder cqgSymbol = new StringBuilder();

            cqgSymbol.Append(contractType);

            cqgSymbol.Append(".US.");

            cqgSymbol.Append(underlyingSymbol);

            cqgSymbol.Append(month);

            cqgSymbol.Append((year % 100));

            cqgSymbol.Append(ConversionAndFormatting.convertToStrikeForCQGSymbol(
                                    optionStrikePrice,
                                    optionStrikeIncrement,
                                    optionStrikeDisplay, instrumentId));

            return cqgSymbol.ToString();
        }
    }
}
