using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace ICE_Import
{
    internal static class ParsedData
    {
        public delegate void ParseEventHandler();
        public static event ParseEventHandler ParseSucceeded;
        public static event ParseEventHandler ParseFailed;
        public static EOD_Futures_578[] FutureRecords;
        public static EOD_Options_578[] OptionRecords;
        public static bool FuturesOnly;

        private static bool IsConform;

        public static bool IsParsed
        {
            get
            {
                if (FuturesOnly)
                {
                    return FutureRecords != null;
                }
                else
                {
                    return OptionRecords != null && FutureRecords != null;
                }
            }
        }

        public static bool IsReady
        {
            get
            {
                if (FuturesOnly)
                {
                    return FutureRecords != null;
                }
                else
                {
                    return OptionRecords != null && FutureRecords != null && IsConform;
                }
            }
        }

        public static void OnParseComplete()
        {
            if (IsParsed)
            {
                if (FuturesOnly || ConformityCheck())
                {
                    Program.csvf.Hide();
                    if (Program.dbf == null)
                    {
                        Program.dbf = new FormDB();
                    }
                    Program.dbf.Show();

                    // Raise event
                    ParseSucceeded();
                }
                else
                {
                    MessageBox.Show("The selected Future CSV Files(s) and Option CSV File(s) do not conform to each other.", "ICE Import (DB Form)", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Raise event
                    ParseFailed();
                }
            }
            else
            {
                // Raise event
                ParseFailed();
            }
        }
        
        private static bool ConformityCheck()
        {
            var futureStripNames = new HashSet<DateTime>(FutureRecords.Select(item => item.StripName));
            var optionStripNames = new HashSet<DateTime>(OptionRecords.Select(item => item.StripName));
            IsConform = optionStripNames.IsSubsetOf(futureStripNames);
            return IsConform;
        }
    }
}
