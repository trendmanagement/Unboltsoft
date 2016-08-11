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
        public static List<EOD_Future> FutureRecords;
        public static List<EOD_Option> OptionRecords;
        public static List<EOD_Option_Selected> OptionsRecordsSelected;
        public static bool FuturesOnly;
        public static string FutureProductName;
        public static string OptionProductName;

        public static string JsonPath;
        public static JsonConfig JsonConfig;
        public static string Description;
        public static int? NormalizeConst;
        public static double? OptionTickSize;

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


            if (!FuturesOnly) Description = JsonConfig.ICE_Configuration.TMLDB_Description;

            if (FuturesOnly || ConformityCheck() && !string.IsNullOrEmpty(Description))
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
            List<DateTime> jsonConfigDates = ParsedData.ParseRegularOptions();

            string formText = "ICE Import (CSV Form)";

            if (FutureRecords.Count != 0 && OptionRecords.Count != 0)
            {
                string futureProductName = ParsedData.FutureProductName.Replace(" Futures", string.Empty);
                string optionProductName = ParsedData.OptionProductName.Replace(" Options", string.Empty);
                IsConform = (futureProductName == optionProductName);
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
            OptionsRecordsSelected = new List<EOD_Option_Selected>();
            foreach (var option in OptionRecords)
            {
                if (futureStripNames.Contains(option.StripName))
                {
                    OptionsRecordsSelected.Add(new EOD_Option_Selected(option.StripName, option));
                }
                else if (!jsonConfigDates.Contains(option.StripName))
                {
                    var stripName = futureStripNames.Where(item => item < option.StripName).First();
                    OptionsRecordsSelected.Add(new EOD_Option_Selected(stripName, option));
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
                int i = 0;
                foreach (DateTime optionStripName in optionNotFound)
                {
                    if (i < 10)
                    {
                        sb.Append(optionStripName.ToString("MMMyy") + "\n");
                        i++;
                    }
                    OptionRecords.RemoveAll(option => option.StripName == optionStripName);
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

        public static List<DateTime> ParseRegularOptions()
        {
            var jsonConfigDates = new List<DateTime>();
            if (ParsedData.FutureRecords != null)
            {
                HashSet<int> intYears = new HashSet<int>(FutureRecords.Select(item => item.StripName.Year));
                List<string> years = intYears.Select(year => year.ToString()).ToList();
                foreach (string month in JsonConfig.ICE_Configuration.Regular_Options)
                {
                    foreach (string year in years)
                    {
                        string monthYear = month + year;
                        jsonConfigDates.Add(Convert.ToDateTime(monthYear));
                    }
                }
            }
            return jsonConfigDates;
        }

    }
}
