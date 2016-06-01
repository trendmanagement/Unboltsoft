using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            if (!IsParsed)
            {
                // Raise event
                ParseFailed();
                return;
            }

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
                // Raise event
                ParseFailed();
            }
        }
        
        private static bool ConformityCheck()
        {
            var futureStripNames = new HashSet<DateTime>(FutureRecords.Select(item => item.StripName));
            var optionStripNames = new HashSet<DateTime>(OptionRecords.Select(item => item.StripName));
            foreach (DateTime futureStripName in futureStripNames)
            {
                optionStripNames.Remove(futureStripName);
            }
            IsConform = optionStripNames.Count == 0;

            if (!IsConform)
            {
                var sb = new StringBuilder(
                    "The selected Future CSV Files(s) and Option CSV File(s) do not conform to each other.\r\n\r\n" +
                    "Options with the following values of StripName do not have corresponding futures " +
                    "(only the first 10 values are shown):\r\n\r\n");
                foreach (DateTime optionStripName in optionStripNames.Take(10))
                {
                    sb.Append(optionStripName.ToString("MMMyy") + "\r\n");
                }
                sb.Append("\r\nDo you want to proceed anyway?");
                DialogResult result = MessageBox.Show(sb.ToString(), "ICE Import (DB Form)", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                IsConform = result == DialogResult.Yes;
            }
            return IsConform;
        }
    }
}
