using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormCSV : Form
    {
        private void backgroundWorker_ParsingFutures_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            ParsedData.FutureRecords = BgWorkerCommon.Parse<EOD_Futures>(worker, FutureFilePaths);

            e.Result = worker.CancellationPending;
        }

        private void backgroundWorker_ParsingFutures_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BgWorkerCommon.ProgressChanged(e, FutureFilePaths, label_ParsedFuture, progressBar_ParsingFuture);
        }

        private void backgroundWorker_ParsingFutures_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BgWorkerCommon.RunWorkerCompleted(
                e,
                "futures",
                ref ParsedData.FutureRecords,
                ParsedData.FutureRecords.Select(item => item.ProductName),
                this,
                label_ParsedFuture,
                progressBar_ParsingFuture,
                EnableDisableFuture,
                FutureFilePaths.Length);
        }
    }
}
