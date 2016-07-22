// Warning CS0649  Field is never assigned to, and will always have its default value null
#pragma warning disable 0649

using System;
using FileHelpers;

namespace ICE_Import
{
    [IgnoreFirst]
    [DelimitedRecord(",")]
    class EOD_Options_Selected
    {
        [FieldConverter(typeof(DateTimeConverter), "MMMyy")]
        public DateTime DateNameForFuture;

        public EOD_Options EOD_Option;
    }
}
