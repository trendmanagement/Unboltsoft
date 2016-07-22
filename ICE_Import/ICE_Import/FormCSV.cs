using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormCSV : Form
    {
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
                progressBar_ParsingOption,
                button_ParseOption);
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
            button_ParseOption.Enabled = !start;
            button_CancelOption.Enabled = start;
        }

        private void EnableDisableFuture(bool start)
        {
            button_InputFuture.Enabled = !start;
            button_ParseFuture.Enabled = !start;
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
                progressBar_ParsingFuture,
                button_ParseFuture);
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
            ProgressBar progressBar,
            Button button)
        {
            label.Text = "(not selected)";
            button.Enabled = false;

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
            button.Enabled = true;

            return filePaths;
        }

        private void checkBoxFuturesOnly_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = checkBoxFuturesOnly.Checked;
            ParsedData.FuturesOnly = isChecked;
            button_InputOption.Visible = !isChecked;
            button_ParseOption.Visible = !isChecked;
            button_CancelOption.Visible = !isChecked;
            progressBar_ParsingOption.Visible = !isChecked;
            label_ParsedOption.Visible = !isChecked;
            label_InputOption.Visible = !isChecked;
            buttonJson.Visible = !isChecked;

            if (isChecked)
            {
                ParsedData.OptionRecords = null;
            }

            ParsedData.OnParseComplete();
        }

        private void buttonJson_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Data file|*.json";
                dialog.Title = "Select data file(s)";
                dialog.Multiselect = true;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        ParsedData.JsonString = File.ReadAllText(dialog.FileName);
                        ParsedData.JsonConfig = JsonConvert.DeserializeObject<JsonConfig>(ParsedData.JsonString);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message,
                            "Parse json error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                            result = DialogResult.No;
                    }
                }
                labelJson.Text = (ParsedData.JsonConfig == null) ? string.Empty : dialog.SafeFileName;
                ParsedData.OnParseComplete();
            }
        }

        private void FormCSV_Load(object sender, EventArgs e)
        {
            buttonJson.Enabled = true;
        }
    }
}
