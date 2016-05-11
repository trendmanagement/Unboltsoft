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
        string conStr = "Server=tcp:h9ggwlagd1.database.windows.net,1433; Database=TMLDB_Copy; User ID=dataupdate@h9ggwlagd1; Password=6dcEpZKSFRNYk^AN; Encrypt=True; TrustServerCertificate=False; Connection Timeout=30;";

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

            if (StaticData.optionRecords == null || StaticData.futureRecords == null)
            {
                buttonLoad.Enabled = false;
            }
            else
            {
                richTextBoxLog.Text += StaticData.futureRecords.GetType().Name.Trim('[', ']') + " entities count: " + StaticData.futureRecords.Length.ToString()  + " ready to push to DB" + "\n";
                richTextBoxLog.Text += StaticData.optionRecords.GetType().Name.Trim('[', ']') + " entities count: " + StaticData.optionRecords.Length.ToString() + " ready to push to DB" + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private void StaticData_ParseComplete()
        {
            if (buttonLoad.Enabled != true)
            {
                richTextBoxLog.Text += "Entities count: " + StaticData.optionRecords.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.optionRecords.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private async Task SetLocalDataEOD_Options_578(CancellationToken ct)
        {
            int count = 0;
            int number;
            int persent = (int.TryParse((StaticData.optionRecords.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.optionRecords.Length;

            try
            {
                var context = new LocalEntitiesDataContext();
                //OptionsDataContext context = new OptionsDataContext();
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                foreach (EOD_Options_578 option in StaticData.optionRecords)
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
                if (StaticData.optionRecords.Length > count)
                {
                    richTextBoxLog.Text += "Was NOT loaded " + (StaticData.optionRecords.Length - count).ToString() + " entities from " + StaticData.optionRecords.Length.ToString() + " to DataBase" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private async Task SetRemoteDataEOD_Options_578(CancellationToken ct)
        {
            int count = 0;
            int number;
            int persent = (int.TryParse((StaticData.optionRecords.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.optionRecords.Length;

            try
            {
                var context = new RemoteEntitiesDataContext(conStr);
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                foreach (EOD_Options_578 option in StaticData.optionRecords)
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
                        progressBarLoad.Value = (ct.IsCancellationRequested) ? 0 : count;

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
                if (StaticData.optionRecords.Length > count)
                {
                    richTextBoxLog.Text += "Was NOT loaded " + (StaticData.optionRecords.Length - count).ToString() + " entities from " + StaticData.optionRecords.Length.ToString() + " to DataBase" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private async Task SetLocalDataEOD_Futures_578(CancellationToken ct)
        {
            int count = 0;
            int globalCount = 0;
            int number;
            int persent = (int.TryParse((StaticData.futureRecords.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.futureRecords.Length;
            Utilities utilites = new Utilities();
            List<string> stripName = new List<string>();
            bool newFuture = true;

            try
            {
                LocalEntitiesDataContext context = new LocalEntitiesDataContext();
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                EOD_Futures_578 fu = (EOD_Futures_578)StaticData.futureRecords[0];
                string futuresName = fu.ProductName.Trim(" Future".ToArray());
                await Task.Run(() =>
                {

                foreach (EOD_Futures_578 future in StaticData.futureRecords)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        foreach(string item in stripName)
                        {
                            if(item == future.StripName.ToString())
                            {
                                newFuture = false;
                                break;
                            }
                            else
                            {
                                newFuture = true;
                            }
                        }
                        if(newFuture)
                        {
                                //TODO: Create query to get idinstrument by description from tblinstruments
                                //idinstrument for description = Cocoa is 36
                                int idinstrument = 36;

                                char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());
                                string contractName = utilites.generateCQGSymbolFromSpan('F', "CCE", monthchar, future.StripName.Year);

                                contract tableFuture = new contract
                                {
                                    //idcontract must generete by DB

                                    //TODO: Create query to get cqgsymbol by description from tblinstruments
                                    //cqgsymbol for description = Cocoa is CCE
                                    contractname = contractName,
                                    month = monthchar,
                                    monthint = (short)future.StripName.Month,
                                    year = (long)future.StripName.Year,
                                    idinstrument = idinstrument,
                                    expirationdate = future.Date,
                                    cqgsymbol = contractName
                                };

                                //dailycontractsettlement tableDCS = new dailycontractsettlement
                                //{
                                //    //idcontract must generete by DB
                                //    idcontract = tableFuture.idcontract,
                                //    date = future.Date,
                                //    settlement = (double)future.SettlementPrice,
                                //    volume = (future.Volume != null) ? (long)future.Volume : 0,
                                //    openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                                //};
                                context.contracts.InsertOnSubmit(tableFuture);
                                //context.dailycontractsettlements.InsertOnSubmit(tableDCS);
                                context.SubmitChanges();
                                count++;
                                stripName.Add(future.StripName.ToString());
                        }
                    }
                    catch (OperationCanceledException cancel)
                    {
                            MessageBox.Show(cancel.Message);
                        //richTextBoxLog.Text += cancel.Message + "\n";
                        //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        //richTextBoxLog.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        //richTextBoxLog.Text += "ERROR" + "\n";
                        //richTextBoxLog.Text += ex.Message + "\n";
                        //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        //richTextBoxLog.ScrollToCaret();
                    }
                    finally
                    {
                        globalCount++;

                        //progressBarLoad.Value = (ct.IsCancellationRequested) ? 0 : globalCount;

                        //TODO: 
                        if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                        {
                            currentPersent += 10;
                            //richTextBoxLog.Text += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                            //richTextBoxLog.ScrollToCaret();
                        }
                    }
                }
                }, ct);

                await Task.Run(() =>
                {

                    foreach (EOD_Futures_578 future in StaticData.futureRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                            int idcontract = (int)context.contracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0].idcontract;

                            dailycontractsettlement tableDCS = new dailycontractsettlement
                            {
                                //idcontract must generete by DB
                                idcontract = idcontract,
                                date = future.Date,
                                settlement = (double)future.SettlementPrice,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            context.dailycontractsettlements.InsertOnSubmit(tableDCS);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            MessageBox.Show(cancel.Message);
                            //richTextBoxLog.Text += cancel.Message + "\n";
                            //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                            //richTextBoxLog.ScrollToCaret();
                        }
                        catch (Exception ex)
                        {
                            //richTextBoxLog.Text += "ERROR" + "\n";
                            //richTextBoxLog.Text += ex.Message + "\n";
                            //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                            //richTextBoxLog.ScrollToCaret();
                        }
                        finally
                        {
                            globalCount++;

                            //progressBarLoad.Value = (ct.IsCancellationRequested) ? 0 : globalCount;

                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                //richTextBoxLog.Text += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                                //richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                                //richTextBoxLog.ScrollToCaret();
                            }
                        }
                    }
                }, ct);

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
                if (StaticData.futureRecords.Length > count)
                {
                    richTextBoxLog.Text += "Was NOT loaded " + (StaticData.futureRecords.Length - count).ToString() + " entities from " + StaticData.futureRecords.Length.ToString() + " to DataBase" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private async Task SetRemoteDataEOD_Futures_578(CancellationToken ct)
        {
            int count = 0;
            int number;
            int persent = (int.TryParse((StaticData.optionRecords.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.optionRecords.Length;

            try
            {
                var context = new RemoteEntitiesDataContext(conStr);
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                foreach (EOD_Options_578 option in StaticData.optionRecords)
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
                        progressBarLoad.Value = (ct.IsCancellationRequested) ? 0 : count;

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
                if (StaticData.optionRecords.Length > count)
                {
                    richTextBoxLog.Text += "Was NOT loaded " + (StaticData.optionRecords.Length - count).ToString() + " entities from " + StaticData.optionRecords.Length.ToString() + " to DataBase" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private async void buttonLoad_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            string input = StaticData.optionRecords.GetType().Name.Trim('[', ']');
            EntityNames name = new EntityNames();
            Enum.TryParse(input, out name);
            switch (name)
            {
                case EntityNames.EOD_Futures_578:
                    // TODO: load data EOD_Futures_578 to db like EOD_Options_578 entity
                    break;
                case EntityNames.EOD_Options_578:
                    richTextBoxLog.Text += "Loading statrted" + "\n";
                    if (checkBoxCheckDB.Checked) await SetLocalDataEOD_Futures_578(cts.Token);
                    else await SetRemoteDataEOD_Futures_578(cts.Token);
                    LocalEntitiesDataContext context = new LocalEntitiesDataContext();
                    buttonFill_Click(sender, e);
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

            RemoteEntitiesDataContext remoteContext = new RemoteEntitiesDataContext(conStr);
            LocalEntitiesDataContext localContext = new LocalEntitiesDataContext();

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            if (!checkBoxCheckDB.Checked)
            {
                List<tbloption> listOption = new List<tbloption>();
                try
                {
                    int count = remoteContext.tbloptions.Where(item => item.cqgsymbol == "somesymbol").Count();
                    richTextBoxLog.Text += "Fill " + count.ToString() + " entities started from option table" + "\n";
                    richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                    richTextBoxLog.ScrollToCaret();
                    try
                    {
                        await Task.Run(() =>
                        {
                            listOption = (from item in remoteContext.tbloptions
                                                            where item.cqgsymbol == "somesymbol"
                                                            select item
                                                  ).ToList();
                        }, ctsFill.Token);
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
                    int count = listOption.Count;
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
