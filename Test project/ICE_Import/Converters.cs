using System;
using System.Globalization;
using FileHelpers;

namespace ICE_Import
{
    abstract class TrimmingConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return Parse(from.Trim('\"'));
        }

        public abstract object Parse(string from);

        public override string FieldToString(object fieldValue)
        {
            throw new NotImplementedException();
        }
    }

    class StringConverter : TrimmingConverter
    {
        public override object Parse(string from)
        {
            return from;
        }
    }

    class CharConverter : TrimmingConverter
    {
        public override object Parse(string from)
        {
            try
            {
                return Convert.ToChar(from);
            }
            catch (FormatException)
            {
                ThrowConvertException(from, "Failed to convert to Char.");
                return null;
            }
        }
    }

    class UInt64Converter : TrimmingConverter
    {
        public override object Parse(string from)
        {
            try
            {
                return Convert.ToUInt64(from);
            }
            catch (Exception)
            {
                // FormatException, OverflowException
                ThrowConvertException(from, "Failed to convert to UInt64.");
                return null;
            }
        }
    }

    class Int64Converter : TrimmingConverter
    {
        public override object Parse(string from)
        {
            try
            {
                return Convert.ToInt64(from);
            }
            catch (Exception)
            {
                // FormatException, OverflowException
                ThrowConvertException(from, "Failed to convert to Int64.");
                return null;
            }
        }
    }

    class DecimalConverter : TrimmingConverter
    {
        public override object Parse(string from)
        {
            try
            {
                return Convert.ToDecimal(from);
            }
            catch (Exception)
            {
                // FormatException, OverflowException
                ThrowConvertException(from, "Failed to convert to Decimal.");
                return null;
            }
        }
    }

    class DateTimeConverter : TrimmingConverter
    {
        string Format;

        public DateTimeConverter(string format)
        {
            Format = format;
        }

        public override object Parse(string from)
        {
            DateTime result;
            bool success = DateTime.TryParseExact(from, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
            if (!success)
            {
                string errorMsg = string.Format("The string does not match the format: \"{0}\".", Format);
                ThrowConvertException(from, errorMsg);
            }
            return result;
        }
    }
}
