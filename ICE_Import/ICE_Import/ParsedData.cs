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
        public static List<EOD_Futures> FutureRecords;
        public static List<EOD_Options> OptionRecords;
        public static bool FuturesOnly;

        private static bool IsConform;

        public static string ProductName;

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

            ProductName = FutureRecords[0].ProductName;
            int i = ProductName.IndexOf(" Futures");
            ProductName = ProductName.Remove(i);

            if (FuturesOnly || ConformityCheck())
            {
                Program.csvf.Hide();
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
            string formText = "ICE Import (DB Form)";

            if (FutureRecords.Count != 0 && OptionRecords.Count != 0)
            {
                // Check conformity of ProductName
                string optionProductName = OptionRecords[0].ProductName;
                int i = optionProductName.IndexOf(" Options");
                optionProductName = optionProductName.Remove(i);
                IsConform = ProductName == optionProductName;
                if (!IsConform)
                {
                    MessageBox.Show(
                        "The selected Future CSV Files(s) and Option CSV File(s) do not conform to each other by \"ProductName\" column.",
                        formText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
            }

            // Check conformity of StripName
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
                    "The selected Future CSV Files(s) and Option CSV File(s) do not conform to each other.\n\n" +
                    "Options with the following values of StripName do not have corresponding futures " +
                    "(only the first 10 values are shown):\n\n");
                foreach (DateTime optionStripName in optionStripNames)
                {
                    sb.Append(optionStripName.ToString("MMMyy") + "\n");
                    var items = OptionRecords.Where(option => option.StripName == optionStripName).ToList();
                    foreach(var item in items)
                    {
                        OptionRecords.Remove(item);
                    }
                }
                sb.Append(
                    "\nDo you want to proceed anyway?\n\n" +
                    "(If you select \"Yes\", the extra options will not be pushed to database.)");
                DialogResult result = MessageBox.Show(
                    sb.ToString(),
                    formText,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                IsConform = result == DialogResult.Yes;
            }

            return IsConform;
        }

        /// <summary>
        /// Given "ProductName" from ICE CSV file, get "description" key for [cqgdb].[tblinstruments] table.
        /// </summary>
        public static string GetDescription()
        {
            int idx = ProductName.IndexOf(' ');
            if (idx == -1)
            {
                return ProductName;
            }
            else
            {
                return ProductName.Substring(0, idx);
            }
        }
    }
}
