using System.ComponentModel;
using System.Windows.Forms;
using FileHelpers;

namespace ICE_Import
{
    public partial class Form1 : Form
    {
        private void backgroundWorker_Parsing_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int selectedIndex = (int)e.Argument;

            if (selectedIndex == 0)
            {
                Parse<EOD_Futures_578>(worker);
            }
            else
            {
                Parse<EOD_Options_578>(worker);
            }

            e.Result = worker.CancellationPending;
        }

        private void Parse<T>(BackgroundWorker worker)
            where T : class
        {
            worker.ReportProgress(0);

            var engine = new FileHelperEngine<T>();

            for (int i = 0; i < FilePaths.Length; i++)
            {
                T[] records = engine.ReadFile(FilePaths[i]);

                // TODO: Push the records to the database

                worker.ReportProgress(i + 1);

                if (worker.CancellationPending)
                {
                    return;
                }
            }
        }

        private void backgroundWorker_Parsing_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int fileIdx = e.ProgressPercentage;
            if (fileIdx < FilePaths.Length)
            {
                label_ParsedFile.Text = FilePaths[fileIdx];
            }
            progressBar_Parsing.Value = fileIdx;
        }

        private void backgroundWorker_Parsing_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
#if !DEBUG
                MessageBox.Show(e.Error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }
            else if ((bool)e.Result)
            {
                MessageBox.Show("Cancelled by user.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor = Cursors.Default;
            }
            else
            {
                string msg = string.Format("{0} file(s) has been parsed successfully.", FilePaths.Length);
                MessageBox.Show(msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            label_ParsedFile.Text = string.Empty;
            progressBar_Parsing.Value = 0;
            EnableDisable(false);
        }
    }
}
