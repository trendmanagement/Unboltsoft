using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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
        string StoredProcPrefix;

        bool IsAsyncUpdate;

        ConnectionStrings ConnectionStrings = new ConnectionStrings();

        string ConnectionString;
        DataClassesTMLDBDataContext Context;

        TMLDBReader TMLDBReader;

        long IdInstrument = -1;
        string CqgSymbol;
        double RiskFreeInterestRate = double.NaN;
        double TickSize = double.NaN;

        public FormDB()
        {
            InitializeComponent();

            this.Resize += FormDB_Resize;
            this.LogMessage += FormDB_LogMessage;
            this.FormClosed += FormDB_FormClosed;
            ParsedData.ParseSucceeded += ParsedData_ParseSucceeded;
            ParsedData.ParseFailed += ParsedData_ParseFailed;
            AsyncTaskListener.Updated += AsyncTaskListener_Updated;

            rb_DB_CheckedChanged(rb_LocalDB, null);
            cb_TestTables_CheckedChanged(null, null);
            cb_StoredProcs_CheckedChanged(null, null);
            cb_AsyncUpdate_CheckedChanged(null, null);

            var contextTMLDB = new DataClassesTMLDBDataContext(ConnectionStrings.TMLDB);
            TMLDBReader = new TMLDBReader(contextTMLDB);
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
            buttonCheckPushedData.Location = new Point()
            {
                X = buttonCheckPushedData.Location.X,
                Y = this.Height - 247
            };
            labelRPS1.Location = new Point()
            {
                X = this.Width - 103,
                Y = this.Height - 170
            };
            labelRPS2.Location = new Point()
            {
                X = this.Width - 65,
                Y = this.Height - 170
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
            buttonCheckPushedData.Enabled = false;

            FormDB_Resize(sender, e);

            if (!ParsedData.IsReady)
            {
                ParsedData_ParseFailed();
            }
        }

        private void ParsedData_ParseSucceeded()
        {
            string pat = "{0} entries count: {1} (ready for pushing to DB)";
            string msg = string.Format(
                pat,
                "EOD_Futures",
                ParsedData.FutureRecords.Count);
            LogMessage(msg);
            if (!ParsedData.FuturesOnly)
            {
                msg = string.Format(
                    pat,
                    "EOD_Options",
                    ParsedData.OptionRecords.Count);
                LogMessage(msg);
            }

            buttonPush.Enabled = true;
        }

        private void ParsedData_ParseFailed()
        {
            buttonPush.Enabled = false;
        }

        private async void buttonPush_Click(object sender, EventArgs e)
        {
            if (!ValidateOptions(true))
            {
                return;
            }

            if (DatabaseName == "TMLDB" && !IsTestTables)
            {
                // Ask confirmation
                var result = MessageBox.Show(
                    "You are about to update NON-TEST tables of NON-TEST database.\n\nAre you sure?",
                    Text,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            EnableDisable(true);

            bool areThreeParamsFound = await Task.Run(
                () => TMLDBReader.GetThreeParams(
                    ParsedData.GetDescription(),
                    ref IdInstrument,
                    ref CqgSymbol,
                    ref TickSize));

            bool isRiskFound = await Task.Run(
                () => TMLDBReader.GetRisk(
                    ref RiskFreeInterestRate));

            if (!areThreeParamsFound || !isRiskFound)
            {
                MessageBox.Show(
                    "While looking in TMLDB, a value for at least one of the following parameters was not found:\n\n" +
                    "ID Instrument\nCQG Symbol\nTick Size\nRisk Free Interest Rate",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                EnableDisable(false);
                return;
            }

            cts = new CancellationTokenSource();

            try
            {
                if (IsStoredProcs)
                {
                    // Install stored procedures from SQL files into DB
                    bool success = await Task.Run(() =>
                        StoredProcsInstallator.Install(ConnectionString, IsTestTables, cts.Token));
                    if (!success)
                    {
                        EnableDisable(false);
                        return;
                    }

                    // Update either test or non-test tables,
                    // either synchronously or asynchronously
                    await PushDataToDBWithSPs(cts.Token);
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
                labelRPS2.Text = string.Empty;
            }
            buttonPull.Enabled = !start;
            buttonCancel.Enabled = start;
            buttonToCSV.Enabled = !start;
            buttonCheckPushedData.Enabled = start ? false : (dataGridViewContract.DataSource != null);
            //buttonCheckPushedData.Enabled = false;
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

            // Prepare the connection string
            switch (tag)
            {
                case 1:
                    // Local DB
                    DatabaseName = "Local";
                    IsLocalDB = true;
                    ConnectionString = ConnectionStrings.Local;
                    break;
                case 2:
                    // TMLDB_Copy
                    DatabaseName = "TMLDB_Copy";
                    IsLocalDB = false;
                    ConnectionString = ConnectionStrings.TMLDB_Copy;
                    break;
                case 3:
                    // TMLDB
                    DatabaseName = "TMLDB";
                    IsLocalDB = false;
                    ConnectionString = ConnectionStrings.TMLDB;
                    break;
                default:
                    throw new ArgumentException();
            }

            // Change DB context
            if (tag != 3)
            {
                Context = new DataClassesTMLDBDataContext(ConnectionString);
            }
            else
            {
                Context = TMLDBReader.Context;
            }

            if (IsStoredProcs)
            {
                // Switch stored procs helper
                StoredProcsHelper.Switch(Context, IsTestTables, IsAsyncUpdate, ConnectionString);
            }

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

            StoredProcPrefix = prefix;

            if (IsStoredProcs)
            {
                // Switch stored procs helper
                StoredProcsHelper.Switch(Context, IsTestTables, IsAsyncUpdate, ConnectionString);
            }

            LogMessage(string.Format("You selected {0} tables", testNonTest));
        }

        private void cb_StoredProcs_CheckedChanged(object sender, EventArgs e)
        {
            IsStoredProcs = cb_StoredProcs.Checked;

            cb_AsyncUpdate.Enabled = IsStoredProcs;

            string storedCoded;
            if (IsStoredProcs)
            {
                storedCoded = "STORED";
            }
            else
            {
                cb_AsyncUpdate.Checked = false;
                IsAsyncUpdate = false;
                storedCoded = "CODED";
            }

            if (IsStoredProcs)
            {
                // Switch stored procs helper
                StoredProcsHelper.Switch(Context, IsTestTables, IsAsyncUpdate, ConnectionString);
            }

            LogMessage(string.Format("You selected {0} procedures", storedCoded));
        }

        private void cb_AsyncUpdate_CheckedChanged(object sender, EventArgs e)
        {
            IsAsyncUpdate = cb_AsyncUpdate.Checked;

            // Switch stored procs helper
            StoredProcsHelper.Switch(Context, IsTestTables, IsAsyncUpdate, ConnectionString);

            string asyncSync = IsAsyncUpdate ? "ASYNCHRONOUS" : "SYNCHRONOUS";
            LogMessage(string.Format("You selected {0} update", asyncSync));
        }

        private void buttonCheckPushedData_Click(object sender, EventArgs e)
        {
            ValidatePushedFutureData();
            ValidatePushedDailyFuturesData();
            ValidatePushedOptionData();
            ValidatePushedOtionDataData();
            buttonCheckPushedData.Enabled = false;
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
                        labelRPS2.Text = Math.Round(rps).ToString();
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

        private void ValidatePushedFutureData()
        {
            HashSet<DateTime> futureHash = new HashSet<DateTime>(ParsedData.FutureRecords.Select(item => item.StripName));

            for (int i = 0; i < dataGridViewContract.Rows.Count - 1; i++)
            {
                string month = dataGridViewContract[3, i].Value.ToString();
                string year = dataGridViewContract[4, i].Value.ToString();
                string stripName = month + "." + year;
                DateTime itemDT = Convert.ToDateTime(stripName);
                futureHash.Remove(itemDT);
            }
            if (futureHash.Count == 0)
            {
                AsyncTaskListener.LogMessage("All futures were pushed successfully");
            }
            else
            {
                AsyncTaskListener.LogMessageFormat("Failed to push {0} futures", futureHash.Count);
                List<DateTime> residueList = futureHash.ToList();
                foreach (DateTime dt in residueList)
                {
                    AsyncTaskListener.LogMessage(" - " + dt.Month.ToString() + "." + dt.Year.ToString());
                }
            }
        }

        private void ValidatePushedOptionData()
        {
            HashSet<DateTime> optionHash = new HashSet<DateTime>(ParsedData.OptionRecords.Select(item => item.StripName));

            for (int i = 0; i < dataGridViewContract.Rows.Count - 1; i++)
            {
                string month = dataGridViewContract[3, i].Value.ToString();
                string year = dataGridViewContract[4, i].Value.ToString();
                string stripName = month + "." + year;
                DateTime itemDT = Convert.ToDateTime(stripName);
                optionHash.Remove(itemDT);
            }
            if (optionHash.Count == 0)
            {
                AsyncTaskListener.LogMessage("All options were pushed successfully");
            }
            else
            {
                AsyncTaskListener.LogMessageFormat("Failed to push {0} options", optionHash.Count);
                List<DateTime> residueList = optionHash.ToList();
                foreach (DateTime dt in residueList)
                {
                    AsyncTaskListener.LogMessage(" - " + dt.Month.ToString() + "." + dt.Year.ToString());
                }
            }
        }

        private void ValidatePushedDailyFuturesData()
        {
            var futureDailyHash = new HashSet<string>();
            string id;
            DateTime stripName;
            DateTime date;
            string itemHashSet;

            for (int i = 0; i < ParsedData.FutureRecords.Length; i++)
            {
                itemHashSet = GetNameDaylyContractHashSet(ParsedData.FutureRecords[i].StripName, ParsedData.FutureRecords[i].Date);
                futureDailyHash.Add(itemHashSet);
            }

            string itemDB;
            for (int i = 0; i < dataGridViewDailyContract.Rows.Count - 1; i++)
            {
                id = dataGridViewDailyContract[1, i].Value.ToString();
                stripName = GetStripNameContractFromGrid(id, dataGridViewContract);
                date = (DateTime)dataGridViewDailyContract[2, i].Value;
                itemDB = GetNameDaylyContractHashSet(stripName, date);
                futureDailyHash.Remove(itemDB);
            }

            if (futureDailyHash.Count == 0)
            {
                AsyncTaskListener.LogMessage("Futures was pushed success in tbldailycontractsettlements");
            }
            else
            {
                AsyncTaskListener.LogMessage(string.Format("{0} futures was failed push in tbldailycontractsettlements:", futureDailyHash.Count));
                List<string> residueList = futureDailyHash.ToList();
                foreach (string item in residueList)
                {
                    AsyncTaskListener.LogMessage(" - " + item);
                }
            }
        }

        private void ValidatePushedOtionDataData()
        {
            var optionDataHash = new HashSet<string>();
            string id;
            DateTime stripName;
            DateTime date;
            string itemHashSet;

            for (int i = 0; i < ParsedData.FutureRecords.Length; i++)
            {
                itemHashSet = GetNameOptionDataHashSet(ParsedData.FutureRecords[i].StripName, ParsedData.FutureRecords[i].Date);
                optionDataHash.Add(itemHashSet);
            }

            string itemDB;
            for (int i = 0; i < dataGridViewOptionData.Rows.Count - 1; i++)
            {
                id = dataGridViewOptionData[1, i].Value.ToString();
                stripName = GetStripNameContractFromGrid(id, dataGridViewOption);
                date = (DateTime)dataGridViewOptionData[2, i].Value;
                itemDB = GetNameOptionDataHashSet(stripName, date);
                optionDataHash.Remove(itemDB);
            }

            if (optionDataHash.Count == 0)
            {
                AsyncTaskListener.LogMessage("Options was pushed success in tbloptiondata");
            }
            else
            {
                AsyncTaskListener.LogMessage(string.Format("{0} options was failed push in tbloptiondata:", optionDataHash.Count));
                List<string> residueList = optionDataHash.ToList();
                foreach (string item in residueList)
                {
                    AsyncTaskListener.LogMessage(" - " + item);
                }
            }
        }

        private DateTime GetStripNameContractFromGrid(string id, DataGridView dgv)
        {
            var itemDT = new DateTime();
            string month;
            string year;
            string stripName;

            for (int i = 0; i < dgv.Rows.Count - 1; i++)
            {
                if(id == dgv[0, i].Value.ToString())
                {
                    month = Utilities.MonthToStringMonthCode(Convert.ToInt32(dgv[3, i].Value.ToString()));
                    year = dgv[4, i].Value.ToString();
                    stripName = month + year;
                    return itemDT = Convert.ToDateTime(stripName);
                }
            }
            return itemDT;
        }

        private string GetNameDaylyContractHashSet(DateTime stripName, DateTime date)
        {
            string stripNameMonth = stripName.Month.ToString();
            string stripNameYear = stripName.Year.ToString();
            string dateDay = date.Day.ToString();
            string dateMonth = date.Month.ToString();
            string dateYear = date.Year.ToString();
            string itemHash = stripNameMonth + "." + stripNameYear + ";" + dateDay + "."+ dateMonth + "." + dateYear;
            return itemHash;
        }

        private string GetNameOptionDataHashSet(DateTime stripName, DateTime date)
        {
            double futureYear = stripName.Year + stripName.Month * 0.0833333;
            double expirateYear = date.Year + date.Month * 0.0833333;

            string stripNameMonth = stripName.Month.ToString();
            string stripNameYear = stripName.Year.ToString();
            string dateDay = date.Day.ToString();
            string dateMonth = date.Month.ToString();
            string dateYear = date.Year.ToString();
            string timeToExpirationDate = (futureYear - expirateYear).ToString();
            string itemHash = stripNameMonth + "." + stripNameYear + ";" + dateDay + "." + dateMonth + "." + dateYear + ";" + timeToExpirationDate;
            return itemHash;
        }

    }
}
