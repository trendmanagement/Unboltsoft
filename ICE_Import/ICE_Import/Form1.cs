using System;
using System.IO;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class Form1 : Form
    {
        string[] OptionFilePaths;
        string[] FutureFilePaths;

        string[] Symbols = { "EOD_Futures_578", "EOD_Options_578" };

        public Form1()
        {
            InitializeComponent();
        }

        private void button_InputOption_Click(object sender, EventArgs e)
        {
            OptionFilePaths = SelectFiles(
                "Options",
                label_InputOption,
                progressBar_ParsingOption,
                button_ParseOption);
        }

        private void button_ParseOptions_Click(object sender, EventArgs e)
        {
            EnableDisableOption(true);
            backgroundWorker_ParsingOptions.RunWorkerAsync(Symbols[1]);
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

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            progressBar_ParsingOption.Width = Width - 40;
            progressBar_ParsingFuture.Width = Width - 40;
        }

        private void buttonDB_Click(object sender, EventArgs e)
        {
            if(Program.dbf == null)
            {
                Program.dbf = new DataBaseForm();
                Program.dbf.Show();
            }
            else if (Program.dbf.Visible == false)
            {
                Program.dbf = new DataBaseForm();
                Program.dbf.Show();
            }
        }

        private void button_InputFuture_Click(object sender, EventArgs e)
        {
            FutureFilePaths = SelectFiles(
                "Futures",
                label_InputFuture,
                progressBar_ParsingFuture,
                button_ParseFuture);
        }

        private void button_ParseFuture_Click(object sender, EventArgs e)
        {
            EnableDisableFuture(true);
            backgroundWorker_ParsingFutures.RunWorkerAsync(Symbols[0]);
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

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Program.dbf.Visible) Program.dbf.Visible = false;
        }
    }
}
