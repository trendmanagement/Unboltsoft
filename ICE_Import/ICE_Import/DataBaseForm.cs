﻿using System;
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
        public delegate void SetLogMessageDelegate(string message);
        public event SetLogMessageDelegate SetLogMessage;

        CancellationTokenSource cts;
        CancellationTokenSource ctsFill;
        string conStr = "Server=tcp:h9ggwlagd1.database.windows.net,1433; Database=TMLDB_Copy; User ID=dataupdate@h9ggwlagd1; Password=6dcEpZKSFRNYk^AN; Encrypt=True; TrustServerCertificate=False; Connection Timeout=30;";

        public DataBaseForm()
        {
            InitializeComponent();
            this.Resize += DataBaseForm_Resize;
            StaticData.ParseComplete += StaticData_ParseComplete;
            this.SetLogMessage += DataBaseForm_SetLogMessage;
        }

        private void DataBaseForm_SetLogMessage(string message)
        {
            richTextBoxLog.Text += message + "\n";
            richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
            richTextBoxLog.ScrollToCaret();
        }

        private void ValuesFromTask(string message, int count)
        {
            if(message != "")
            {
                richTextBoxLog.Text += message + "\n";
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
            progressBarLoad.Value = count;
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
            buttonPush.Location = new Point()
            {
                X = buttonPush.Location.X,
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
            buttonPull.Location = new Point()
            {
                X = buttonPull.Location.X,
                Y = tabControlOption.Height + 5
            };
        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            buttonCancel.Enabled = false;

            DataBaseForm_Resize(sender, e);

            if (StaticData.optionRecords == null || StaticData.futureRecords == null)
            {
                buttonPush.Enabled = false;
            }
            else
            {
                SetLogMessage(StaticData.futureRecords.GetType().Name.Trim('[', ']') + " entities count: " + StaticData.futureRecords.Length.ToString()  + " ready to push to DB");
                SetLogMessage(StaticData.optionRecords.GetType().Name.Trim('[', ']') + " entities count: " + StaticData.optionRecords.Length.ToString() + " ready to push to DB");
                buttonPush.Enabled = true;
            }
        }

        private void StaticData_ParseComplete()
        {
            if (buttonPush.Enabled != true)
            {
                SetLogMessage("Entities count: " + StaticData.optionRecords.Length.ToString());
                SetLogMessage("Type of entity: " + StaticData.optionRecords.GetType().Name.Trim('[', ']'));
                buttonPush.Enabled = true;
            }
        }

        private async Task PushDataToLocalDB(CancellationToken ct)
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
                buttonPush.Enabled = false;
                buttonCancel.Enabled = true;

                EOD_Futures_578 fu = (EOD_Futures_578)StaticData.futureRecords[0];
                string futuresName = fu.ProductName.Trim(" Future".ToArray());

                await Task.Run(() =>
                {
                    string log = "";
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
                                    context.contracts.InsertOnSubmit(tableFuture);
                                    context.SubmitChanges();
                                    count++;
                                    stripName.Add(future.StripName.ToString());
                            }
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;

                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }
                }, ct);

                await Task.Run(() =>
                {
                    string log = "";
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
                                settlement = (future.SettlementPrice != null)? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            context.dailycontractsettlements.InsertOnSubmit(tableDCS);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;
                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }
                }, ct);

                await Task.Run(() =>
                {
                    string log = "";
                    foreach (EOD_Options_578 option in StaticData.optionRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            //TODO: Create query to get idinstrument by description from tblinstruments
                            //idinstrument for description = Cocoa is 36
                            int idinstrument = 36;

                            char monthchar = Convert.ToChar(((MonthCodes)option.StripName.Month).ToString());

                            int idcontract = (int)context.contracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToArray()[0].idcontract;
                            //TODO: Create query to get idinstrument by description from tblinstruments
                            //idinstrument for description = Cocoa is 36
                            string optionName = utilites.generateOptionCQGSymbolFromSpan(option.OptionType, "CCE", monthchar, option.StripName.Year, (option.StrikePrice != null)? (double)option.StrikePrice : 0, 0, 0, idinstrument);

                            option tableOption = new option
                            {
                                //idoption must generete by DB
                                optionname = optionName,
                                optionmonth = monthchar,
                                optionmonthint = option.StripName.Month,
                                optionyear = option.StripName.Year,
                                strikeprice = (option.StrikePrice != null)? (double)option.StrikePrice : 0,
                                callorput = option.OptionType,
                                idinstrument = idinstrument,
                                expirationdate = DateTime.Now,
                                idcontract = idcontract,
                                cqgsymbol = optionName
                            };

                            // callPutFlag                      - tableOption.callorput
                            // S - stock price                  - 1.56
                            // X - strike price of option       - option.SettlementPrice
                            // T - time to expiration in years  - 0.5
                            // r - risk-free interest rate      - option.OpenInterest
                            // currentOptionPrice               - tableOption.strikeprice

                            double impliedvol = OptionCalcs.calculateOptionVolatility(tableOption.callorput, 
                                                                                      1.56, 
                                                                                      (option.SettlementPrice!= null)? (double)option.SettlementPrice : 0, 
                                                                                      0.5, 
                                                                                      (option.OpenInterest != null)? (double)option.OpenInterest : 0, 
                                                                                      tableOption.strikeprice);

                            optiondata tableOptionData = new optiondata
                            {
                                //idoptiondata must generete by DB
                                idoption = tableOption.idoption,
                                datetime = option.Date,
                                price = 1,
                                impliedvol = impliedvol,
                                timetoexpinyear = (option.StrikePrice != null)? (double)option.StrikePrice : 0
                            };

                            context.optiondatas.InsertOnSubmit(tableOptionData);
                            context.options.InsertOnSubmit(tableOption);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;
                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }
                }, ct);

            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                buttonPush.Enabled = true;
                buttonCancel.Enabled = false;

                Invoke(new Action(() => DataBaseForm_SetLogMessage("Was loaded to DataBase " + count.ToString() + " entities")));
                SetLogMessage("Was loaded to DataBase " + count.ToString() + " entities");
                if (StaticData.futureRecords.Length > count)
                {
                    SetLogMessage("Was NOT loaded " + (StaticData.futureRecords.Length - count).ToString() + " entities from " + StaticData.futureRecords.Length.ToString() + " to DB");
                }
            }
        }

        private async Task PushDataToRemoteDB(CancellationToken ct)
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
                var context = new RemoteEntitiesDataContext(conStr);
                buttonPush.Enabled = false;
                buttonCancel.Enabled = true;

                await Task.Run(() =>
                {
                    string log = "";
                    foreach (EOD_Futures_578 future in StaticData.futureRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            foreach (string item in stripName)
                            {
                                if (item == future.StripName.ToString())
                                {
                                    newFuture = false;
                                    break;
                                }
                                else
                                {
                                    newFuture = true;
                                }
                            }
                            if (newFuture)
                            {
                                //TODO: Create query to get idinstrument by description from tblinstruments
                                //idinstrument for description = Cocoa is 36
                                int idinstrument = 36;

                                char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());
                                string contractName = utilites.generateCQGSymbolFromSpan('F', "CCE", monthchar, future.StripName.Year);

                                tblcontract tableFuture = new tblcontract
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
                                context.tblcontracts.InsertOnSubmit(tableFuture);
                                context.SubmitChanges();
                                count++;
                                stripName.Add(future.StripName.ToString());
                            }
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;

                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }

                }, ct);

                await Task.Run(() =>
                {
                    string log = "";
                    foreach (EOD_Futures_578 future in StaticData.futureRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                            int idcontract = (int)context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0].idcontract;

                            tbldailycontractsettlement tableDCS = new tbldailycontractsettlement
                            {
                                //idcontract must generete by DB
                                idcontract = idcontract,
                                date = future.Date,
                                settlement = (future.SettlementPrice != null) ? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            context.tbldailycontractsettlements.InsertOnSubmit(tableDCS);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;

                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }
                }, ct);

                await Task.Run(() =>
                {
                    string log = "";
                    foreach (EOD_Options_578 option in StaticData.optionRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            //TODO: Create query to get idinstrument by description from tblinstruments
                            //idinstrument for description = Cocoa is 36
                            int idinstrument = 36;

                            char monthchar = Convert.ToChar(((MonthCodes)option.StripName.Month).ToString());

                            int idcontract = (int)context.tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToArray()[0].idcontract;
                            //TODO: Create query to get idinstrument by description from tblinstruments
                            //idinstrument for description = Cocoa is 36
                            string optionName = utilites.generateOptionCQGSymbolFromSpan(option.OptionType, "CCE", monthchar, option.StripName.Year, (option.StrikePrice != null) ? (double)option.StrikePrice : 0, 0, 0, idinstrument);

                            tbloption tableOption = new tbloption
                            {
                                //idoption must generete by DB
                                optionname = optionName,
                                optionmonth = monthchar,
                                optionmonthint = option.StripName.Month,
                                optionyear = option.StripName.Year,
                                strikeprice = (option.StrikePrice != null) ? (double)option.StrikePrice : 0,
                                callorput = option.OptionType,
                                idinstrument = idinstrument,
                                expirationdate = DateTime.Now,
                                idcontract = idcontract,
                                cqgsymbol = optionName
                            };

                            // callPutFlag                      - tableOption.callorput
                            // S - stock price                  - 1.56
                            // X - strike price of option       - option.SettlementPrice
                            // T - time to expiration in years  - 0.5
                            // r - risk-free interest rate      - option.OpenInterest
                            // currentOptionPrice               - tableOption.strikeprice

                            double impliedvol = OptionCalcs.calculateOptionVolatility(tableOption.callorput,
                                                                                    1.56,
                                                                                    (option.SettlementPrice != null) ? (double)option.SettlementPrice : 0,
                                                                                    0.5,
                                                                                    (option.OpenInterest != null) ? (double)option.OpenInterest : 0,
                                                                                    tableOption.strikeprice);

                            tbloptiondata tableOptionData = new tbloptiondata
                            {
                                //idoptiondata must generete by DB
                                idoption = tableOption.idoption,
                                datetime = option.Date,
                                price = 1,
                                impliedvol = impliedvol,
                                timetoexpinyear = (option.StrikePrice != null) ? (double)option.StrikePrice : 0
                            };

                            context.tbloptiondatas.InsertOnSubmit(tableOptionData);
                            context.tbloptions.InsertOnSubmit(tableOption);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += "ERROR" + "\n";
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;

                            //TODO: 
                            if (count % (10 * persent) > 0 && count % (10 * persent) < 0.5)
                            {
                                currentPersent += 10;
                                log += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            }
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = "";
                        }
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                buttonPush.Enabled = true;
                buttonCancel.Enabled = false;
                SetLogMessage("Was loaded to DataBase " + count.ToString() + " entities");
                if (StaticData.optionRecords.Length > count)
                {
                    SetLogMessage("Was NOT loaded " + (StaticData.optionRecords.Length - count).ToString() + " entities from " + StaticData.optionRecords.Length.ToString() + " to DataBase");
                }
            }
        }

        private async void buttonPush_Click(object sender, EventArgs e)
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
                    richTextBoxLog.Text += "Pushing statrted" + "\n";
                    if (checkBoxCheckDB.Checked) await PushDataToLocalDB(cts.Token);
                    else await PushDataToRemoteDB(cts.Token);
                    buttonPull_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void checkBoxCheckDB_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCheckDB.Checked)
            {
               SetLogMessage("You selected local DB");
            }
            else
            {
                SetLogMessage("You selected remote DB");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if(cts != null)
            {
                cts.Cancel();
            }
        }

        private async void buttonPull_Click(object sender, EventArgs e)
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
                    int count = 100;

                    SetLogMessage("Pulling " + count + " entities started from contract table");

                    await Task.Run(() =>
                    {
                        bsContract.DataSource = (from item in remoteContext.tblcontracts
                                                 select item
                                                ).Take(count).ToList();
                    }, ctsFill.Token);

                    SetLogMessage("Pulling " + count + " entities started from dailycontractsettlements table");

                    await Task.Run(() =>
                    {
                        bsDailyContractSettlement.DataSource = (from item in remoteContext.tbldailycontractsettlements
                                                                select item
                                                                ).Take(count).ToList();
                    }, ctsFill.Token);

                    //int count = remoteContext.tbloptions.Where(item => item.cqgsymbol == "somesymbol").Count();
                    SetLogMessage("Pulling " + count.ToString() + " entities started from option table");
                    try
                    {
                        await Task.Run(() =>
                        {
                            listOption = (from item in remoteContext.tbloptions
                                                            //where item.cqgsymbol == "somesymbol"
                                                            select item
                                                  ).Take(count).ToList();
                        }, ctsFill.Token);
                    }
                    catch (Exception ex)
                    {
                        SetLogMessage(ex.Message);
                    }
                    finally
                    {
                        bsOption.DataSource = listOption;
                    }

                    SetLogMessage("Pulling " + count + " entities started from optiondata table");
                    await Task.Run(() =>
                    {
                        bsOptionData.DataSource = (from item in remoteContext.tbloptiondatas
                                                   select item
                                                  ).Take(count).ToList();
                    }, ctsFill.Token);

                }
                catch (OperationCanceledException cancel)
                {
                    SetLogMessage(cancel.Message);
                }
                catch (Exception ex)
                {
                    SetLogMessage("ERROR");
                    SetLogMessage(ex.Message);
                }
                finally
                {
                    int count = 4 * 100;
                    SetLogMessage("Pulled: " + count.ToString() + " entities from remote DB");
                }
            }
            else
            {
                try
                {
                    SetLogMessage("Pulling " + localContext.options.Count().ToString() + " entities started from option table");
                    await Task.Run(() =>
                    {
                        bsOption.DataSource = (from item in localContext.options
                                               select item
                                              ).ToList();
                    }, ctsFill.Token);

                    SetLogMessage("Pulling " + localContext.optiondatas.Count().ToString() + " entities started from optiondata table");
                    await Task.Run(() =>
                    {
                        bsOptionData.DataSource = (from item in localContext.optiondatas
                                                   select item
                                                  ).ToList();
                    }, ctsFill.Token);

                    SetLogMessage("Pulling " + localContext.contracts.Count().ToString() + " entities started from contract table");
                    await Task.Run(() =>
                    {
                        bsContract.DataSource = (from item in localContext.contracts
                                                 select item
                                                ).ToList();
                    }, ctsFill.Token);

                    SetLogMessage("Pulling " + localContext.dailycontractsettlements.Count().ToString() + " entities started from dailycontractsettlements table");
                    await Task.Run(() =>
                    {
                        bsDailyContractSettlement.DataSource = (from item in localContext.dailycontractsettlements
                                                                select item
                                                                ).ToList();
                    }, ctsFill.Token);

                }
                catch (OperationCanceledException cancel)
                {
                    SetLogMessage(cancel.Message);
                }
                catch (Exception ex)
                {
                    SetLogMessage("ERROR");
                    SetLogMessage(ex.Message);
                }
                finally
                {
                    int count = localContext.dailycontractsettlements.Count() + localContext.options.Count() + localContext.optiondatas.Count() + localContext.contracts.Count();
                    SetLogMessage("Pulled: " + count.ToString() + " entities from lockal DB");
                }

            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

    }
}
