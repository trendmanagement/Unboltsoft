// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING
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
        async Task PushDataToDB(CancellationToken ct)
        {
            progressBar.Maximum = 2 * ParsedData.FutureRecords.Count;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Count;
            }

            int globalCount = 0;
            DateTime start = DateTime.Now;

            AsyncTaskListener.Init();

            try
            {
                AsyncTaskListener.Init("Pushing of FUTURES data started");

                await Task.Run(() => PushFuturesToDB(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                await Task.Run(() => PushDailyFuturesToDB(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                AsyncTaskListener.LogMessage("Pushing of FUTURES data complete");

                if (!ParsedData.FuturesOnly)
                {
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data started");
                    await Task.Run(() => PushOptionsToDB(ref globalCount, ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data complete");
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
                string msg = string.Format("Pushed to DB: {0} entries", globalCount);
                LogMessage(msg);
                if (ParsedData.FutureRecords.Count > globalCount)
                {
                    msg = string.Format("NOT pushed to DB: {0} entries from {1}",
                        ParsedData.FutureRecords.Count - globalCount,
                        ParsedData.FutureRecords.Count);
                    LogMessage(msg);
                }

                EnableDisable(false);
            }
        }

        void PushFuturesToDB(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;
            var tblContractList = new List<tblcontract>();

            int count = 0;

            StripNameHashSet = new HashSet<DateTime>();
            StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            string log = string.Empty;
            foreach (EOD_Futures future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    bool newFuture = !StripNameHashSet.Contains(future.StripName);
                    if (newFuture)
                    {
                        char monthChar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                        string contractName = Utilities.GenerateCQGSymbolFromSpan(
                            'F',
                            CqgSymbol,
                            monthChar, 
                            future.Date.Year);

                        #region Find data in DB like pushed
                        try
                        {
                            tblContractList = tblcontracts_.Where(item => item.month == monthChar && item.year == future.StripName.Year).ToList();

                            int countContracts = tblContractList.Count;
                            if (countContracts != 0)
                            {
                                long id = tblContractList[0].idcontract;
                                log += string.Format(
                                    "Message from {0} pushing {1}TBLCONTRACTS tables\n" +
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
                                "ERROR message from {0} pushing {1}TBLCONTRACTS tables\n" +
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

                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "future",
                            IdInstrument,
                            future.StripName,
                            ref log);

                        var tableFuture = new tblcontract 
                        {
                            contractname = contractName,
                            month = monthChar,
                            monthint = (short)future.StripName.Month,
                            year = (long)future.StripName.Year,
                            idinstrument = IdInstrument,
                            expirationdate = expirationDate,
                            cqgsymbol = contractName
                        };
                        tblcontracts_.InsertOnSubmit(tableFuture);
                        Context.SubmitChanges();    // idcontract is generated by DB
                        StripNameHashSet.Add(future.StripName);
                        count++;
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLCONTRACT table\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLCONTRACT table\n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Count)
                    {
                        log += string.Format(
                            "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                            count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }    

        void PushDailyFuturesToDB(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;
            var tbldailycontractsettlements_ = Context.tbldailycontractsettlements;
            var tblDailyContractList = new List<tbldailycontractsettlement>();
            var tblContractList = new List<tblcontract>();

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Futures future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthChar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                    #region Find contract
                    try
                    {
                        tblContractList = tblcontracts_.Where(item => item.month == monthChar && item.year == future.StripName.Year).ToList();

                        if (tblContractList.Count == 0)
                        {
                            var stripName = monthChar + future.StripName.Year.ToString();
                            log += string.Format(
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
                                "Can't find contract for: {2}\n",
                                DatabaseName, TablesPrefix, stripName);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
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

                    #region Find data in DB like pushed
                    try
                    {
                        tblDailyContractList = tbldailycontractsettlements_.Where(item => item.idcontract == tblContractList[0].idcontract && item.date == future.Date).ToList();
                        if (tblDailyContractList.Count > 0)
                        {
                            long id = tblDailyContractList[0].iddailycontractsettlements;
                            log += string.Format(
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
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

                    var tableDCS = new tbldailycontractsettlement 
                    {
                        idcontract = tblContractList[0].idcontract, // generated by DB
                        date = future.Date,
                        settlement = future.SettlementPrice.GetValueOrDefault(),
                        volume = (long)future.Volume.GetValueOrDefault(),
                        openinterest = (long)future.OpenInterest.GetValueOrDefault()
                    };

                    tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                    Context.SubmitChanges();
                    StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table\n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Count)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

        void PushOptionsToDB(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.tblcontracts;
            var tbloptions_ = Context.tbloptions;
            var tbloptiondatas_ = Context.tbloptiondatas;
            var tblContractList = new List<tblcontract>();
            var tblOptionList = new List<tbloption>();
            var tblOptionDataList = new List<tbloptiondata>();

            OptionDataHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Options option in ParsedData.OptionRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthChar = Convert.ToChar(((MonthCodes)option.StripName.Month).ToString());

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        CqgSymbol,
                        monthChar,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        IdInstrument);

                    bool isOptionCreated = false;

                    var TO = new tbloption();

                    #region Find data in DB like pushed
                    try
                    {
                        tblOptionList = tbloptions_.Where(item => item.optionname == optionName).ToList();
                        int countContracts = tblOptionList.Count;
                        if (countContracts > 0)
                        {
                            isOptionCreated = true;
                            TO = tblOptionList[0];
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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
                            idContract = tblcontracts_.Where(item => item.month == monthChar && item.year == option.StripName.Year).First().idcontract;
                        }
                        catch (InvalidOperationException)
                        {
                            // This option does not have corresponding future -- just skip it
                            // (we already informed user about that)
                            continue;
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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

                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "option",
                            IdInstrument,
                            option.StripName,
                            ref log);

                        var tableOption = new tbloption 
                        {
                            optionname = optionName,
                            optionmonth = monthChar,
                            optionmonthint = option.StripName.Month,
                            optionyear = option.StripName.Year,
                            strikeprice = option.StrikePrice.GetValueOrDefault(),
                            callorput = option.OptionType,
                            idinstrument = IdInstrument,
                            expirationdate = expirationDate,
                            idcontract = idContract,
                            cqgsymbol = optionName
                        };
                        tbloptions_.InsertOnSubmit(tableOption);
                        Context.SubmitChanges();    // idoption is generated by DB

                        #region Find id for fist time pushed option
                        try
                        {
                            tblOptionList = new List<tbloption>();
                            tblOptionList = tbloptions_.Where(item => item == tableOption).ToList();
                            if (tblOptionList.Count == 0)
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
                                TO = tblOptionList[0];
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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

                    double futureYear = option.StripName.Year + option.StripName.Month / 12.0;
                    double expirateYear = option.Date.Year + option.Date.Month / 12.0;

                    #region Find data in DB like pushed
                    try
                    {
                        tblOptionDataList = tbloptiondatas_.Where(item => item.timetoexpinyears == (futureYear - expirateYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList();
                        if (tblOptionDataList.Count > 0)
                        {
                            long id = tblOptionDataList[0].idoptiondata;
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
                        int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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

                    #region Implied Volatility
                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - from table tbloptioninputdata
                    // currentOptionPrice               - option.SettlementPrice 
                    // tickSize                         - from table tblinstruments (secondaryoptionticksize or optionticksize)

                    double impliedvol = OptionCalcs.CalculateOptionVolatilityNR(
                        option.OptionType,
                        1.56,
                        Utilities.NormalizePrice(option.StrikePrice),
                        0.5,
                        RiskFreeInterestRate,
                        Utilities.NormalizePrice(option.SettlementPrice),
                        TickSize);
                    #endregion

                    var tableOptionData = new tbloptiondata 
                    {
                        //idoptiondata must generate by DB
                        idoption = TO.idoption,
                        datetime = option.Date,
                        price = option.SettlementPrice.GetValueOrDefault(),
                        impliedvol = impliedvol,
                        timetoexpinyears = futureYear - expirateYear
                    };

                    tbloptiondatas_.InsertOnSubmit(tableOptionData);
                    Context.SubmitChanges();
                    //OptionDataHashSet.Add(TO.idoption);
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
                    int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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
                    if (globalCount == 2 * ParsedData.FutureRecords.Count + ParsedData.OptionRecords.Count)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

        async Task PushDataToDBTest(CancellationToken ct)
        {
            progressBar.Maximum = 2 * ParsedData.FutureRecords.Count;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Count;
            }

            int globalCount = 0;
            DateTime start = DateTime.Now;

            AsyncTaskListener.Init();

            try
            {
                AsyncTaskListener.Init("Pushing of FUTURES data started");

                await Task.Run(() => PushFuturesToDBTest(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                await Task.Run(() => PushDailyFuturesToDBTest(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                AsyncTaskListener.LogMessage("Pushing of FUTURES data complete");

                if (!ParsedData.FuturesOnly)
                {
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data started");
                    await Task.Run(() => PushOptionsToDBTest(ref globalCount, ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data complete");
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
                string msg = string.Format("Pushed to DB: {0} entries", globalCount);
                LogMessage(msg);
                if (ParsedData.FutureRecords.Count > globalCount)
                {
                    msg = string.Format("NOT pushed to DB: {0} entries from {1}",
                        ParsedData.FutureRecords.Count - globalCount,
                        ParsedData.FutureRecords.Count);
                    LogMessage(msg);
                }

                EnableDisable(false);
            }
        }

        void PushFuturesToDBTest(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;
            var tblContractList = new List<test_tblcontract>();

            int count = 0;

            StripNameHashSet = new HashSet<DateTime>();
            StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            string log = string.Empty;
            foreach (EOD_Futures future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    bool newFuture = !StripNameHashSet.Contains(future.StripName);
                    if (newFuture)
                    {
                        char monthChar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                        string contractName = Utilities.GenerateCQGSymbolFromSpan(
                            'F',
                            CqgSymbol,
                            monthChar, 
                            future.Date.Year);

                        #region Find data in DB like pushed
                        try
                        {
                            tblContractList = tblcontracts_.Where(item => item.month == monthChar && item.year == future.StripName.Year).ToList();

                            int countContracts = tblContractList.Count;
                            if (countContracts != 0)
                            {
                                long id = tblContractList[0].idcontract;
                                log += string.Format(
                                    "Message from {0} pushing {1}TBLCONTRACTS tables\n" +
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
                                "ERROR message from {0} pushing {1}TBLCONTRACTS tables\n" +
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

                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "future",
                            IdInstrument,
                            future.StripName,
                            ref log);

                        var tableFuture = new test_tblcontract 
                        {
                            contractname = contractName,
                            month = monthChar,
                            monthint = (short)future.StripName.Month,
                            year = (long)future.StripName.Year,
                            idinstrument = IdInstrument,
                            expirationdate = expirationDate,
                            cqgsymbol = contractName
                        };
                        tblcontracts_.InsertOnSubmit(tableFuture);
                        Context.SubmitChanges();    // idcontract is generated by DB
                        StripNameHashSet.Add(future.StripName);
                        count++;
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLCONTRACT table\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLCONTRACT table\n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Count)
                    {
                        log += string.Format(
                            "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                            count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }    

        void PushDailyFuturesToDBTest(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;
            var tbldailycontractsettlements_ = Context.test_tbldailycontractsettlements;
            var tblDailyContractList = new List<test_tbldailycontractsettlement>();
            var tblContractList = new List<test_tblcontract>();

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Futures future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthChar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());

                    #region Find contract
                    try
                    {
                        tblContractList = tblcontracts_.Where(item => item.month == monthChar && item.year == future.StripName.Year).ToList();

                        if (tblContractList.Count == 0)
                        {
                            var stripName = monthChar + future.StripName.Year.ToString();
                            log += string.Format(
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
                                "Can't find contract for: {2}\n",
                                DatabaseName, TablesPrefix, stripName);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
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

                    #region Find data in DB like pushed
                    try
                    {
                        tblDailyContractList = tbldailycontractsettlements_.Where(item => item.idcontract == tblContractList[0].idcontract && item.date == future.Date).ToList();
                        if (tblDailyContractList.Count > 0)
                        {
                            long id = tblDailyContractList[0].iddailycontractsettlements;
                            log += string.Format(
                                "Message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
                                "We already have entry with id: {2}\n",
                                DatabaseName, TablesPrefix, id);
                            continue;
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT tables\n" +
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

                    var tableDCS = new test_tbldailycontractsettlement 
                    {
                        idcontract = tblContractList[0].idcontract, // generated by DB
                        date = future.Date,
                        settlement = future.SettlementPrice.GetValueOrDefault(),
                        volume = (long)future.Volume.GetValueOrDefault(),
                        openinterest = (long)future.OpenInterest.GetValueOrDefault()
                    };

                    tbldailycontractsettlements_.InsertOnSubmit(tableDCS);
                    Context.SubmitChanges();
                    StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));
                    count++;
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                    break;
                }
#if !DEBUG
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table\n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
#endif
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Count)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

        void PushOptionsToDBTest(ref int globalCount, CancellationToken ct)
        {
            var tblcontracts_ = Context.test_tblcontracts;
            var tbloptions_ = Context.test_tbloptions;
            var tbloptiondatas_ = Context.test_tbloptiondatas;
            var tblContractList = new List<test_tblcontract>();
            var tblOptionList = new List<test_tbloption>();
            var tblOptionDataList = new List<test_tbloptiondata>();

            OptionDataHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            int count = 0;

            string log = string.Empty;
            foreach (EOD_Options option in ParsedData.OptionRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    char monthChar = Convert.ToChar(((MonthCodes)option.StripName.Month).ToString());

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        CqgSymbol,
                        monthChar,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        IdInstrument);

                    bool isOptionCreated = false;

                    var TO = new test_tbloption();

                    #region Find data in DB like pushed
                    try
                    {
                        tblOptionList = tbloptions_.Where(item => item.optionname == optionName).ToList();
                        int countContracts = tblOptionList.Count;
                        if (countContracts > 0)
                        {
                            isOptionCreated = true;
                            TO = tblOptionList[0];
                        }
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                        log += string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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
                            idContract = tblcontracts_.Where(item => item.month == monthChar && item.year == option.StripName.Year).First().idcontract;
                        }
                        catch (InvalidOperationException)
                        {
                            // This option does not have corresponding future -- just skip it
                            // (we already informed user about that)
                            continue;
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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

                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "option",
                            IdInstrument,
                            option.StripName,
                            ref log);

                        var tableOption = new test_tbloption 
                        {
                            optionname = optionName,
                            optionmonth = monthChar,
                            optionmonthint = option.StripName.Month,
                            optionyear = option.StripName.Year,
                            strikeprice = option.StrikePrice.GetValueOrDefault(),
                            callorput = option.OptionType,
                            idinstrument = IdInstrument,
                            expirationdate = expirationDate,
                            idcontract = idContract,
                            cqgsymbol = optionName
                        };
                        tbloptions_.InsertOnSubmit(tableOption);
                        Context.SubmitChanges();    // idoption is generated by DB

                        #region Find id for fist time pushed option
                        try
                        {
                            tblOptionList = new List<test_tbloption>();
                            tblOptionList = tbloptions_.Where(item => item == tableOption).ToList();
                            if (tblOptionList.Count == 0)
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
                                TO = tblOptionList[0];
                            }
                        }
#if !DEBUG
                        catch (Exception ex)
                        {
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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

                    double futureYear = option.StripName.Year + option.StripName.Month / 12.0;
                    double expirateYear = option.Date.Year + option.Date.Month / 12.0;

                    #region Find data in DB like pushed
                    try
                    {
                        tblOptionDataList = tbloptiondatas_.Where(item => item.timetoexpinyears == (futureYear - expirateYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList();
                        if (tblOptionDataList.Count > 0)
                        {
                            long id = tblOptionDataList[0].idoptiondata;
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
                        int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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

                    #region Implied Volatility
                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - from table tbloptioninputdata
                    // currentOptionPrice               - option.SettlementPrice 
                    // tickSize                         - from table tblinstruments (secondaryoptionticksize or optionticksize)

                    double impliedvol = OptionCalcs.CalculateOptionVolatilityNR(
                        option.OptionType,
                        1.56,
                        Utilities.NormalizePrice(option.StrikePrice),
                        0.5,
                        RiskFreeInterestRate,
                        Utilities.NormalizePrice(option.SettlementPrice),
                        TickSize);
                    #endregion

                    var tableOptionData = new test_tbloptiondata 
                    {
                        //idoptiondata must generate by DB
                        idoption = TO.idoption,
                        datetime = option.Date,
                        price = option.SettlementPrice.GetValueOrDefault(),
                        impliedvol = impliedvol,
                        timetoexpinyears = futureYear - expirateYear
                    };

                    tbloptiondatas_.InsertOnSubmit(tableOptionData);
                    Context.SubmitChanges();
                    //OptionDataHashSet.Add(TO.idoption);
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
                    int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
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
                    if (globalCount == 2 * ParsedData.FutureRecords.Count + ParsedData.OptionRecords.Count)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", count, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

    }
}