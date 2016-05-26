﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        bool IsAsyncUpdate;

        string ConnectionString;
        DataClassesTMLDBDataContext Context;

        public FormDB()
        {
            InitializeComponent();

            this.Resize += FormDB_Resize;
            this.LogMessage += FormDB_LogMessage;
            this.FormClosed += FormDB_FormClosed;
            ParsedData.ParseComplete += ParsedData_ParseComplete;
            AsyncTaskListener.Updated += AsyncTaskListener_Updated;

            rb_DB_CheckedChanged(rb_LocalDB, null);
            cb_TestTables_CheckedChanged(null, null);
            cb_StoredProcs_CheckedChanged(null, null);
            cb_AsyncUpdate_CheckedChanged(null, null);
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
            progressBar.Location = new Point()
            {
                X = progressBar.Location.X,
                Y = this.Height - 146
            };
            progressBar.Width = this.Width - 40;
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
                ParsedData_ParseComplete();
            }
        }

        private void ParsedData_ParseComplete()
        {
            string pat = "{0} entries count: {1} (ready for pushing to DB)";
            string msg = string.Format(
                pat,
                ParsedData.FutureRecords.GetType().Name.Trim('[', ']'),
                ParsedData.FutureRecords.Length);
            LogMessage(msg);
            if (!ParsedData.FuturesOnly)
            {
                msg = string.Format(
                    pat,
                    ParsedData.OptionRecords.GetType().Name.Trim('[', ']'),
                    ParsedData.OptionRecords.Length);
                LogMessage(msg);
            }

            buttonPush.Enabled = true;
        }
        
        private async void buttonPush_Click(object sender, EventArgs e)
        {
            if (!ValidateOptions(true))
            {
                return;
            }

            EnableDisable(true);

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

            LogMessage("Pushing started");

            cts = new CancellationTokenSource();

            try
            {
                if (IsStoredProcs)
                {
                    // Install stored procedures from SQL files into DB
                    await Task.Run(() =>
                        StoredProcsInstallator.Install(ConnectionString, IsTestTables, cts.Token));

                    if (IsAsyncUpdate)
                    {
                        await PushDataToDBWithSPsAsync(cts.Token);
                    }
                    else
                    {
                        // Update either test and non-test tables
                        await PushDataToDBWithSPs(cts.Token);
                    }
                }
                else
                {
                    if (IsTestTables)
                    {
                        await PushDataToDBTest(cts.Token);
                    }
                    else
                    {
                        await PushDataToDB(cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
                // The form was closed during the process
            }

            // Update the data grid
            buttonPull_Click(sender, e);
        }

        private void buttonPull_Click(object sender, EventArgs e)
        {
            if (!ValidateOptions())
            {
                return;
            }

            EnableDisable(true);

            try
            {
                if (IsTestTables)
                {
                    PullDataFromDBTest();
                }
                else
                {
                    PullDataFromDB();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
                // The form was closed during the process
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
            cb_TestTables.Enabled = !start;
            cb_StoredProcs.Enabled = !start;
            cb_AsyncUpdate.Enabled = !start;
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
                labelRPS.Text = string.Empty;
            }
            buttonPull.Enabled = !start;
            buttonCancel.Enabled = start;
            buttonToCSV.Enabled = !start;
            progressBar.Value = 0;
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
            StoredProcsSwitch.Update(Context, IsTestTables);

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

            // Update stored procs switch
            StoredProcsSwitch.Update(Context, IsTestTables);

            LogMessage(string.Format("You selected {0} tables", testNonTest));
        }

        private void cb_StoredProcs_CheckedChanged(object sender, EventArgs e)
        {
            IsStoredProcs = cb_StoredProcs.Checked;

            cb_AsyncUpdate.Enabled = IsStoredProcs;
            if (!IsStoredProcs)
            {
                cb_AsyncUpdate.Checked = false;
            }

            string storedCoded = IsStoredProcs ? "STORED" : "CODED";
            LogMessage(string.Format("You selected {0} procedures", storedCoded));
        }

        private void cb_AsyncUpdate_CheckedChanged(object sender, EventArgs e)
        {
            IsAsyncUpdate = cb_AsyncUpdate.Checked;

            string asyncSync = IsAsyncUpdate ? "ASYNCHRONOUS" : "SYNCHRONOUS";
            LogMessage(string.Format("You selected {0} update", asyncSync));
        }

        private void AsyncTaskListener_Updated(
            string message = null,
            int progress = -1,
            double rps = double.NaN)
        {
            Action action = new Action(
                () =>
                {
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        richTextBoxLog.Text += message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    if (progress != -1)
                    {
                        progressBar.Value = progress;
                    }
                    if (!double.IsNaN(rps))
                    {
                        labelRPS.Text = Math.Round(rps).ToString();
                    }
                });

            try
            {
                Invoke(action);
            }
            catch (ObjectDisposedException)
            {
                // User closed the form
            }
        }

        private bool ValidateOptions(bool isPush = false)
        {
            if (isPush && DatabaseName == "TMLDB" && IsTestTables && IsStoredProcs)
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

        private void LogElapsedTime(TimeSpan timeSpan)
        {
            LogMessage("Elapsed time: " + timeSpan);
        }
    }
}
