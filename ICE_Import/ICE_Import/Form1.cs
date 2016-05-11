using System;
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
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Data file|*.csv";
                dialog.Title = "Select data file(s)";
                dialog.Multiselect = true;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                OptionFilePaths = dialog.FileNames;
            }

            label_InputOption.Text = (OptionFilePaths.Length == 1) ? OptionFilePaths[0] : "(multiple files)";
            progressBar_ParsingOption.Maximum = OptionFilePaths.Length;
            button_ParseOption.Enabled = true;
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
            if(StaticData.dbf == null)
            {
                StaticData.dbf = new DataBaseForm();
                StaticData.dbf.Show();
            }
            else if (StaticData.dbf.Visible == false)
            {
                StaticData.dbf = new DataBaseForm();
                StaticData.dbf.Show();
            }

        }

        private void button_InputFuture_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Data file|*.csv";
                dialog.Title = "Select data file(s)";
                dialog.Multiselect = true;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                FutureFilePaths = dialog.FileNames;
            }
            label_InputFuture.Text = (FutureFilePaths.Length == 1) ? FutureFilePaths[0] : "(multiple files)";
            progressBar_ParsingFuture.Maximum = FutureFilePaths.Length;
            button_ParseFuture.Enabled = true;
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
    }
}
