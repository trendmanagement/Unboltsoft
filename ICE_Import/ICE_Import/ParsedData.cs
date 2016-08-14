using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ICE_Import
{
    internal static class ParsedData
    {
        private const string FormText = "ICE Import (CSV Form)";

        public delegate void ParseEventHandler();

        public static event ParseEventHandler ParseSucceeded;
        public static event ParseEventHandler ParseFailed;

        public static List<EOD_Future> FutureRecords;
        public static List<EOD_Option> OptionRecords;
        public static JsonConfig JsonConfig;

        public static List<EOD_Option_Selected> OptionsRecordsSelected;
        public static bool FuturesOnly;
        public static string FutureProductName;
        public static string OptionProductName;

        public static int? StrikePriceToCQGSymbolFactor;

        private static bool IsConform;

        public static void OnParseComplete()
        {
            bool isReady = AreCsvParsed && CsvCsvJsonConformityCheck();

            if (isReady)
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

        public static bool IsReady
        {
            get
            {
                return AreCsvParsed && IsConform;
            }
        }

        /// <summary>
        /// Check conformity between future CSV file(s), option CSV file(s) and JSON file.
        /// Also, initialize IsConform data member to skip double checks.
        /// </summary>
        private static bool CsvCsvJsonConformityCheck()
        {
            IsConform = false;

            // We already checked that FutureRecords (and OptionRecords) are not empty

            string futureProductName = ParsedData.FutureProductName.Replace(" Futures", string.Empty);
            if (!FuturesOnly)
            {
                // Check conformity of ProductName between futures and options
                string optionProductName = ParsedData.OptionProductName.Replace(" Options", string.Empty);
                if (futureProductName != optionProductName)
                {
                    MessageBox.Show(
                        "The selected Futures CSV File(s) and Options CSV File(s) do not conform to each other by \"ProductName\" column.",
                        FormText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return IsConform;
                }
            }

            if (!IsJsonParsed)
            {
                return IsConform;
            }
            else
            {
                // Check JSON
                string msg = JsonConfig.Validate(FuturesOnly);
                if (msg != null)
                {
                    MessageBox.Show(
                        msg,
                        FormText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return IsConform;
                }
            }

            // Check conformity of ProductName between CSV and JSON
            if (futureProductName != JsonConfig.ICE_ProductName)
            {
                MessageBox.Show(
                    "The selected CSV File(s) do not conform to the JSON File by the product name.",
                    FormText,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return IsConform;
            }

            if (FuturesOnly)
            {
                IsConform = true;
                return IsConform;
            }

            List<DateTime> jsonConfigDates = ParsedData.ParseRegularOptions();

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
                    "The selected Futures CSV File(s) and Options CSV File(s) do not conform to each other.\n\n" +
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
                    FormText,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                IsConform = (result == DialogResult.Yes);
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
                foreach (string month in JsonConfig.Regular_Options)
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

        private static bool AreCsvParsed
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

        private static bool IsJsonParsed
        {
            get
            {
                return JsonConfig != null;
            }
        }
    }
}
