using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ICE_Import
{
    public partial class FormCSV : Form
    {
        const string NotSelected = "(not selected)";

        string[] OptionFilePaths;
        string[] FutureFilePaths;

        public FormCSV()
        {
            InitializeComponent();
        }

        private void button_InputOption_Click(object sender, EventArgs e)
        {
            ParsedData.OptionRecords = null;
            ParsedData.OnParseComplete();

            OptionFilePaths = SelectFiles(
                "Options",
                label_InputOption,
                progressBar_ParsingOption);

            if (OptionFilePaths != null)
            {
                // Parse the selected CSV files
                EnableDisableOption(true);
                backgroundWorker_ParsingOptions.RunWorkerAsync();
            }
        }

        private void button_ParseOptions_Click(object sender, EventArgs e)
        {
            EnableDisableOption(true);
            backgroundWorker_ParsingOptions.RunWorkerAsync();
        }

        private void button_CancelOption_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            button_CancelOption.Enabled = false;
            backgroundWorker_ParsingOptions.CancelAsync();
        }

        private void EnableDisableOption(bool start)
        {
            button_InputOption.Enabled = !start;
            button_CancelOption.Enabled = start;
        }

        private void EnableDisableFuture(bool start)
        {
            button_InputFuture.Enabled = !start;
            button_CancelFuture.Enabled = start;
        }

        private void FormCSV_SizeChanged(object sender, EventArgs e)
        {
            progressBar_ParsingOption.Width = Width - 40;
            progressBar_ParsingFuture.Width = Width - 40;
        }

        private void buttonDB_Click(object sender, EventArgs e)
        {
            Hide();
            if (Program.dbf == null)
            {
                Program.dbf = new FormDB();
            }
            Program.dbf.Show();
        }

        private void button_InputFuture_Click(object sender, EventArgs e)
        {
            ParsedData.FutureRecords = null;
            ParsedData.OnParseComplete();

            FutureFilePaths = SelectFiles(
                "Futures",
                label_InputFuture,
                progressBar_ParsingFuture);

            if (FutureFilePaths != null)
            {
                // Parse the selected CSV files
                EnableDisableFuture(true);
                backgroundWorker_ParsingFutures.RunWorkerAsync();
            }
        }

        private void button_ParseFuture_Click(object sender, EventArgs e)
        {
            EnableDisableFuture(true);
            backgroundWorker_ParsingFutures.RunWorkerAsync();
        }

        private void button_CancelFuture_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            button_CancelFuture.Enabled = false;
            backgroundWorker_ParsingFutures.CancelAsync();
        }

        private string[] SelectFiles(
            string expSymbType,
            Label label,
            ProgressBar progressBar)
        {
            label.Text = NotSelected;

            string[] filePaths;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Data file|*.csv";
                dialog.Title = "Select data file(s)";
                dialog.Multiselect = true;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    return null;
                }
                filePaths = dialog.FileNames;
            }

            // Check file names
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (!fileName.Contains(expSymbType))
                {
                    string msg = string.Format("The file name \"{0}\" does not contain \"{1}\".", fileName, expSymbType);
                    MessageBox.Show(msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            label.Text = (filePaths.Length == 1) ? filePaths[0] : "(multiple files)";
            progressBar.Maximum = filePaths.Length;

            return filePaths;
        }

        private void checkBoxFuturesOnly_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = checkBoxFuturesOnly.Checked;
            ParsedData.FuturesOnly = isChecked;

            button_InputOption.Visible = !isChecked;
            label_InputOption.Visible = !isChecked;
            button_CancelOption.Visible = !isChecked;
            label_ParsedOption.Visible = !isChecked;
            progressBar_ParsingOption.Visible = !isChecked;

            ParsedData.OnParseComplete();
        }

        private void button_InputJson_Click(object sender, EventArgs e)
        {
            label_InputJson.Text = NotSelected;
            ParsedData.JsonConfig = null;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Data file|*.json";
                dialog.Title = "Select data file(s)";
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    return;
                }

                try
                {
                    string text = File.ReadAllText(dialog.FileName);
                    ParsedData.JsonConfig = JsonConvert.DeserializeObject<JsonConfig>(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                string msg = ParsedData.JsonConfig.Validate(ParsedData.FuturesOnly);
                if (msg != null)
                {
                    MessageBox.Show(
                        msg,
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    ParsedData.JsonConfig = null;
                    return;
                }

                label_InputJson.Text = dialog.FileName;

                ParsedData.OnParseComplete();
            }
        }

        private void FormCSV_Load(object sender, EventArgs e)
        {
            button_InputJson.Enabled = true;
        }
    }
}
