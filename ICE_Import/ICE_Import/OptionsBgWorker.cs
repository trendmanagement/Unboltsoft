using System.ComponentModel;
using System.Windows.Forms;
using FileHelpers;

namespace ICE_Import
{
    public partial class Form1 : Form
    {
        private void backgroundWorker_ParsingOptions_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Parse<EOD_Options_578>(worker);

            e.Result = worker.CancellationPending;
        }

        private void Parse<T>(BackgroundWorker worker) where T : class
        {
            worker.ReportProgress(0);

            var engine = new FileHelperEngine<T>();
            T[] records = null;

            for (int i = 0; i < OptionFilePaths.Length; i++)
            {
                records = engine.ReadFile(OptionFilePaths[i]);

                worker.ReportProgress(i + 1);

                if (worker.CancellationPending)
                {
                    return;
                }
            }

        StaticData.optionRecords = records;
        }

        private void backgroundWorker_ParsingOptions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int fileIdx = e.ProgressPercentage;
            if (fileIdx < OptionFilePaths.Length)
            {
                label_ParsedOption.Text = OptionFilePaths[fileIdx];
            }
            progressBar_ParsingOption.Value = fileIdx;
        }

        private void backgroundWorker_ParsingOptions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Can't parse options data file!");
            }
            else if ((bool)e.Result)
            {
                MessageBox.Show("Parsing options cancelled by user.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor = Cursors.Default;
            }
            else
            {
                string msg = string.Format("{0} options file(s) has been parsed successfully.", OptionFilePaths.Length);
                MessageBox.Show(msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            label_ParsedOption.Text = string.Empty;
            progressBar_ParsingOption.Value = 0;
            EnableDisableOption(false);

            if (StaticData.optionRecords != null && StaticData.futureRecords != null) StaticData.OnParseComplete();
        }
    }
}
