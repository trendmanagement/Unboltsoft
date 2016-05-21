using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        public delegate void LogMessageDelegate(string message);
        public event LogMessageDelegate LogMessage;

        CancellationTokenSource cts;

        string DatabaseName;
        bool IsLocalDB;

        string TablesPrefix;
        bool IsTestTables;

        bool IsStoredProcs;

        string ConnectionString;
        DataClassesTMLDBDataContext Context;

        public FormDB()
        {
            InitializeComponent();

            this.Resize += FormDB_Resize;
            ParsedData.ParseComplete += ParsedData_ParseComplete;
            this.LogMessage += FormDB_LogMessage;
            this.FormClosed += FormDB_FormClosed;
            
            rb_DB_CheckedChanged(rb_LocalDB, null);
            cb_TestTables_CheckedChanged(null, null);
            cb_StoredProcs_CheckedChanged(null, null);
        }

        private void FormDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.csvf.Close();
        }

        private void FormDB_LogMessage(string message)
        {
            richTextBoxLog.Text += message + "\n";
            richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
            richTextBoxLog.ScrollToCaret();
        }

        private void ValuesFromTask(string message, int count)
        {
            if (message != string.Empty)
            {
                richTextBoxLog.Text += message + "\n";
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
            progressBarLoad.Value = count;
        }

        private void FormDB_Resize(object sender, EventArgs e)
        {
            tabControl.Size = new Size()
            {
                Width = this.Width - 15,
                Height = this.Height - 253
            };
            groupBox1.Location = new Point()
            {
                X = groupBox1.Location.X,
                Y = this.Height - 247
            };
            groupBox2.Location = new Point()
            {
                X = groupBox2.Location.X,
                Y = this.Height - 247
            };
            buttonPush.Location = new Point()
            {
                X = buttonPush.Location.X,
                Y = this.Height - 247
            };
            buttonPull.Location = new Point()
            {
                X = buttonPull.Location.X,
                Y = this.Height - 218
            };
            buttonCancel.Location = new Point()
            {
                X = buttonCancel.Location.X,
                Y = this.Height - 175
            };
            progressBarLoad.Location = new Point()
            {
                X = progressBarLoad.Location.X,
                Y = this.Height - 146
            };
            progressBarLoad.Width = tabControl.Width - 15;
            buttonToCSV.Location = new Point()
            {
                X = this.Width - 25 - buttonToCSV.Width,
                Y = this.Height - 247
            };
        }

        private void FormDB_Load(object sender, EventArgs e)
        {
            buttonCancel.Enabled = false;

            FormDB_Resize(sender, e);

            if (!ParsedData.IsReady)
            {
                buttonPush.Enabled = false;
            }
            else
            {
                LogMessage(ParsedData.FutureRecords.GetType().Name.Trim('[', ']') + " entities count: " + ParsedData.FutureRecords.Length.ToString()  + " ready to push to DB");
                if (!ParsedData.FuturesOnly)
                {
                    LogMessage(ParsedData.OptionRecords.GetType().Name.Trim('[', ']') + " entities count: " + ParsedData.OptionRecords.Length.ToString() + " ready to push to DB");
                }

                buttonPush.Enabled = true;
            }
        }

        private void ParsedData_ParseComplete()
        {
            if (buttonPush.Enabled != true)
            {
                LogMessage("Entities count: " + ParsedData.OptionRecords.Length.ToString());
                LogMessage("Type of entity: " + ParsedData.OptionRecords.GetType().Name.Trim('[', ']'));
                buttonPush.Enabled = true;
            }
        }
        
        private async void buttonPush_Click(object sender, EventArgs e)
        {
            if (!ValidateOptions())
            {
                return;
            }

            if (DatabaseName == "TMLDB" && !IsTestTables)
            {
                // Ask confirmation
                var result = MessageBox.Show(
                    "Are you about to update NON-TEST tables of NON-TEST database.\n\nAre you sure?",
                    Text,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            cts = new CancellationTokenSource();

            LogMessage("Pushing started");

            if (IsTestTables)
            {
                if (IsStoredProcs)
                {
                    // await PushDataToDBTestStoredProcedures(cts.Token);
                }
                else
                {
                    await PushDataToDBTest(cts.Token);
                }
            }
            else
            {
                if (IsStoredProcs)
                {
                    await PushDataToDBStoredProcedures(cts.Token);
                }
                else
                {
                    await PushDataToDB(cts.Token);
                }
            }

            buttonPull_Click(sender, e);

            //string input = ParsedData.OptionRecords.GetType().Name.Trim('[', ']');

            //EntityNames name = new EntityNames();
            //Enum.TryParse(input, out name);
            //switch (name)
            //{
            //    case EntityNames.EOD_Futures_578:
            //        // TODO: load data EOD_Futures_578 to db like EOD_Options_578 entity
            //        break;
            //    case EntityNames.EOD_Options_578:
            //        break;
            //    default:
            //        break;
            //}
        }

        private void buttonPull_Click(object sender, EventArgs e)
        {
            if (!ValidateOptions())
            {
                return;
            }

            progressBarLoad.Value = 0;

            if (IsTestTables)
            {
                PullDataFromDBTest();
            }
            else
            {
                PullDataFromDB();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private void buttonToCSV_Click(object sender, EventArgs e)
        {
            Hide();
            Program.csvf.Show();
        }

        private void EnableDisable(bool start)
        {
            rb_LocalDB.Enabled = !start;
            rb_TMLDBCopy.Enabled = !start;
            rb_TMLDB.Enabled = !start;
            cb_StoredProcs.Enabled = !start;
            cb_TestTables.Enabled = !start;
            if (start)
            {
                buttonPush.Enabled = false;
            }
            else
            {
                if (ParsedData.IsReady)
                {
                    buttonPush.Enabled = true;
                }
            }
            buttonPull.Enabled = !start;
            buttonCancel.Enabled = !start;
            buttonToCSV.Enabled = !start;
        }

        private void rb_DB_CheckedChanged(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;
            if (!rb.Checked)
            {
                return;
            }

            int tag = int.Parse((string)rb.Tag);

            string localConnectionStringPattern =
@"Data Source=(localdb)\MSSQLLocalDB;
Integrated Security=True;
AttachDbFileName={0};";

            string remoteConnectionStringPattern =
@"Server=tcp:h9ggwlagd1.database.windows.net,1433;
Database={0};
User ID=dataupdate@h9ggwlagd1;
Password=6dcEpZKSFRNYk^AN;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;";

            // Prepare the connection string
            switch (tag)
            {
                case 1:
                    // Local DB
                    DatabaseName = "LOCAL";
                    IsLocalDB = true;
                    string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "DatabaseLocal.mdf");
                    ConnectionString = string.Format(localConnectionStringPattern, filePath);
                    break;
                case 2:
                    // TMLDB_Copy
                    DatabaseName = "TMLDB_Copy";
                    IsLocalDB = false;
                    ConnectionString = string.Format(remoteConnectionStringPattern, "TMLDB_Copy");
                    break;
                case 3:
                    // TMLDB
                    DatabaseName = "TMLDB";
                    IsLocalDB = false;
                    ConnectionString = string.Format(remoteConnectionStringPattern, "TMLDB");
                    break;
                default:
                    throw new ArgumentException();
            }

            // Change DB context
            Context = new DataClassesTMLDBDataContext(ConnectionString);

            LogMessage(string.Format("You selected {0} database", DatabaseName));
        }

        private void cb_TestTables_CheckedChanged(object sender, EventArgs e)
        {
            IsTestTables = cb_TestTables.Checked;

            string prefix;
            string testNonTest;
            if (IsTestTables)
            {
                TablesPrefix = "TEST_";
                prefix = "test_";
                testNonTest = "TEST";
            }
            else
            {
                TablesPrefix = string.Empty;
                prefix = string.Empty;
                testNonTest = "NON-TEST";
            }

            // Rename tabs
            tabPageContract.Text = prefix + "tblcontracts";
            tabPageDailyContract.Text = prefix + "tbldailycontractsettlements";
            tabPageOption.Text = prefix + "tbloption";
            tabPageOptionData.Text = prefix + "tbloptiondata";

            LogMessage(string.Format("You selected {0} tables", testNonTest));
        }

        private void cb_StoredProcs_CheckedChanged(object sender, EventArgs e)
        {
            IsStoredProcs = cb_StoredProcs.Checked;

            string storedCoded = IsStoredProcs ? "STORED" : "CODED";
            LogMessage(string.Format("You selected {0} procedures", storedCoded));
        }

        private bool ValidateOptions()
        {
            if (DatabaseName == "TMLDB" && IsTestTables && IsStoredProcs)
            {
                MessageBox.Show(
                    "TMLDB does not have stored procedures for working with test tables.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            else if (DatabaseName == "TMLDB_Copy" && IsTestTables)
            {
                MessageBox.Show(
                    "TMLDB_Copy does not have test tables.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
