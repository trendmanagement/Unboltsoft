using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        private async Task PushDataToDB(CancellationToken ct)
        {
            int count = 0;
            int globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //int currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length;
            if (isLocal)
            {
                progressBarLoad.Maximum += ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length;
            }
            Utilities utilites = new Utilities();
            List<string> stripName = new List<string>();
            bool newFuture = true;
            
            try
            {
                EnableDisable(true);

                await Task.Run(() =>
                {
                    string log = String.Empty;
                    foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
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

                                try
                                {
                                    List<tblcontract> tblcontracts = new List<tblcontract>();
                                    foreach(tblcontract item in context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToList())
                                    {
                                        tblcontracts.Add(item);
                                    }
                                    int countContracts = tblcontracts.Count;
                                    if(countContracts != 0)
                                    {
                                        long id = tblcontracts[0].idcontract;
                                        log += string.Format(
                                            "Message from {0} pushing TBLCONTRACTS tables \n" +
                                            "Wee already have entity with id: {1}\n",
                                            locRem, id);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int erc = globalCount;
                                    log += string.Format(
                                        "ERROR message from {0} pushing TBLCONTRACTS tables \n" +
                                        "Can't check idcontract for entity N: {1}\n",
                                        locRem, erc);
                                    log += ex.Message + "\n";
                                    continue;
                                }

                                var tableFuture = new tblcontract
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
                            log += string.Format("Cancel message from {0} pushing TBLCONTRACT table \n", locRem);
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += string.Format("ERROR message from {0} pushing TBLCONTRACT table \n", locRem);
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;
                            if (globalCount == ParsedData.FutureRecords.Length)
                            {
                                log += string.Format("Pushed {0} entities to {1} TBLCONTRACT table", count, locRem);
                            }
                            //TODO: Fix this
                            //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                            //{
                            //    currentPercent += 10;
                            //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            //}
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = String.Empty;
                        }
                    }
                }, ct);

                count = 0;
                await Task.Run(() =>
                {
                    string log = string.Empty;
                    foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());
                            tblcontract contract = context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                            try
                            {
                                List<tbldailycontractsettlement> tdcs = new List<tbldailycontractsettlement>();
                                foreach(tbldailycontractsettlement item in context.tbldailycontractsettlements.Where(item => item.idcontract == contract.idcontract && item.date == future.Date).ToList())
                                {
                                    tdcs.Add(item);
                                }
                                if (tdcs.Count != 0)
                                {
                                    long id = tdcs[0].iddailycontractsettlements;
                                    log += string.Format(
                                        "Message from {0} pushing TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                        "Wee already have entity with id: {1}\n",
                                        locRem, id);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                    "Can't check idcontract for entity N: {1}\n",
                                    locRem, erc);
                                log += ex.Message + "\n";
                                continue;
                            }

                            tbldailycontractsettlement tableDCS = new tbldailycontractsettlement
                            {
                                //idcontract must generete by DB
                                idcontract = contract.idcontract,
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
                            log += string.Format("Cancel message from {0} pushing TBLDAILYCONTRACTSETTLEMENT table \n", locRem);
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            log += string.Format("ERROR message from {0} pushing TBLDAILYCONTRACTSETTLEMENT table \n", locRem);
                            log += ex.Message + "\n";
                        }
                        finally
                        {
                            globalCount++;
                            if (globalCount == ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length)
                            {
                                log += string.Format("Pushed {0} entities to {1} TBLDAILYCONTRACTSETTLEMENT table", count, locRem);
                            }
                            //TODO: Fix this
                            //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                            //{
                            //    currentPercent += 10;
                            //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            //}
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = String.Empty;
                        }
                    }
                }, ct);

                count = 0;
                await Task.Run(() =>
                {
                    string log = string.Empty;
                    foreach (EOD_Options_578 option in ParsedData.OptionRecords)
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

                            string optionName = utilites.generateOptionCQGSymbolFromSpan(option.OptionType, "CCE", monthchar, option.StripName.Year, (option.StrikePrice != null) ? (double)option.StrikePrice : 0, 0, 0, idinstrument);

                            List<tbloption> tbloptions = new List<tbloption>();
                            try
                            {
                                foreach (tbloption item in context.tbloptions.Where(item => item.optionmonth == monthchar && item.optionyear == option.StripName.Year && item.optionname == optionName).ToList())
                                {
                                    tbloptions.Add(item);
                                }
                                int countContracts = tbloptions.Count;
                                if (countContracts != 0)
                                {
                                    long id = tbloptions[0].idoption;
                                    log += string.Format(
                                        "Message from {0} pushing pushing TBLOPTIONS table \n" +
                                        "Wee already have entity with id: {1}\n",
                                        locRem, id);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                    "Can't read N: {1} from DB\n",
                                    locRem, erc);
                                log += ex.Message + "\n";
                                continue;
                            }

                            long idContract;
                            try
                            {
                                idContract = context.tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;
                            }
                            catch(IndexOutOfRangeException outEx)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                    "We dont have contract for option entity N: {1}\n",
                                    locRem, erc);
                                log += outEx.Message + "\n";
                                continue;
                            }
                            catch(Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                    "Can't find contract for option entity N: {1}\n",
                                    locRem, erc);
                                log += ex.Message + "\n";
                                continue;
                            }

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
                                expirationdate = option.Date,
                                idcontract = idContract,
                                cqgsymbol = optionName
                            };
                            context.tbloptions.InsertOnSubmit(tableOption);
                            context.SubmitChanges();

                            double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                            double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                            List<tbloptiondata> tdcs = new List<tbloptiondata>();
                            try
                            {
                                foreach (tbloptiondata item in context.tbloptiondatas.Where(item => item.timetoexpinyears == (futureYear - expiranteYear) && item.datetime == option.Date).ToList())
                                {
                                    tdcs.Add(item);
                                }
                                if (tdcs.Count != 0)
                                {
                                    long id = tdcs[0].idoptiondata;
                                    log += string.Format(
                                        "Message from {0} pushing TBLOPTIONDATAS tables \n" +
                                        "Wee already have entity with id: {1}\n",
                                        locRem, id);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                    "Can't check idcontract for entity N: {1}\n",
                                    locRem, erc);
                                log += ex.Message + "\n";
                                continue;
                            }

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
                                //idoptiondata must generate by DB
                                idoption = idContract,
                                datetime = option.Date,
                                price = (option.StrikePrice != null) ? (double)option.StrikePrice : 1,
                                impliedvol = impliedvol,
                                timetoexpinyears = futureYear - expiranteYear
                            };

                            context.tbloptiondatas.InsertOnSubmit(tableOptionData);
                            context.SubmitChanges();
                            count++;
                        }
                        catch (OperationCanceledException cancel)
                        {
                            log += string.Format("Cancel message from {0} pushing TBLOPTIONDATAS table \n", locRem);
                            log += cancel.Message + "\n";
                        }
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                "Can't push entity N: {1}\n",
                                locRem, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
                        finally
                        {
                            globalCount++;
                            if (globalCount == ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                            {
                                log += string.Format("Pushed {0} entities to {1} TBLOPTIONS and TBLOPTIONDATAS tables", count, locRem);
                            }
                            //TODO: Fix this
                            //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                            //{
                            //    currentPercent += 10;
                            //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            //}
                            Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                            log = String.Empty;
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
                EnableDisable(false);

                SetLogMessage("Pushed to DB: " + count.ToString() + " entities");
                if (ParsedData.FutureRecords.Length > count)
                {
                    SetLogMessage("Was NOT pushed " + (ParsedData.FutureRecords.Length - count).ToString() + " entities from " + ParsedData.FutureRecords.Length.ToString() + " to DB");
                }
            }
        }
    }
}
