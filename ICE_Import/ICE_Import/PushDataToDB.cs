﻿// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING
// WARNING                                                                         WARNING
// WARNING    DO NOT EDIT THIS .CS FILE, BECAUSE ALL YOUR CHANGES WILL BE LOST!    WARNING
// WARNING    EDIT CORRESPONDING .TT FILE INSTEAD!                                 WARNING
// WARNING    ALSO, DO NOT COMMIT THIS .CS FILE!                                   WARNING
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
        int globalCount;
        //int currentPercent;

        async Task PushDataToDB(CancellationToken ct)
        {
            globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = 2 * ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBarLoad.Maximum += ParsedData.OptionRecords.Length;
            }

            DateTime start = DateTime.Now;

            try
            {
                await Task.Run(() => PushFuturesToDB(ct), ct);
                LogElapsedTime(DateTime.Now - start);

                await Task.Run(() => PushDailyFuturesToDB(ct), ct);
                LogElapsedTime(DateTime.Now - start);

                if (!ParsedData.FuturesOnly)
                {
                    await Task.Run(() => PushOptionsToDB(ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                LogMessage("ERROR");
                LogMessage(ex.Message);
            }
#endif
            finally
            {
                DateTime stop = DateTime.Now;
                LogMessage("Timy with clientside checking - " + (stop - start));

                string msg = string.Format("Pushed to DB: {0} entries", globalCount);
                LogMessage(msg);
                if (ParsedData.FutureRecords.Length > globalCount)
                {
                    msg = string.Format("Was NOT pushed {0} entries from {1} to DB",
                        (ParsedData.FutureRecords.Length - globalCount),
                        ParsedData.FutureRecords.Length);
                    LogMessage(msg);
                }

                EnableDisable(false);
            }
        }

        void PushFuturesToDB(CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;

            int count = 0;

            var stripNameHashSet = new HashSet<DateTime>();

            string log = string.Empty;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    bool newFuture = !stripNameHashSet.Contains(future.StripName);
                    if (newFuture)
                    {
                        //TODO: Create query to get idinstrument by description from tblinstruments
                        //idinstrument for description = Cocoa is 36
                        int idinstrument = 36;

                        string contractName = Utilities.GenerateCQGSymbolFromSpan(
                            'F',
                            "CCE",
                            future.Date.Month,
                            future.Date.Year);

                        #region Find data in DB like pushed
                        try
                        {
                            var tblcontracts = new List<tblcontract>();
                            foreach (var item in tblcontracts_.Where(item => item.expirationdate == future.StripName).ToList())
                            {
                                tblcontracts.Add(item);
                            }
                            int countContracts = tblcontracts.Count;
                            if (countContracts != 0)
                            {
                                long id = tblcontracts[0].idcontract;
                                log += string.Format(
                                    "Message from {0} pushing {1}TBLCONTRACTS tables \n" +
                                    "We already have entry with id: {2}\n",
                                    DatabaseName, TablesPrefix, id);
                                continue;
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount;
                            log += string.Format(
                                "ERROR message from {0} pushing {1}TBLCONTRACTS tables \n" +
                                "Can't check idcontract for entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
                        }
                        #endregion

                        var tableFuture = new tblcontract                        {
                            //idcontract must generete by DB

                            //TODO: Create query to get cqgsymbol by description from tblinstruments
                            //cqgsymbol for description = Cocoa is CCE
                            contractname = contractName,
                            month = Utilities.MonthToMonthCode(future.Date.Month),
                            monthint = (short)future.Date.Month,
                            year = (long)future.Date.Year,
                            idinstrument = idinstrument,
                            expirationdate = future.StripName.Date,
                            cqgsymbol = contractName
                        };
                        tblcontracts_.InsertOnSubmit(tableFuture);
                        Context.SubmitChanges();

                        count++;
                        stripNameHashSet.Add(future.StripName);
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Length)
                    {
                        log += string.Format(
                            "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                            count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }    

        void PushDailyFuturesToDB(CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;
            var tbldailycontractsettlements_ = Context.tbldailycontractsettlements;

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthchar = Utilities.MonthToMonthCode(future.Date.Month);
                    var contract = tblcontracts_.Where(item => item.expirationdate == future.StripName).ToArray()[0];

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
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables \n" +
                            "Can't check idcontract for entry N: {2}\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    var tableDCS = new tbldailycontractsettlement                    {
                        //idcontract must generete by DB
                        idcontract = contract.idcontract,
                        date = future.Date,
                        settlement = future.SettlementPrice.GetValueOrDefault(),
                        volume = (long)future.Volume.GetValueOrDefault(),
                        openinterest = (long)future.OpenInterest.GetValueOrDefault()
                    };

                    tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                    Context.SubmitChanges();
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }

        void PushOptionsToDB(CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;
            var tbloptions_ = Context.tbloptions;
            var tbloptiondatas_ = Context.tbloptiondatas;

            int count = 0;

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

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        "CCE",
                        option.Date.Month,
                        option.Date.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        idinstrument);

                    bool isOptionCreated = false;

                    var TO = new tbloption();

                    char monthchar = Utilities.MonthToMonthCode(option.Date.Month);

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
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                            "Can't read N: {2} from DB\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    if (!isOptionCreated)
                    {
                        #region Find id contract for pushing option
                        long idContract;
                        try
                        {
                            idContract = tblcontracts_.Where(item => item.expirationdate == option.StripName).ToList()[0].idcontract;
                        }
                        catch (IndexOutOfRangeException outEx)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                                "We dont have contract for option entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += outEx.Message + "\n";
                            continue;
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                                "Can't find contract for option entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
                        }
                        #endregion

                        var tableOption = new tbloption                        {
                            //idoption must generate by DB
                            optionname = optionName,
                            optionmonth = monthchar,
                            optionmonthint = option.Date.Month,
                            optionyear = option.Date.Year,
                            strikeprice = option.StrikePrice.GetValueOrDefault(),
                            callorput = option.OptionType,
                            idinstrument = idinstrument,
                            expirationdate = option.StripName,
                            idcontract = idContract,
                            cqgsymbol = optionName
                        };
                        tbloptions_.InsertOnSubmit(tableOption);
                        Context.SubmitChanges();

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
                                    "Message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                    "Can't check idoption for entry with N: {2}, Second query\n",
                                    DatabaseName, TablesPrefix, id);
                                continue;
                            }
                            else
                            {
                                TO = tblopt[0];
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                "Connection error - can't check idoption for entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
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
                                "Message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing TBLOPTIONDATAS{1} tables\n" +
                            "Connection error - can't check idcontract for entry N: {2}\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 

                    double impliedvol = OptionCalcs.CalculateOptionVolatility(
                        TO.callorput,
                        1.56,
                        option.StrikePrice.GetValueOrDefault(),
                        0.5,
                        0.08,
                        option.SettlementPrice.GetValueOrDefault());

                    var tableOptionData = new tbloptiondata                    {
                        //idoptiondata must generate by DB
                        idoption = TO.idoption,
                        datetime = option.Date,
                        price = option.StrikePrice.GetValueOrDefault(1),
                        impliedvol = impliedvol,
                        timetoexpinyears = futureYear - expiranteYear
                    };

                    tbloptiondatas_.InsertOnSubmit(tableOptionData);
                    Context.SubmitChanges();
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                    log += string.Format(
                        "ERROR message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n" +
                        "Can't push entry N: {2}\n",
                        DatabaseName, TablesPrefix, erc);
                    log += ex.Message + "\n";
                    continue;
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }

        async Task PushDataToDBTest(CancellationToken ct)
        {
            globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = 2 * ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBarLoad.Maximum += ParsedData.OptionRecords.Length;
            }

            DateTime start = DateTime.Now;

            try
            {
                await Task.Run(() => PushFuturesToDBTest(ct), ct);
                LogElapsedTime(DateTime.Now - start);

                await Task.Run(() => PushDailyFuturesToDBTest(ct), ct);
                LogElapsedTime(DateTime.Now - start);

                if (!ParsedData.FuturesOnly)
                {
                    await Task.Run(() => PushOptionsToDBTest(ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                LogMessage("ERROR");
                LogMessage(ex.Message);
            }
#endif
            finally
            {
                DateTime stop = DateTime.Now;
                LogMessage("Timy with clientside checking - " + (stop - start));

                string msg = string.Format("Pushed to DB: {0} entries", globalCount);
                LogMessage(msg);
                if (ParsedData.FutureRecords.Length > globalCount)
                {
                    msg = string.Format("Was NOT pushed {0} entries from {1} to DB",
                        (ParsedData.FutureRecords.Length - globalCount),
                        ParsedData.FutureRecords.Length);
                    LogMessage(msg);
                }

                EnableDisable(false);
            }
        }

        void PushFuturesToDBTest(CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;

            int count = 0;

            var stripNameHashSet = new HashSet<DateTime>();

            string log = string.Empty;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    bool newFuture = !stripNameHashSet.Contains(future.StripName);
                    if (newFuture)
                    {
                        //TODO: Create query to get idinstrument by description from tblinstruments
                        //idinstrument for description = Cocoa is 36
                        int idinstrument = 36;

                        string contractName = Utilities.GenerateCQGSymbolFromSpan(
                            'F',
                            "CCE",
                            future.Date.Month,
                            future.Date.Year);

                        #region Find data in DB like pushed
                        try
                        {
                            var tblcontracts = new List<test_tblcontract>();
                            foreach (var item in tblcontracts_.Where(item => item.expirationdate == future.StripName).ToList())
                            {
                                tblcontracts.Add(item);
                            }
                            int countContracts = tblcontracts.Count;
                            if (countContracts != 0)
                            {
                                long id = tblcontracts[0].idcontract;
                                log += string.Format(
                                    "Message from {0} pushing {1}TBLCONTRACTS tables \n" +
                                    "We already have entry with id: {2}\n",
                                    DatabaseName, TablesPrefix, id);
                                continue;
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount;
                            log += string.Format(
                                "ERROR message from {0} pushing {1}TBLCONTRACTS tables \n" +
                                "Can't check idcontract for entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
                        }
                        #endregion

                        var tableFuture = new test_tblcontract                        {
                            //idcontract must generete by DB

                            //TODO: Create query to get cqgsymbol by description from tblinstruments
                            //cqgsymbol for description = Cocoa is CCE
                            contractname = contractName,
                            month = Utilities.MonthToMonthCode(future.Date.Month),
                            monthint = (short)future.Date.Month,
                            year = (long)future.Date.Year,
                            idinstrument = idinstrument,
                            expirationdate = future.StripName.Date,
                            cqgsymbol = contractName
                        };
                        tblcontracts_.InsertOnSubmit(tableFuture);
                        Context.SubmitChanges();

                        count++;
                        stripNameHashSet.Add(future.StripName);
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Length)
                    {
                        log += string.Format(
                            "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                            count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }    

        void PushDailyFuturesToDBTest(CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;
            var tbldailycontractsettlements_ = Context.test_tbldailycontractsettlements;

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthchar = Utilities.MonthToMonthCode(future.Date.Month);
                    var contract = tblcontracts_.Where(item => item.expirationdate == future.StripName).ToArray()[0];

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
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables \n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables \n" +
                            "Can't check idcontract for entry N: {2}\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    var tableDCS = new test_tbldailycontractsettlement                    {
                        //idcontract must generete by DB
                        idcontract = contract.idcontract,
                        date = future.Date,
                        settlement = future.SettlementPrice.GetValueOrDefault(),
                        volume = (long)future.Volume.GetValueOrDefault(),
                        openinterest = (long)future.OpenInterest.GetValueOrDefault()
                    };

                    tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                    Context.SubmitChanges();
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }

        void PushOptionsToDBTest(CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;
            var tbloptions_ = Context.test_tbloptions;
            var tbloptiondatas_ = Context.test_tbloptiondatas;

            int count = 0;

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

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        "CCE",
                        option.Date.Month,
                        option.Date.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        idinstrument);

                    bool isOptionCreated = false;

                    var TO = new test_tbloption();

                    char monthchar = Utilities.MonthToMonthCode(option.Date.Month);

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
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                            "Can't read N: {2} from DB\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    if (!isOptionCreated)
                    {
                        #region Find id contract for pushing option
                        long idContract;
                        try
                        {
                            idContract = tblcontracts_.Where(item => item.expirationdate == option.StripName).ToList()[0].idcontract;
                        }
                        catch (IndexOutOfRangeException outEx)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                                "We dont have contract for option entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += outEx.Message + "\n";
                            continue;
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                                "Can't find contract for option entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
                        }
                        #endregion

                        var tableOption = new test_tbloption                        {
                            //idoption must generate by DB
                            optionname = optionName,
                            optionmonth = monthchar,
                            optionmonthint = option.Date.Month,
                            optionyear = option.Date.Year,
                            strikeprice = option.StrikePrice.GetValueOrDefault(),
                            callorput = option.OptionType,
                            idinstrument = idinstrument,
                            expirationdate = option.StripName,
                            idcontract = idContract,
                            cqgsymbol = optionName
                        };
                        tbloptions_.InsertOnSubmit(tableOption);
                        Context.SubmitChanges();

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
                                    "Message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                    "Can't check idoption for entry with N: {2}, Second query\n",
                                    DatabaseName, TablesPrefix, id);
                                continue;
                            }
                            else
                            {
                                TO = tblopt[0];
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                            log += string.Format(
                                "ERROR message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                "Connection error - can't check idoption for entry N: {2}\n",
                                DatabaseName, TablesPrefix, erc);
                            log += ex.Message + "\n";
                            continue;
                        }
#endif
                        finally
                        {
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
                                "Message from {0} pushing {1}TBLOPTIONDATAS table\n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                        log += string.Format(
                            "ERROR message from {0} pushing TBLOPTIONDATAS{1} tables\n" +
                            "Connection error - can't check idcontract for entry N: {2}\n",
                            DatabaseName, TablesPrefix, erc);
                        log += ex.Message + "\n";
                        continue;
                    }
#endif
                    finally
                    {
                    }
                    #endregion

                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 

                    double impliedvol = OptionCalcs.CalculateOptionVolatility(
                        TO.callorput,
                        1.56,
                        option.StrikePrice.GetValueOrDefault(),
                        0.5,
                        0.08,
                        option.SettlementPrice.GetValueOrDefault());

                    var tableOptionData = new test_tbloptiondata                    {
                        //idoptiondata must generate by DB
                        idoption = TO.idoption,
                        datetime = option.Date,
                        price = option.StrikePrice.GetValueOrDefault(1),
                        impliedvol = impliedvol,
                        timetoexpinyears = futureYear - expiranteYear
                    };

                    tbloptiondatas_.InsertOnSubmit(tableOptionData);
                    Context.SubmitChanges();
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                    log += string.Format(
                        "ERROR message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n" +
                        "Can't push entry N: {2}\n",
                        DatabaseName, TablesPrefix, erc);
                    log += ex.Message + "\n";
                    continue;
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", count, DatabaseName, TablesPrefix);
                    }
                    //TODO: Fix this
                    //if (count % (10 * percent) > 0 && count % (10 * percent) < 0.5)
                    //{
                    //    currentPercent += 10;
                    //    log += "Current progress: " + currentPercent.ToString() + "% - " + count.ToString() + " entries" + "\n";
                    //}
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, globalCount);
                    log = string.Empty;
                }
            }
        }

    }
}