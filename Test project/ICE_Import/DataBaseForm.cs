using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class DataBaseForm : Form
    {
        CancellationTokenSource cts;

        public DataBaseForm()
        {
            InitializeComponent();
            this.Resize += DataBaseForm_Resize;
            StaticData.ParseComplete += StaticData_ParseComplete;
        }

        private void DataBaseForm_Resize(object sender, EventArgs e)
        {
            tabControlOption.Size = new Size()
            {
                Width = this.Width - 15,
                Height = this.Height - 175
            };
            richTextBoxLog.Location = new Point()
            {
                X = 7,
                Y = tabControlOption.Height + 7 + 15 + 25
            };
            richTextBoxLog.Width = tabControlOption.Width - 15;
            richTextBoxLog.Height = 90;
            buttonLoad.Location = new Point()
            {
                X = buttonLoad.Location.X,
                Y = tabControlOption.Height + 5
            };
            checkBoxCheckDB.Location = new Point()
            {
                X = checkBoxCheckDB.Location.X,
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

        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            buttonCancel.Enabled = false;

            DataBaseForm_Resize(sender, e);

            // TODO: This line of code loads data into the 'testDatabaseDataSet3.tbldailycontractsettlements' table. You can move, or remove it, as needed.
            this.tbldailycontractsettlementsTableAdapter.Fill(this.testDatabaseDataSet3.tbldailycontractsettlements);
            // TODO: This line of code loads data into the 'testDatabaseDataSet2.tblcontracts' table. You can move, or remove it, as needed.
            this.tblcontractsTableAdapter.Fill(this.testDatabaseDataSet2.tblcontracts);
            // TODO: This line of code loads data into the 'testDatabaseDataSet1.tbloptiondata' table. You can move, or remove it, as needed.
            this.tbloptiondataTableAdapter.Fill(this.testDatabaseDataSet1.tbloptiondata);

            OptionsDataContext context = new OptionsDataContext();
            BindingSource bindingSourceBaners = new BindingSource();
            bindingSourceBaners.DataSource = (from item in context.tbloptions
                                              select item
                                              ).ToList();
            dataGridViewOption.DataSource = bindingSourceBaners;
            this.tbloptionsTableAdapter.Fill(this.testDatabaseDataSet.tbloptions);

            if (StaticData.records == null)
            {
                buttonLoad.Enabled = false;
            }
            else
            {
                richTextBoxLog.Text += "Count entities: " + StaticData.records.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.records.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private void StaticData_ParseComplete()
        {
            if (buttonLoad.Enabled != true)
            {
                richTextBoxLog.Text += "Count entities: " + StaticData.records.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.records.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private async Task GetDataEOD_Options_578(CancellationToken ct)
        {
            int count = 0;
            int number;
            int persent = (int.TryParse((StaticData.records.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.records.Length;

            try
            {
                OptionsDataContext context = new OptionsDataContext();
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                foreach (EOD_Options_578 option in StaticData.records)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        await Task.Run(() =>
                        {
                            tbloption tableOption = new tbloption
                            {
                                idoption = long.Parse(count.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString()),
                                optionname = option.ProductName,
                                optionmanth = option.Date.Month.ToString().ToCharArray()[0],
                                optionmanthint = option.Date.Month,
                                optionyear = option.Date.Year,
                                strikeprice = (double)option.StrikePrice,
                                callorput = 'c',
                                idinstrument = 1,
                                expirationdate = DateTime.Now,
                                idcontract = 1,
                                cqgsymbol = "somesymbol"
                            };
                            context.tbloptions.InsertOnSubmit(tableOption);
                            context.SubmitChanges();
                            count++;
                        }, ct);
                        
                    }
                    catch (OperationCanceledException cancel)
                    {
                        richTextBoxLog.Text += cancel.Message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxLog.Text += "ERROR" + "\n";
                        richTextBoxLog.Text += ex.Message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    finally
                    {
                        progressBarLoad.Value = (ct.IsCancellationRequested)? 0 : count;

                        if (count % (10 * persent) == 0)
                        {
                            currentPersent += 10;
                            richTextBoxLog.Text += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                            richTextBoxLog.ScrollToCaret();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBoxLog.Text += "ERROR" + "\n";
                richTextBoxLog.Text += ex.Message + "\n";
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
            finally
            {
                buttonLoad.Enabled = true;
                buttonCancel.Enabled = false;
                richTextBoxLog.Text += "Was loaded to DataBase " + count.ToString() + " entities" + "\n";
                if (StaticData.records.Length > count)
                {
                    richTextBoxLog.Text += "Was NOT loaded " + (StaticData.records.Length - count).ToString() + " entities from " + StaticData.records.Length.ToString() + " to DataBase" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private async void buttonLoad_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            string input = StaticData.records.GetType().Name.Trim('[', ']');
            EntityNames name = new EntityNames();
            Enum.TryParse(input, out name);
            switch (name)
            {
                case EntityNames.EOD_Futures_578:
                    // TODO: load data EOD_Futures_578 to db like EOD_Options_578 entity
                    break;
                case EntityNames.EOD_Options_578:
                    richTextBoxLog.Text += "Loading statrted" + "\n";
                    await GetDataEOD_Options_578(cts.Token);
                    OptionsDataContext context = new OptionsDataContext();
                    BindingSource bindingSourceBaners = new BindingSource();
                    // TODO: query for each table
                    bindingSourceBaners.DataSource = (from item in context.tbloptions
                                                      select item
                                                      ).ToList();
                    dataGridViewOption.DataSource = bindingSourceBaners;
                    break;
                default:
                    break;
            }
        }

        private void checkBoxCheckDB_CheckedChanged(object sender, EventArgs e)
        {
            //
            //TODO: Check context data type from lockal or remote db
            //Now wee just have lockal OptionsDataContext entity
            //

            if (checkBoxCheckDB.Checked == true)
            {
                MessageBox.Show("Wee don't have remote DataBase yet");
            }
            checkBoxCheckDB.Checked = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if(cts != null)
            {
                cts.Cancel();
            }
        }
    }
}
