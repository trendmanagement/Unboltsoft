﻿// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING
// WARNING                                                                         WARNING
// WARNING    DO NOT EDIT THIS .CS FILE, BECAUSE ALL YOUR CHANGES WILL BE LOST!    WARNING
// WARNING    EDIT CORRESPONDING .TT FILE INSTEAD!                                 WARNING
// WARNING                                                                         WARNING
// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING

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
            var context_ = Context;
            var tblcontracts_ = context_.tblcontracts;
            var tbldailycontractsettlements_ = context_.tbldailycontractsettlements;
            var tbloptions_ = context_.tbloptions;
            var tbloptiondatas_ = context_.tbloptiondatas;

            int count = 0;
            int globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //int currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length + ((!ParsedData.FuturesOnly) ? ParsedData.OptionRecords.Length : 0);
            Utilities utilites = new Utilities();
            List<string> stripName = new List<string>();
            bool newFuture = true;

            DateTime start = DateTime.Now;

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

                                #region Find data in DB like pushed
                                try
                                {
                                    var tblcontracts = new List<tblcontract>();
                                    foreach (var item in tblcontracts_.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToList())
                                    {
                                        tblcontracts.Add(item);
                                    }
                                    int countContracts = tblcontracts.Count;
                                    if (countContracts != 0)
                                    {
                                        long id = tblcontracts[0].idcontract;
                                        log += string.Format(
                                            "Message from {0} pushing TBLCONTRACTS tables \n" +
                                            "We already have entity with id: {1}\n",
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
                                #endregion

                                var tableFuture = new tblcontract                                {
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
                                tblcontracts_.InsertOnSubmit(tableFuture);
                                context_.SubmitChanges();
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
                SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

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
                            var contract = tblcontracts_.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                            #region Find data in DB like pushed
                            try
                            {
                                var tdcs = new List<tbldailycontractsettlement>();
                                foreach (var item in tbldailycontractsettlements_.Where(item => item.idcontract == contract.idcontract && item.date == future.Date).ToList())
                                {
                                    tdcs.Add(item);
                                }
                                if (tdcs.Count != 0)
                                {
                                    long id = tdcs[0].iddailycontractsettlements;
                                    log += string.Format(
                                        "Message from {0} pushing TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                        "We already have entity with id: {1}\n",
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
                            #endregion

                            var tableDCS = new tbldailycontractsettlement                            {
                                //idcontract must generete by DB
                                idcontract = contract.idcontract,
                                date = future.Date,
                                settlement = (future.SettlementPrice != null) ? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                            context_.SubmitChanges();
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
                SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

                if (!ParsedData.FuturesOnly)
                {
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

                                bool isOptionCreated = false;

                                var TO = new tbloption();

                                #region Find data in DB like pushed
                                var tbloptions = new List<tbloption>();
                                try
                                {
                                    var optlist = tbloptions_.Where(item => item.optionmonth == monthchar && item.optionyear == option.StripName.Year && item.optionname == optionName).ToList();
                                    foreach (var item in optlist)
                                    {
                                        tbloptions.Add(item);
                                    }
                                    int countContracts = tbloptions.Count;
                                    if (countContracts > 0)
                                    {
                                        isOptionCreated = true;
                                        TO = tbloptions[0];
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
                                #endregion

                                if (!isOptionCreated)
                                {
                                    #region Find id contract for pushing option
                                    long idContract;
                                    try
                                    {
                                        idContract = tblcontracts_.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;
                                    }
                                    catch (IndexOutOfRangeException outEx)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                            "We dont have contract for option entity N: {1}\n",
                                            locRem, erc);
                                        log += outEx.Message + "\n";
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                            "Can't find contract for option entity N: {1}\n",
                                            locRem, erc);
                                        log += ex.Message + "\n";
                                        continue;
                                    }
                                    #endregion

                                    var tableOption = new tbloption                                    {
                                        //idoption must generate by DB
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
                                    tbloptions_.InsertOnSubmit(tableOption);
                                    context_.SubmitChanges();

                                    #region Find id for fist time pushed option
                                    var tblopt = new List<tbloption>();
                                    try
                                    {
                                        foreach (var item in tbloptions_.Where(item => item == tableOption).ToList())
                                        {
                                            tblopt.Add(item);
                                        }
                                        if (tblopt.Count == 0)
                                        {
                                            long id = count;
                                            log += string.Format(
                                                "Message from {0} pushing TBLOPTIONDATAS tables \n" +
                                                "Can't check idoption for entity with N: {1}, Second query\n",
                                                locRem, id);
                                            continue;
                                        }
                                        else
                                        {
                                            TO = tblopt[0];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                            "Connection error - can't check idoption for entity N: {1}\n",
                                            locRem, erc);
                                        log += ex.Message + "\n";
                                        continue;
                                    }
                                    #endregion

                                }

                                double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                                double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                                #region Find data in DB like pushed
                                var tdcs = new List<tbloptiondata>();
                                try
                                {
                                    foreach (var item in tbloptiondatas_.Where(item => item.timetoexpinyears == (futureYear - expiranteYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList())
                                    {
                                        tdcs.Add(item);
                                    }
                                    if (tdcs.Count > 0)
                                    {
                                        long id = tdcs[0].idoptiondata;
                                        log += string.Format(
                                            "Message from {0} pushing TBLOPTIONDATAS tables \n" +
                                            "We already have entity with id: {1}\n",
                                            locRem, id);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                    log += string.Format(
                                        "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                        "Connection error - can't check idcontract for entity N: {1}\n",
                                        locRem, erc);
                                    log += ex.Message + "\n";
                                    continue;
                                }
                                #endregion

                                // callPutFlag                      - tableOption.callorput
                                // S - stock price                  - 1.56
                                // X - strike price of option       - option.StrikePrice
                                // T - time to expiration in years  - 0.5
                                // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                                // currentOptionPrice               - option.SettlementPrice 

                                double impliedvol = OptionCalcs.calculateOptionVolatility(TO.callorput,
                                                                                        1.56,
                                                                                        (option.StrikePrice != null) ? (double)option.StrikePrice : 0,
                                                                                        0.5,
                                                                                        0.08,
                                                                                        (option.SettlementPrice != null) ? (double)option.SettlementPrice : 0);

                                var tableOptionData = new tbloptiondata                                {
                                    //idoptiondata must generate by DB
                                    idoption = TO.idoption,
                                    datetime = option.Date,
                                    price = (option.StrikePrice != null) ? (double)option.StrikePrice : 1,
                                    impliedvol = impliedvol,
                                    timetoexpinyears = futureYear - expiranteYear
                                };

                                tbloptiondatas_.InsertOnSubmit(tableOptionData);
                                context_.SubmitChanges();
                                count++;
                            }
                            catch (OperationCanceledException cancel)
                            {
                                log += string.Format("Cancel message from {0} pushing TBLOPTIONS and TBLOPTIONDATAS table \n", locRem);
                                log += cancel.Message + "\n";
                            }
                            catch (Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing TBLOPTIONS and TBLOPTIONDATAS tables \n" +
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
            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                DateTime stop = DateTime.Now;
                SetLogMessage("Timy with clientside checking - " + (stop - start));

                EnableDisable(false);

                SetLogMessage("Pushed to DB: " + count.ToString() + " entities");
                if (ParsedData.FutureRecords.Length > count)
                {
                    SetLogMessage("Was NOT pushed " + (ParsedData.FutureRecords.Length - count).ToString() + " entities from " + ParsedData.FutureRecords.Length.ToString() + " to DB");
                }
            }
        }

        private async Task PushDataToDBTest(CancellationToken ct)
        {
            var context_ = TestContext;
            var tblcontracts_ = context_.test_tblcontracts;
            var tbldailycontractsettlements_ = context_.test_tbldailycontractsettlements;
            var tbloptions_ = context_.test_tbloptions;
            var tbloptiondatas_ = context_.test_tbloptiondatas;

            int count = 0;
            int globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //int currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length + ((!ParsedData.FuturesOnly) ? ParsedData.OptionRecords.Length : 0);
            Utilities utilites = new Utilities();
            List<string> stripName = new List<string>();
            bool newFuture = true;

            DateTime start = DateTime.Now;

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

                                #region Find data in DB like pushed
                                try
                                {
                                    var tblcontracts = new List<test_tblcontract>();
                                    foreach (var item in tblcontracts_.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToList())
                                    {
                                        tblcontracts.Add(item);
                                    }
                                    int countContracts = tblcontracts.Count;
                                    if (countContracts != 0)
                                    {
                                        long id = tblcontracts[0].idcontract;
                                        log += string.Format(
                                            "Message from {0} pushing TBLCONTRACTS tables \n" +
                                            "We already have entity with id: {1}\n",
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
                                #endregion

                                var tableFuture = new test_tblcontract                                {
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
                                tblcontracts_.InsertOnSubmit(tableFuture);
                                context_.SubmitChanges();
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
                SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

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
                            var contract = tblcontracts_.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                            #region Find data in DB like pushed
                            try
                            {
                                var tdcs = new List<test_tbldailycontractsettlement>();
                                foreach (var item in tbldailycontractsettlements_.Where(item => item.idcontract == contract.idcontract && item.date == future.Date).ToList())
                                {
                                    tdcs.Add(item);
                                }
                                if (tdcs.Count != 0)
                                {
                                    long id = tdcs[0].iddailycontractsettlements;
                                    log += string.Format(
                                        "Message from {0} pushing TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                        "We already have entity with id: {1}\n",
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
                            #endregion

                            var tableDCS = new test_tbldailycontractsettlement                            {
                                //idcontract must generete by DB
                                idcontract = contract.idcontract,
                                date = future.Date,
                                settlement = (future.SettlementPrice != null) ? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                            context_.SubmitChanges();
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
                SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

                if (!ParsedData.FuturesOnly)
                {
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

                                bool isOptionCreated = false;

                                var TO = new test_tbloption();

                                #region Find data in DB like pushed
                                var tbloptions = new List<test_tbloption>();
                                try
                                {
                                    var optlist = tbloptions_.Where(item => item.optionmonth == monthchar && item.optionyear == option.StripName.Year && item.optionname == optionName).ToList();
                                    foreach (var item in optlist)
                                    {
                                        tbloptions.Add(item);
                                    }
                                    int countContracts = tbloptions.Count;
                                    if (countContracts > 0)
                                    {
                                        isOptionCreated = true;
                                        TO = tbloptions[0];
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
                                #endregion

                                if (!isOptionCreated)
                                {
                                    #region Find id contract for pushing option
                                    long idContract;
                                    try
                                    {
                                        idContract = tblcontracts_.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;
                                    }
                                    catch (IndexOutOfRangeException outEx)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                            "We dont have contract for option entity N: {1}\n",
                                            locRem, erc);
                                        log += outEx.Message + "\n";
                                        continue;
                                    }
                                    catch (Exception ex)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing pushing TBLOPTIONS table \n" +
                                            "Can't find contract for option entity N: {1}\n",
                                            locRem, erc);
                                        log += ex.Message + "\n";
                                        continue;
                                    }
                                    #endregion

                                    var tableOption = new test_tbloption                                    {
                                        //idoption must generate by DB
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
                                    tbloptions_.InsertOnSubmit(tableOption);
                                    context_.SubmitChanges();

                                    #region Find id for fist time pushed option
                                    var tblopt = new List<test_tbloption>();
                                    try
                                    {
                                        foreach (var item in tbloptions_.Where(item => item == tableOption).ToList())
                                        {
                                            tblopt.Add(item);
                                        }
                                        if (tblopt.Count == 0)
                                        {
                                            long id = count;
                                            log += string.Format(
                                                "Message from {0} pushing TBLOPTIONDATAS tables \n" +
                                                "Can't check idoption for entity with N: {1}, Second query\n",
                                                locRem, id);
                                            continue;
                                        }
                                        else
                                        {
                                            TO = tblopt[0];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                        log += string.Format(
                                            "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                            "Connection error - can't check idoption for entity N: {1}\n",
                                            locRem, erc);
                                        log += ex.Message + "\n";
                                        continue;
                                    }
                                    #endregion

                                }

                                double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                                double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                                #region Find data in DB like pushed
                                var tdcs = new List<test_tbloptiondata>();
                                try
                                {
                                    foreach (var item in tbloptiondatas_.Where(item => item.timetoexpinyears == (futureYear - expiranteYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList())
                                    {
                                        tdcs.Add(item);
                                    }
                                    if (tdcs.Count > 0)
                                    {
                                        long id = tdcs[0].idoptiondata;
                                        log += string.Format(
                                            "Message from {0} pushing TBLOPTIONDATAS tables \n" +
                                            "We already have entity with id: {1}\n",
                                            locRem, id);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                    log += string.Format(
                                        "ERROR message from {0} pushing TBLOPTIONDATAS tables \n" +
                                        "Connection error - can't check idcontract for entity N: {1}\n",
                                        locRem, erc);
                                    log += ex.Message + "\n";
                                    continue;
                                }
                                #endregion

                                // callPutFlag                      - tableOption.callorput
                                // S - stock price                  - 1.56
                                // X - strike price of option       - option.StrikePrice
                                // T - time to expiration in years  - 0.5
                                // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                                // currentOptionPrice               - option.SettlementPrice 

                                double impliedvol = OptionCalcs.calculateOptionVolatility(TO.callorput,
                                                                                        1.56,
                                                                                        (option.StrikePrice != null) ? (double)option.StrikePrice : 0,
                                                                                        0.5,
                                                                                        0.08,
                                                                                        (option.SettlementPrice != null) ? (double)option.SettlementPrice : 0);

                                var tableOptionData = new test_tbloptiondata                                {
                                    //idoptiondata must generate by DB
                                    idoption = TO.idoption,
                                    datetime = option.Date,
                                    price = (option.StrikePrice != null) ? (double)option.StrikePrice : 1,
                                    impliedvol = impliedvol,
                                    timetoexpinyears = futureYear - expiranteYear
                                };

                                tbloptiondatas_.InsertOnSubmit(tableOptionData);
                                context_.SubmitChanges();
                                count++;
                            }
                            catch (OperationCanceledException cancel)
                            {
                                log += string.Format("Cancel message from {0} pushing TBLOPTIONS and TBLOPTIONDATAS table \n", locRem);
                                log += cancel.Message + "\n";
                            }
                            catch (Exception ex)
                            {
                                int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                                log += string.Format(
                                    "ERROR message from {0} pushing TBLOPTIONS and TBLOPTIONDATAS tables \n" +
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
            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                DateTime stop = DateTime.Now;
                SetLogMessage("Timy with clientside checking - " + (stop - start));

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