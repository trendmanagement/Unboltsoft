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
        CancellationTokenSource ctsFill;

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
            buttonFill.Location = new Point()
            {
                X = buttonFill.Location.X,
                Y = tabControlOption.Height + 5
            };



        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            buttonCancel.Enabled = false;

            DataBaseForm_Resize(sender, e);

            if (StaticData.records == null)
            {
                buttonLoad.Enabled = false;
            }
            else
            {
                richTextBoxLog.Text += "Entities count: " + StaticData.records.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.records.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private void StaticData_ParseComplete()
        {
            if (buttonLoad.Enabled != true)
            {
                richTextBoxLog.Text += "Entities count: " + StaticData.records.Length.ToString() + "\n";
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
                var context = new LocalEntitiesDataContext();
                //OptionsDataContext context = new OptionsDataContext();
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
                            option tableOption = new option
                            {
                                idoption = long.Parse(count.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString()),
                                optionname = option.ProductName,
                                optionmonth = option.Date.Month.ToString().ToCharArray()[0],
                                optionmonthint = option.Date.Month,
                                optionyear = option.Date.Year,
                                strikeprice = (double)option.StrikePrice,
                                callorput = 'c',
                                idinstrument = 1,
                                expirationdate = DateTime.Now,
                                idcontract = 1,
                                cqgsymbol = "somesymbol"
                            };
                            context.options.InsertOnSubmit(tableOption);
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
                    //RemoteEntitiesDataContext context = new RemoteEntitiesDataContext("constr");
                    LocalEntitiesDataContext context = new LocalEntitiesDataContext();
                    BindingSource bindingSourceBaners = new BindingSource();
                    // TODO: query for each table
                    bindingSourceBaners.DataSource = (from item in context.options
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
            if (checkBoxCheckDB.Checked)
            {
                //MessageBox.Show("Local DataBase");
            }
            else
            {
                //MessageBox.Show("Remote DataBase");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if(cts != null)
            {
                cts.Cancel();
            }
        }

        private async void buttonFill_Click(object sender, EventArgs e)
        {
            ctsFill = new CancellationTokenSource();
            string conStr = "Server=tcp:h9ggwlagd1.database.windows.net,1433; Database=TMLDB_Copy; User ID=dataupdate@h9ggwlagd1; Password=6dcEpZKSFRNYk^AN; Encrypt=True; TrustServerCertificate=False; Connection Timeout=30;";

            RemoteEntitiesDataContext remoteContext = new RemoteEntitiesDataContext(conStr);
            LocalEntitiesDataContext localContext = new LocalEntitiesDataContext();

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            if (!checkBoxCheckDB.Checked)
            {
                try
                {
                    richTextBoxLog.Text += "Fill " + remoteContext.tbloptions.Where( item => item.optionyear == 2006  && item.idinstrument == 35).Count().ToString() + " entities started from option table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();

                    List<tbloption> listOption = new List<tbloption>();

                    try
                    {
                        await Task.Run(() =>
                        {
                            listOption = (from item in remoteContext.tbloptions
                                                            where item.optionyear == 2016
                                                            where item.idinstrument == 35
                                                            select item
                                                  ).ToList();
                        }, ctsFill.Token);
                        //foreach (tbloption tb in listOption)
                        //{
                        //    richTextBoxLog.Text += tb + "\n";
                        //    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        //    richTextBoxLog.ScrollToCaret();
                        //}
                    }
                    catch (Exception ex)
                    {
                        richTextBoxLog.Text += ex.Message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    finally
                    {
                        bsOption.DataSource = listOption;
                    }
                    //richTextBoxLog.Text += "Fill " + remoteContext.tbloptiondatas.Count().ToString() + " entities started from optiondata table" + "\n";
                    //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    //richTextBoxLog.ScrollToCaret();

                    //await Task.Run(() =>
                    //{
                    //    bsOptionData.DataSource = (from item in remoteContext.tbloptiondatas
                    //                               select item
                    //                              ).ToList();
                    //}, ctsFill.Token);

                    //richTextBoxLog.Text += "Fill " + remoteContext.tblcontracts.Count().ToString() + " entities started from contract table" + "\n";
                    //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    //richTextBoxLog.ScrollToCaret();

                    //await Task.Run(() =>
                    //{
                    //    bsContract.DataSource = (from item in remoteContext.tblcontracts
                    //                             select item
                    //                            ).ToList();
                    //}, ctsFill.Token);

                    //richTextBoxLog.Text += "Fill " + remoteContext.tbldailycontractsettlements.Count().ToString() + " entities started from dailycontractsettlements table" + "\n";
                    //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    //richTextBoxLog.ScrollToCaret();

                    //await Task.Run(() =>
                    //{
                    //    bsDailyContractSettlement.DataSource = (from item in remoteContext.tbldailycontractsettlements
                    //                                            select item
                    //                                            ).ToList();
                    //}, ctsFill.Token);

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
                    int count = dataGridViewOption.RowCount;
                    richTextBoxLog.Text += "Filled: " + count.ToString() + " entities" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();
                }
            }
            else
            {
                try
                {
                    richTextBoxLog.Text += "Fill " + localContext.options.Count().ToString() + " entities started from option table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();

                    await Task.Run(() =>
                    {
                        bsOption.DataSource = (from item in localContext.options
                                               select item
                                              ).ToList();
                    }, ctsFill.Token);

                    richTextBoxLog.Text += "Fill " + localContext.optiondatas.Count().ToString() + " entities started from optiondata table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();

                    await Task.Run(() =>
                    {
                        bsOptionData.DataSource = (from item in localContext.optiondatas
                                                   select item
                                                  ).ToList();
                    }, ctsFill.Token);

                    richTextBoxLog.Text += "Fill " + localContext.contracts.Count().ToString() + " entities started from contract table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();

                    await Task.Run(() =>
                    {
                        bsContract.DataSource = (from item in localContext.contracts
                                                 select item
                                                ).ToList();
                    }, ctsFill.Token);

                    richTextBoxLog.Text += "Fill " + localContext.dailycontractsettlements.Count().ToString() + " entities started from dailycontractsettlements table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();

                    await Task.Run(() =>
                    {
                        bsDailyContractSettlement.DataSource = (from item in localContext.dailycontractsettlements
                                                                select item
                                                                ).ToList();
                    }, ctsFill.Token);

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
                    int count = localContext.dailycontractsettlements.Count() + localContext.options.Count() + localContext.optiondatas.Count() + localContext.contracts.Count();
                    richTextBoxLog.Text += "Filled: " + count.ToString() + " entities" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();
                }

            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }
    }
}
