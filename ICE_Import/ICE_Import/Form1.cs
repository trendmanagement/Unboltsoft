using System;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class Form1 : Form
    {
        string[] FilePaths;

        string[] Symbols = { "EOD_Futures_578", "EOD_Options_578" };

        public Form1()
        {
            InitializeComponent();
            comboBox_Symbol.Items.AddRange(Symbols);
            comboBox_Symbol.SelectedIndex = 0;
        }

        private void button_InputFile_Click(object sender, EventArgs e)
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
                FilePaths = dialog.FileNames;
            }

            label_InputFile.Text = (FilePaths.Length == 1) ? FilePaths[0] : "(multiple files)";
            progressBar_Parsing.Maximum = FilePaths.Length;
            button_Parse.Enabled = true;
        }

        private void button_Parse_Click(object sender, EventArgs e)
        {
            EnableDisable(true);
            backgroundWorker_Parsing.RunWorkerAsync(comboBox_Symbol.SelectedIndex);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            button_Cancel.Enabled = false;
            backgroundWorker_Parsing.CancelAsync();
        }

        private void EnableDisable(bool start)
        {
            button_InputFiles.Enabled = !start;
            comboBox_Symbol.Enabled = !start;
            button_Parse.Enabled = !start;
            button_Cancel.Enabled = start;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            progressBar_Parsing.Width = Width - 40;
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
    }
}
