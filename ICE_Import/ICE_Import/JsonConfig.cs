using System;

namespace ICE_Import
{
    public class JsonConfig
    {
        public ICEConfiguration ICE_Configuration;

        public class ICEConfiguration
        {
            public int? NormalizeConstant;
            public long? IdInstrument;
            public string CQGSymbol;
            public double? OptionTickSize;
            public double? OptionStrikeIncrement;
            public double? OptionStrikeDisplay;
            public string TMLDB_Description;
            public string[] Regular_Options;
        }
    }
}
