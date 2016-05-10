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

        [FieldConverter(typeof(FloatConverter))]
        public float? StrikePrice;

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

        [FieldConverter(typeof(FloatConverter))]
        public float? FirstPrice;

        [FieldConverter(typeof(FloatConverter))]
        public float? HighPrice;

        [FieldConverter(typeof(FloatConverter))]
        public float? LowPrice;

        [FieldConverter(typeof(FloatConverter))]
        public float? SettlementPrice;

        [FieldConverter(typeof(FloatConverter))]
        public float? SettlementPriceChange;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? Volume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFPVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? EFSVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? BlockVolume;

        [FieldConverter(typeof(FloatConverter))]
        public float? WeightedAveragePrice;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? OpenInterest;

        [FieldConverter(typeof(FloatConverter))]
        public float? ImpliedVolatility;

        [FieldConverter(typeof(FloatConverter))]
        public float? Delta;

        [FieldConverter(typeof(StringConverter))]
        public string RelativePeriod;

        [FieldConverter(typeof(StringConverter))]
        public string RelativeStrike;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong HubID;

        [FieldConverter(typeof(StringConverter))]
        public string HubName;

        [FieldOptional]
        [FieldConverter(typeof(FloatConverter))]
        public float? ClosePrice;

        [FieldOptional]
        [FieldConverter(typeof(Int64Converter))]
        public long? OpenInterestChange;
    }
}
