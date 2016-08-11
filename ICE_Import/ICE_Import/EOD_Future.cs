// Warning CS0649  Field is never assigned to, and will always have its default value null
#pragma warning disable 0649

using System;
using FileHelpers;

namespace ICE_Import
{
    [IgnoreFirst]
    [DelimitedRecord(",")]
    class EOD_Future_CSV
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

        [FieldConverter(typeof(Int64Converter))]
        public long? EFSVolume;

        [FieldConverter(typeof(UInt64Converter))]
        public ulong? BlockVolume;

        [FieldConverter(typeof(DoubleConverter))]
        public double? WeightedAveragePrice;

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
        [FieldConverter(typeof(DoubleConverter))]
        public double? ClosePrice;

        [FieldOptional]
        [FieldConverter(typeof(Int64Converter))]
        public long? OpenInterestChange;
    }

    /// <summary>
    /// A reduced version of the class EOD_Futures_CSV that contains only the data we actually use
    /// </summary>
    class EOD_Future
    {
        public DateTime Date;
        public DateTime StripName;
        public double? SettlementPrice;
        public ulong? Volume;
        public ulong? OpenInterest;

        public EOD_Future(EOD_Future_CSV csvRow)
        {
            Date            = csvRow.Date;
            StripName       = csvRow.StripName;
            SettlementPrice = csvRow.SettlementPrice;
            Volume          = csvRow.Volume;
            OpenInterest    = csvRow.OpenInterest;
        }
    }
}
