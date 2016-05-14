using System.ComponentModel;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormCSV : Form
    {
        private void backgroundWorker_ParsingFutures_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ParsedData.FutureRecords = Parse<EOD_Futures_578>(worker, FutureFilePaths);

            e.Result = worker.CancellationPending;
        }

        private void backgroundWorker_ParsingFutures_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int fileIdx = e.ProgressPercentage;
            if (fileIdx < FutureFilePaths.Length)
            {
                label_ParsedFuture.Text = FutureFilePaths[fileIdx];
            }
            progressBar_ParsingFuture.Value = fileIdx;
        }

        private void backgroundWorker_ParsingFutures_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Can't parse futures data file!");
            }
            else if ((bool)e.Result)
            {
                MessageBox.Show("Parsing futures data cancelled by user.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor = Cursors.Default;
            }
            else
            {
                string msg = string.Format("{0} futures file(s) has been parsed successfully.", FutureFilePaths.Length);
                MessageBox.Show(msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            label_ParsedFuture.Text = string.Empty;
            progressBar_ParsingFuture.Value = 0;
            EnableDisableFuture(false);

            if (ParsedData.IsReady)
            {
                ParsedData.OnParseComplete();
            }
        }
    }
}
