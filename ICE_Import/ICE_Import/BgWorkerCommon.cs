using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileHelpers;

namespace ICE_Import
{
    static class BgWorkerCommon
    {
        private const string formText = "ICE Import (CSV Form)";

        public static void ProgressChanged(
            ProgressChangedEventArgs e,
            string[] filePaths,
            Label label,
            ProgressBar progressBar)
        {
            int fileIdx = e.ProgressPercentage;
            if (fileIdx < filePaths.Length)
            {
                label.Text = filePaths[fileIdx];
            }
            progressBar.Value = fileIdx;
        }

        public static void Parse<T1, T2>(
            BackgroundWorker worker,
            string[] filePaths,
            out string productName,
            out List<T2> records)
            where T1 : class
        {
            productName = null;
            records = new List<T2>();

            worker.ReportProgress(0);

            for (int i = 0; i < filePaths.Length; i++)
            {
                if (worker.CancellationPending)
                {
                    productName = null;
                    records = null;
                    return;
                }

                try
                {
                    using (TextReader textReader = new StreamReader(filePaths[i]))
                    {
                        var engine = new FileHelperAsyncEngine<T1>();

                        using (engine.BeginReadStream(textReader))
                        {
                            // Parse the first row from input CSV file
                            T1 csvRow = engine.ReadNext();

                            if (csvRow == null)
                            {
                                // There is no data rows in this CSV file -- just skip it
                                continue;
                            }

                            // Remove extra columns and get the product name
                            string thisProductName;
                            T2 filteredRow = FilterRowAndGetProductName((dynamic)csvRow, out thisProductName);

                            // Check the product name consistency
                            if (productName != null && thisProductName != productName)
                            {
                                MessageBox.Show(
                                    "The selected CSV Files(s) do not conform to each other by \"ProductName\" column.",
                                    formText,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                productName = null;
                                records = null;
                                return;
                            }

                            // Save the first row data
                            productName = thisProductName;
                            records.Add(filteredRow);

                            // Parse all other rows from the CSV file lazily
                            IEnumerable<T1> csvRows = engine.AsEnumerable<T1>();

                            // Remove extra columns lazily
                            IEnumerable<T2> filteredRows = FilterRows((dynamic)csvRows);

                            // Add new rows to the common list
                            records.AddRange(filteredRows);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        formText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    productName = null;
                    records = null;
                    return;
                }

                worker.ReportProgress(i + 1);
            }
        }

        private static EOD_Future FilterRowAndGetProductName(EOD_Future_CSV csvRow, out string productName)
        {
            productName = csvRow.ProductName;
            return new EOD_Future(csvRow);
        }

        private static EOD_Option FilterRowAndGetProductName(EOD_Option_CSV csvRow, out string productName)
        {
            productName = csvRow.ProductName;
            return new EOD_Option(csvRow);
        }

        private static IEnumerable<EOD_Future> FilterRows(IEnumerable<EOD_Future_CSV> csvRows)
        {
            return csvRows.Select(item => new EOD_Future(item));
        }

        private static IEnumerable<EOD_Option> FilterRows(IEnumerable<EOD_Option_CSV> csvRows)
        {
            return csvRows.Select(item => new EOD_Option(item));
        }

        public delegate void EnableDisableDelegate(bool start);

        public static void RunWorkerCompleted<T>(
            RunWorkerCompletedEventArgs e,
            string names,
            ref List<T> records,
            FormCSV form,
            Label label,
            ProgressBar progressBar,
            EnableDisableDelegate EnableDisable,
            int numFiles)
        {
            if (records != null)
            {
                if (e.Error != null)
                {
                    // Failed
                    string msg = string.Format("Failed to parse {0} file(s).", names);
                    MessageBox.Show(msg, form.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    records = null;
                }
                else if ((bool)e.Result)
                {
                    // Cancelled
                    string msg = string.Format("Parsing {0} file(s) cancelled by user.", names);
                    MessageBox.Show(msg, form.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.Cursor = Cursors.Default;
                    records = null;
                }
                else
                {
                    // Parsed

                    string msg;

                    if (records.Count == 0)
                    {
                        msg = string.Format("There is no records in the selected {0} file(s).", names);
                        MessageBox.Show(msg, form.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        records = null;
                    }
                    else
                    {
                        msg = string.Format("{0} {1} file(s) parsed successfully.", numFiles, names);
                        MessageBox.Show(msg, form.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            label.Text = string.Empty;
            progressBar.Value = 0;
            EnableDisable(false);

            ParsedData.OnParseComplete();
        }
    }
}
