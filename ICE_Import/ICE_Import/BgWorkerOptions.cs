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
                "options",
                ref ParsedData.OptionRecords,
                this,
                label_ParsedOption,
                progressBar_ParsingOption,
                EnableDisableOption,
                OptionFilePaths.Length);
        }
    }
}
