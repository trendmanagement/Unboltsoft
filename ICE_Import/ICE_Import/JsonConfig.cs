using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    public class JsonConfig
    {
        public ICEConfiguration ICE_Configuration;

        public class ICEConfiguration
        {
            public RiskFreeInterestRate[] RiskFreeInterestRates;
            public int? NormlizeConstant;
            public long? IdInstrument;
            public string CQGSymbol;
            public double? OptionTickSize;
            public double? OptionStrikeIncrement;
            public double? OptionStrikeDisplay;
            public string TMLDB_Description;
            public string[] Regular_Options;
        }
    }

    public class RiskFreeInterestRate
    {
        public DateTime Date;
        public double Risk;
    }
}
