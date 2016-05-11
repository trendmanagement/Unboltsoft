using System.ComponentModel;
using System.Windows.Forms;
using FileHelpers;

namespace ICE_Import
{
    public partial class Form1 : Form
    {
        private void backgroundWorker_ParsingFutures_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ParseFutures<EOD_Futures_578>(worker);

            e.Result = worker.CancellationPending;
        }

        private void ParseFutures<T>(BackgroundWorker worker) where T : class
        {
            worker.ReportProgress(0);

            var engine = new FileHelperEngine<T>();
            T[] records = null;

            for (int i = 0; i < FutureFilePaths.Length; i++)
            {
                records = engine.ReadFile(FutureFilePaths[i]);

                worker.ReportProgress(i + 1);

                if (worker.CancellationPending)
                {
                    return;
                }
            }

        StaticData.futureRecords = records;
        }

        private void backgroundWorker_ParsingFutures_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int fileIdx = e.ProgressPercentage;
            if (fileIdx < FutureFilePaths.Length)
            {
                label_ParsedOption.Text = FutureFilePaths[fileIdx];
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

            label_ParsedOption.Text = string.Empty;
            progressBar_ParsingFuture.Value = 0;
            EnableDisableFuture(false);

            if (StaticData.optionRecords != null && StaticData.futureRecords != null) StaticData.OnParseComplete();
        }
    }
}
