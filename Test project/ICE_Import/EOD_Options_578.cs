// Warning CS0649  Field is never assigned to, and will always have its default value null
#pragma warning disable 0649

using System;
using FileHelpers;

namespace ICE_Import
{
    [IgnoreFirst]
    [DelimitedRecord(",")]
    class EOD_Options_578
    {
        [FieldConverter(typeof(DateTimeConverter), "dd-MMM-yyyy")]
        public DateTime Date;

        [FieldConverter(typeof(CharConverter))]
        public char TradingType;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong UnderlyingMarketID;

        [FieldConverter(typeof(CharConverter))]
        public char OptionType;

        [FieldConverter(typeof(DecimalConverter))]
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

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? FirstPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? HighPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? LowPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? SettlementPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? SettlementPriceChange;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? Volume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFPVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFSVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? BlockVolume;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? WeightedAveragePrice;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? OpenInterest;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? ImpliedVolatility;

        [FieldConverter(typeof(DecimalConverter))]
        public decimal? Delta;

        [FieldConverter(typeof(StringConverter))]
        public string RelativePeriod;

        [FieldConverter(typeof(StringConverter))]
        public string RelativeStrike;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong HubID;

        [FieldConverter(typeof(StringConverter))]
        public string HubName;

        [FieldOptional]
        [FieldConverter(typeof(DecimalConverter))]
        public decimal? ClosePrice;

        [FieldOptional]
        [FieldConverter(typeof(Int64Converter))]
        public long? OpenInterestChange;
    }
}
