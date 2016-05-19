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
        public delegate void SetLogMessageDelegate(string message);
        public event SetLogMessageDelegate SetLogMessage;

        CancellationTokenSource cts;

        string locConStr = @"Data Source=(localdb)\MSSQLLocalDB;
                             Integrated Security=True;
                             AttachDbFileName=" + Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Database1.mdf") + ";";
        string remConStr = @"Server=tcp:h9ggwlagd1.database.windows.net,1433;
                             Database=TMLDB_Copy;
                             User ID=dataupdate@h9ggwlagd1;
                             Password=6dcEpZKSFRNYk^AN;
                             Encrypt=True;
                             TrustServerCertificate=False;
                             Connection Timeout=30;";
        string remConStrTest = @"Server=tcp:h9ggwlagd1.database.windows.net,1433;
                             Database=TMLDB;
                             User ID=dataupdate@h9ggwlagd1;
                             Password=6dcEpZKSFRNYk^AN;
                             Encrypt=True;
                             TrustServerCertificate=False;
                             Connection Timeout=30;";

        bool isLocal;
        string locRem;

        OFDataContext Context;
        TestOFDataContext TestContext;

        public FormDB()
        {
            InitializeComponent();
            this.Resize += FormDB_Resize;
            ParsedData.ParseComplete += ParsedData_ParseComplete;
            this.SetLogMessage += FormDB_SetLogMessage;
            this.FormClosed += FormDB_FormClosed;

            checkBoxLocalDB_CheckedChanged(null, null);
            checkBoxUseSP_CheckedChanged(null, null);
        }

        private void FormDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.csvf.Close();
        }

        private void FormDB_SetLogMessage(string message)
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
            tabControlOption.Size = new Size()
            {
                Width = this.Width - 15,
                Height = this.Height - 175
            };

            buttonPush.Location = new Point()
            {
                X = buttonPush.Location.X,
                Y = tabControlOption.Height + 5
            };
            checkBoxLocalDB.Location = new Point()
            {
                X = checkBoxLocalDB.Location.X,
                Y = tabControlOption.Height + 7
            };
            checkBoxUseSP.Location = new Point()
            {
                X = checkBoxUseSP.Location.X,
                Y = tabControlOption.Height + 7
            };
            progressBarLoad.Location = new Point()
            {
                X = progressBarLoad.Location.X,
                Y = tabControlOption.Height + 7 + 25
            };
            progressBarLoad.Width = tabControlOption.Width - 15;
            buttonCancel.Location = new Point()
            {
                X = buttonCancel.Location.X,
                Y = tabControlOption.Height + 5
            };
            buttonPull.Location = new Point()
            {
                X = buttonPull.Location.X,
                Y = tabControlOption.Height + 5
            };
            buttonToCSV.Location = new Point()
            {
                X = this.Width - 25 - buttonToCSV.Width,
                Y = tabControlOption.Height + 5
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
                SetLogMessage(ParsedData.FutureRecords.GetType().Name.Trim('[', ']') + " entities count: " + ParsedData.FutureRecords.Length.ToString()  + " ready to push to DB");
                if (!ParsedData.FuturesOnly)
                {
                    SetLogMessage(ParsedData.OptionRecords.GetType().Name.Trim('[', ']') + " entities count: " + ParsedData.OptionRecords.Length.ToString() + " ready to push to DB");
                }

                buttonPush.Enabled = true;
            }
        }

        private void ParsedData_ParseComplete()
        {
            if (buttonPush.Enabled != true)
            {
                SetLogMessage("Entities count: " + ParsedData.OptionRecords.Length.ToString());
                SetLogMessage("Type of entity: " + ParsedData.OptionRecords.GetType().Name.Trim('[', ']'));
                buttonPush.Enabled = true;
            }
        }
        
        private async void buttonPush_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();

            richTextBoxLog.Text += "Pushing started\n";
            if (!isLocal)
            {
                await PushDataToDBTest(cts.Token);
            }
            else
            {
                if (checkBoxUseSP.Checked)
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

        private void checkBoxLocalDB_CheckedChanged(object sender, EventArgs e)
        {
            isLocal = checkBoxLocalDB.Checked;
            if (isLocal)
            {
                locRem = "LOCAL";
                Context = new OFDataContext(locConStr);
            }
            else
            {
                locRem = "REMOTE";
                TestContext = new TestOFDataContext(remConStrTest);
            }
            SetLogMessage(string.Format("You selected {0} DB", locRem));
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            progressBarLoad.Value = 0;
        }

        private void buttonPull_Click(object sender, EventArgs e)
        {
            if (isLocal)
            {
                PullDataFromDB();
            }
            else
            {
                PullDataFromDBTest();
            }
        }
        
        private void buttonToCSV_Click(object sender, EventArgs e)
        {
            Hide();
            Program.csvf.Show();
        }

        private void EnableDisable(bool start)
        {
            if (start)
            {
                buttonPush.Enabled = false;
                buttonPull.Enabled = false;
                buttonCancel.Enabled = true;
                checkBoxLocalDB.Enabled = false;
                buttonToCSV.Enabled = false;
                checkBoxUseSP.Enabled = false;
            }
            else
            {
                if (ParsedData.IsReady)
                {
                    buttonPush.Enabled = true;
                }
                buttonPull.Enabled = true;
                buttonCancel.Enabled = false;
                checkBoxLocalDB.Enabled = true;
                buttonToCSV.Enabled = true;
                checkBoxUseSP.Enabled = true;
            }
        }

        private void checkBoxUseSP_CheckedChanged(object sender, EventArgs e)
        {
            string storedCoded = checkBoxUseSP.Checked ? "STORED" : "CODED";
            SetLogMessage(string.Format("You selected {0} PROCEDURES", storedCoded));
        }
    }
}
