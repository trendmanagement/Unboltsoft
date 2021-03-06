﻿using System;
using System.Collections.Generic;
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

        string TablesPrefix;
        bool IsTestTables;

        string StoredProcPrefix;

        ConnectionStrings ConnectionStrings = new ConnectionStrings();

        string ConnectionString;
        DataClassesTMLDBDataContext Context;

        TMLDBReader TMLDBReader;

        #region Input parameters
        long? IdInstrument;
        string CqgSymbol;
        double? TickSize;
        List<tbloptioninputdata> RiskFreeInterestRates = new List<tbloptioninputdata>();
        #endregion

        HashSet<DateTime> StripNameHashSet;
        HashSet<string> OptionNameHashSet;
        HashSet<Tuple<DateTime, DateTime>> StripNameDateHashSet;
        List<Tuple<string, DateTime, double>> OptionDataList;

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
            buttonDrop.Location = new Point()
            {
                X = buttonDrop.Location.X,
                Y = this.Height - 218
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

                // Choose StrikePriceToCQGSymbolFactor value
                ParsedData.StrikePriceToCQGSymbolFactor = GetStrikePriceToCQGSymbolFactor(
                    ParsedData.OptionRecords.Select(item => item.StrikePrice));
                AsyncTaskListener.LogMessageFormat("Chosen StrikePriceToCQGSymbolFactor value: {0}", ParsedData.StrikePriceToCQGSymbolFactor);
            }
            buttonPush.Enabled = true;
        }

        private void ParsedData_ParseFailed()
        {
            buttonPush.Enabled = false;
        }

        private async void buttonPush_Click(object sender, EventArgs e)
        {
            useOldRFI = false;
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

            bool areThreeParamsFound = false;
            if (IdInstrument == null || CqgSymbol == null || TickSize == null)
            {
                areThreeParamsFound = await Task.Run(
                    () => 
                    TMLDBReader.GetThreeParams(
                        ParsedData.JsonConfig.TMLDB_Description,
                        ref IdInstrument,
                        ref CqgSymbol,
                        ref TickSize));
            }
            else
            {
                areThreeParamsFound = true;
            }


            bool isRiskFound = false;
            if (RiskFreeInterestRates.Count == 0)
            {
                isRiskFound = await Task.Run(() => 
                    TMLDBReader.GetRisk(ref RiskFreeInterestRates));
            }
            else
            {
                isRiskFound = true;
            }

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
                // Install stored procedures from SQL files into DB
                bool success = await Task.Run(() =>
                    StoredProcsInstallator.Install(ConnectionString, IsTestTables, cts.Token));
                if (!success)
                {
                    EnableDisable(false);
                    return;
                }

                await PushDataToDB(cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
                // The form was closed during the process
            }

            // Update the data grid
            //buttonPull_Click(sender, e);
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
            buttonDrop.Enabled = !start;
            checkBox1000.Enabled = !start;
            buttonCheckPushedData.Enabled = start ? false : (dataGridViewContract.DataSource != null);
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
                    ConnectionString = ConnectionStrings.Local;
                    break;
                case 2:
                    // TMLDB_Copy
                    DatabaseName = "TMLDB_Copy";
                    ConnectionString = ConnectionStrings.TMLDB_Copy;
                    break;
                case 3:
                    // TMLDB
                    DatabaseName = "TMLDB";
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

            Context.CommandTimeout = 0;

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

            LogMessage(string.Format("You selected {0} tables", testNonTest));
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
            if (DatabaseName == "TMLDB_Copy" && IsTestTables)
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

        private void buttonDrop_Click(object sender, EventArgs e)
        {
            DropTempTables();
        }

        /// <summary>
        /// Choose the factor for strike price used in CQG symbol (e.g. 1, 10, 100 etc.)
        /// </summary>
        private int GetStrikePriceToCQGSymbolFactor(IEnumerable<decimal?> strikePrices)
        {
            int maxPow = int.MinValue;

            foreach (decimal? strikePrice in strikePrices)
            {
                int[] bits = decimal.GetBits(strikePrice.GetValueOrDefault());
                byte[] bytes = BitConverter.GetBytes(bits[3]);
                int pow = bytes[2];
                if (pow > maxPow)
                {
                    maxPow = pow;
                }
            }

            return (int)Math.Pow(10, maxPow);
        }
    }
}
