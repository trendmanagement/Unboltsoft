// Warning CS0649  Field is never assigned to, and will always have its default value null
#pragma warning disable 0649

using System;
using FileHelpers;

namespace ICE_Import
{
    [IgnoreFirst]
    [DelimitedRecord(",")]
    class EOD_Option_CSV
    {
        [FieldConverter(typeof(DateTimeConverter), "dd-MMM-yyyy")]
        public DateTime Date;

        [FieldConverter(typeof(CharConverter))]
        public char TradingType;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong UnderlyingMarketID;

        [FieldConverter(typeof(CharConverter), true)]
        public char OptionType;

        [FieldConverter(typeof(DecimalConverter), true)]
        public decimal? StrikePrice;

        [FieldConverter(typeof(StringConverter))]
        public string OptionMarketID;

        [FieldConverter(typeof(StringConverter))]
        public string ProductName;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong ProductID;

        [FieldConverter(typeof(DateTimeConverter), "MMMyy")]
        public DateTime StripName;

        [FieldConverter(typeof(CharConverter))]
        public char Type;

        [FieldConverter(typeof(DoubleConverter))]
        public double? FirstPrice;

        [FieldConverter(typeof(DoubleConverter))]
        public double? HighPrice;

        [FieldConverter(typeof(DoubleConverter))]
        public double? LowPrice;

        [FieldConverter(typeof(DoubleConverter))]
        public double? SettlementPrice;

        [FieldConverter(typeof(DoubleConverter))]
        public double? SettlementPriceChange;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? Volume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFPVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFSVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? BlockVolume;

        [FieldConverter(typeof(DoubleConverter))]
        public double? WeightedAveragePrice;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? OpenInterest;

        [FieldConverter(typeof(DoubleConverter))]
        public double? ImpliedVolatility;

        [FieldConverter(typeof(DoubleConverter))]
        public double? Delta;

        [FieldConverter(typeof(StringConverter))]
        public string RelativePeriod;

        [FieldConverter(typeof(StringConverter))]
        public string RelativeStrike;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong HubID;

        [FieldConverter(typeof(StringConverter))]
        public string HubName;

        [FieldOptional]
        [FieldConverter(typeof(DoubleConverter))]
        public double? ClosePrice;

        [FieldOptional]
        [FieldConverter(typeof(Int64Converter))]
        public long? OpenInterestChange;
    }

    /// <summary>
    /// A reduced version of the class EOD_Options_CSV that contains only the data we actually use
    /// </summary>
    class EOD_Option
    {
        public DateTime Date;
        public char OptionType;
        public decimal? StrikePrice;
        public DateTime StripName;
        public double? SettlementPrice;

        public EOD_Option(EOD_Option_CSV csvRow)
        {
            Date            = csvRow.Date;
            OptionType      = csvRow.OptionType;
            StrikePrice     = csvRow.StrikePrice;
            StripName       = csvRow.StripName;
            SettlementPrice = csvRow.SettlementPrice;
        }
    }
}
