using System.Collections.Generic;
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

            BgWorkerCommon.Parse<EOD_Future_CSV, EOD_Future>(
                worker,
                FutureFilePaths,
                "Futures",
                FilterRowAndGetProductName,
                FilterRows,
                out ParsedData.FutureProductName,
                out ParsedData.FutureRecords);

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
                "Futures",
                ref ParsedData.FutureRecords,
                this,
                label_ParsedFuture,
                progressBar_ParsingFuture,
                EnableDisableFuture,
                FutureFilePaths.Length);
        }

        static EOD_Future FilterRowAndGetProductName(EOD_Future_CSV csvRow, out string productName)
        {
            productName = csvRow.ProductName;
            return new EOD_Future(csvRow);
        }

        static IEnumerable<EOD_Future> FilterRows(IEnumerable<EOD_Future_CSV> csvRows)
        {
            return csvRows.Select(item => new EOD_Future(item));
        }
    }
}
