using System;
using System.Collections.Generic;
using System.Globalization;
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
        public static List<EOD_Options_Selected> OptionsRecordsSelected;
        public static List<DateTime> JsonConfigDates;
        public static bool FuturesOnly;
        public static string JsonString;
        public static JsonConfig JsonConfig;

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
                    return OptionRecords != null && FutureRecords != null && JsonConfig != null;
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

            if (!FuturesOnly) ProductName = JsonConfig.ICE_Configuration.TMLDB_Description;

            if (FuturesOnly || ConformityCheck() && !string.IsNullOrEmpty(ProductName))
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

            ParsedData.ParseRegularOptions();

            string formText = "ICE Import (DB Form)";

            if (FutureRecords.Count != 0 && OptionRecords.Count != 0)
            {

                IsConform = ProductName != null;
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
            HashSet<DateTime> optionNotFound = new HashSet<DateTime>();
            OptionsRecordsSelected = new List<EOD_Options_Selected>();
            foreach (var option in OptionRecords)
            {
                if (futureStripNames.Contains(option.StripName))
                {
                    OptionsRecordsSelected.Add(new EOD_Options_Selected
                    {
                        DateNameForFuture = option.StripName,
                        EOD_Option = option
                    });
                }
                else if(!JsonConfigDates.Contains(option.StripName))
                {
                    var stripName = futureStripNames.Where(item => item < option.StripName).First();
                    OptionsRecordsSelected.Add(new EOD_Options_Selected
                    {
                        DateNameForFuture = stripName,
                        EOD_Option = option
                    });
                }
                else
                {
                    optionNotFound.Add(option.StripName);
                }
            }
            IsConform = optionNotFound.Count == 0;
            if (!IsConform)
            {
                var sb = new StringBuilder(
                    "The selected Future CSV Files(s) and Option CSV File(s) do not conform to each other.\n\n" +
                    "Options with the following values of StripName do not have corresponding futures " +
                    "(only the first 10 values are shown):\n\n");
                foreach (DateTime optionStripName in optionNotFound)
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

        public static void ParseRegularOptions()
        {
            if(ParsedData.FutureRecords != null)
            {
                JsonConfigDates = new List<DateTime>();
                HashSet<int> years = new HashSet<int>(FutureRecords.Select(item => item.StripName.Year));
                foreach (var date in JsonConfig.ICE_Configuration.Regular_Options)
                {
                    foreach(var year in years)
                    {
                        CultureInfo culture = new CultureInfo("en-En");
                        string dateConv = string.Format(date + year);
                        JsonConfigDates.Add(Convert.ToDateTime(dateConv));
                    }
                }
            }
        }

    }
}
