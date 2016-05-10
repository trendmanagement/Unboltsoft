// Warning CS0649  Field is never assigned to, and will always have its default value null
#pragma warning disable 0649

using System;
using FileHelpers;

namespace ICE_Import
{
    [IgnoreFirst]
    [DelimitedRecord(",")]
    class EOD_Futures_578
    {
        [FieldConverter(typeof(DateTimeConverter), "dd-MMM-yyyy")]
        public DateTime Date;

        [FieldConverter(typeof(StringConverter))]
        public string MarketID;

        [FieldConverter(typeof(StringConverter))]
        public string ProductName;

        [FieldConverter(typeof(DateTimeConverter), "MMMyy")]
        public DateTime StripName;

        public string Period1;

        public string Period2;

        [FieldConverter(typeof(CharConverter))]
        public char Type;

        [FieldConverter(typeof(DecimalConverter))]
        public float? FirstPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public float? HighPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public float? LowPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public float? SettlementPrice;

        [FieldConverter(typeof(DecimalConverter))]
        public float? SettlementPriceChange;

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

        [FieldConverter(typeof(StringConverter))]
        public string RelativePeriod;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong ProductID;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong HubID;

        [FieldConverter(typeof(StringConverter))]
        public string HubName;

        [FieldOptional]
        [FieldConverter(typeof(DecimalConverter))]
        public float? ClosePrice;

        [FieldOptional]
        [FieldConverter(typeof(Int64Converter))]
        public long? OpenInterestChange;
    }
}
