using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormCSV : Form
    {
        private void backgroundWorker_ParsingOptions_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            BgWorkerCommon.Parse<EOD_Option_CSV, EOD_Option>(
                worker,
                OptionFilePaths,
                "Options",
                FilterRowAndGetProductName,
                FilterRows,
                out ParsedData.OptionProductName,
                out ParsedData.OptionRecords);

            e.Result = worker.CancellationPending;
        }

        private void backgroundWorker_ParsingOptions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BgWorkerCommon.ProgressChanged(e, OptionFilePaths, label_ParsedOption, progressBar_ParsingOption);
        }

        private void backgroundWorker_ParsingOptions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BgWorkerCommon.RunWorkerCompleted(
                e,
                "Options",
                ref ParsedData.OptionRecords,
                this,
                label_ParsedOption,
                progressBar_ParsingOption,
                EnableDisableOption,
                OptionFilePaths.Length);
        }

        static EOD_Option FilterRowAndGetProductName(EOD_Option_CSV csvRow, out string productName)
        {
            productName = csvRow.ProductName;
            return new EOD_Option(csvRow);
        }

        static IEnumerable<EOD_Option> FilterRows(IEnumerable<EOD_Option_CSV> csvRows)
        {
            return csvRows.Select(item => new EOD_Option(item));
        }
    }
}
