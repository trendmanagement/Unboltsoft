using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FileHelpers;
using System;

namespace ICE_Import
{
    static class BgWorkerCommon
    {
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

        public static List<T> Parse<T>(
            BackgroundWorker worker,
            string[] filePaths)
            where T : class
        {
            worker.ReportProgress(0);

            var engine = new FileHelperEngine<T>();
            var records = new List<T>();

            for (int i = 0; i < filePaths.Length; i++)
            {
                if (worker.CancellationPending)
                {
                    return null;
                }

                try
                {
                    T[] newRecords = engine.ReadFile(filePaths[i]);
                    records.AddRange(newRecords);
                    worker.ReportProgress(i + 1);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                    ex.Message,
                    "Cant parse value with error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                }

            }

            return records;
        }

        public delegate void EnableDisableDelegate(bool start);

        public static void RunWorkerCompleted<T>(
            RunWorkerCompletedEventArgs e,
            string names,
            ref List<T> records,
            IEnumerable<string> productNames,
            FormCSV form,
            Label label,
            ProgressBar progressBar,
            EnableDisableDelegate EnableDisable,
            int numFiles)
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
                    var productNamesSet = new HashSet<string>(productNames);
                    if (productNamesSet.Count > 1)
                    {
                        msg = string.Format("\"ProductName\" column values are not the same for all records of {0} file(s).", names);
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
