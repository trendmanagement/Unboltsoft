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
        Dictionary<int, DateTime> expirationtimeDictionary = new Dictionary<int, DateTime>();

        /// <summary>
        /// Push all data to DB with stored procedures.
        /// Update either test or non-test tables, either synchronously or asynchronously.
        /// </summary>
        async Task PushDataToDBWithSPs(CancellationToken ct)
        {
            progressBar.Maximum = ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Length;
            }

            int globalCount = 0;
            DateTime start = DateTime.Now;

            AsyncTaskListener.Init();

            try
            {
                if (IsAsyncUpdate)
                {
                    // Clear commands queue just in case if the previous session was terminated
                    StoredProcsHelper.InitAsyncHelper();
                }

                AsyncTaskListener.Init("Pushing of FUTURES data started");
                await Task.Run(() => PushFuturesToDBWithSP(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);
                AsyncTaskListener.LogMessage("Pushing of FUTURES data complete");

                if (!ParsedData.FuturesOnly)
                {
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data started");
                    await Task.Run(() => PushOptionsToDBWithSP(ref globalCount, ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data complete");
                }

                if (IsAsyncUpdate)
                {
                    // Flush the remaining commands
                    StoredProcsHelper.FinalizeAsyncHelper();
                }
            }
            catch (OperationCanceledException)
            {
                // Already logged
            }
            finally
            {
                EnableDisable(false);
            }

            LogMessage(string.Format("Pushed to DB: {0} entries", globalCount));
        }

        /// <summary>
        /// Push futures to DB with stored procedures.
        /// </summary>
        void PushFuturesToDBWithSP(ref int globalCount, CancellationToken ct)
        {
            var stripNameHashSet = new HashSet<DateTime>();

            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                bool newFuture = !stripNameHashSet.Contains(future.StripName);
                int idinstrument = 36;

                char monthchar = Utilities.MonthToMonthCode(future.StripName.Month);

                string contractname = Utilities.GenerateCQGSymbolFromSpan(
                    'F',
                    "CCE",
                    monthchar,
                    future.StripName.Year);

                    string log = string.Empty;
                    try
                    {
                    if (newFuture)
                    {
                        #region Get expirationtime
                        if (DatabaseName == "TMLDB")
                        {
                            StoredProcsHelper.SPF_Mod(
                                contractname,
                                monthchar,
                                future.StripName.Month,
                                future.StripName.Year,
                                idinstrument,
                                contractname);
                            stripNameHashSet.Add(future.StripName);
                        }
                        else
                        {
                            DateTime expirationtime;
                            int key = future.StripName.Month + future.StripName.Year + idinstrument;
                            if (expirationtimeDictionary.ContainsKey(key))
                            {
                                expirationtime = expirationtimeDictionary[key];
                            }
                            else
                            {
                                var records = ContextTMLDB.tblcontractexpirations.Where(
                                        item =>
                                        item.optionmonthint == future.StripName.Month &&
                                        item.optionyear == future.StripName.Year &&
                                        item.idinstrument == idinstrument).ToArray();

                                if (records.Length == 0)
                                {
                                    log += string.Format("Failed to find expiration date for future with month = {0}, year = {1}, idinstrument = {2} in tblcontractexpirations", future.StripName.Month, future.StripName.Year, idinstrument);
                                    expirationtime = new DateTime();
                                }
                                else
                                {
                                    expirationtime = records[0].expirationdate;
                                    int newKey = future.StripName.Month + future.StripName.Year + idinstrument;
                                    expirationtimeDictionary.Add(newKey, expirationtime);
                                }
                            }
                            #endregion

                            StoredProcsHelper.SPF(
                                contractname,
                                monthchar,
                                future.StripName.Month,
                                future.StripName.Year,
                                idinstrument,
                                expirationtime,
                                contractname);
                            stripNameHashSet.Add(future.StripName);
                        }
                    }

                    StoredProcsHelper.SPDF(
                        future.Date,
                        future.SettlementPrice.GetValueOrDefault(),
                        monthchar,
                        future.StripName.Year,
                        (long)future.Volume.GetValueOrDefault(),
                        (long)future.OpenInterest.GetValueOrDefault());
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
                            globalCount, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

        /// <summary>
        /// Push options and optionsdata to DB with stored procedures.
        /// </summary>
        void PushOptionsToDBWithSP(ref int globalCount, CancellationToken ct)
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

                    char monthchar = Utilities.MonthToMonthCode(option.StripName.Month);

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        "CCE",
                        monthchar,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        idinstrument);

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

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expirateYear = option.Date.Year + option.Date.Month * 0.0833333;

                    #region Get expirationtime
                    DateTime expirationtime;
                    int key = option.StripName.Month + option.StripName.Year + idinstrument;
                    if (expirationtimeDictionary.ContainsKey(key))
                    {
                        expirationtime = expirationtimeDictionary[key];
                    }
                    else
                    {
                        var records = ContextTMLDB.tblcontractexpirations.Where(
                            item =>
                            item.optionmonthint == option.StripName.Month &&
                            item.optionyear == option.StripName.Year &&
                            item.idinstrument == idinstrument).ToArray();

                        if (records.Length == 0)
                        {
                            log += string.Format("Failed to find expiration date for future with month = {0}, year = {1}, idinstrument = {2} in tblcontractexpirations", option.StripName.Month, option.StripName.Year, idinstrument);
                            expirationtime = new DateTime();
                        }
                        else
                        {
                            expirationtime = records[0].expirationdate;
                            int newKey = option.StripName.Month + option.StripName.Year + idinstrument;
                            expirationtimeDictionary.Add(newKey, expirationtime);
                        }
                    }
                    #endregion

                    StoredProcsHelper.SPO(
                        optionName,
                        monthchar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        option.OptionType,
                        idinstrument,
                        expirationtime,
                        optionName);

                    #region Get idoption
                    long idoption = 0;
                    if (cb_TestTables.Checked)
                    {
                        var tbloptions = new List<test_tbloption>();
                        try
                        {
                            var optlist = Context.test_tbloptions.Where(item => item.optionname == optionName).ToList();
                            foreach (var item in optlist)
                            {
                                tbloptions.Add(item);
                            }
                            int countContracts = tbloptions.Count;
                            if (countContracts > 0)
                            {
                                idoption = tbloptions[0].idoption;
                            }
                        }
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
                    }
                    else
                    {
                        var tbloptions = new List<tbloption>();
                        try
                        {
                            var optlist = Context.tbloptions.Where(item => item.optionname == optionName).ToList();
                            foreach (var item in optlist)
                            {
                                tbloptions.Add(item);
                            }
                            int countContracts = tbloptions.Count;
                            if (countContracts > 0)
                            {
                                idoption = tbloptions[0].idoption;
                            }
                        }
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
                    }
                    #endregion

                    StoredProcsHelper.SPOD(
                        (int)idoption,
                        option.Date,
                        option.SettlementPrice.GetValueOrDefault(),
                        impliedvol,
                        futureYear - expirateYear);
                }
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
                finally
                {
                    globalCount++;
                    if (globalCount == 2 * ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }

        bool GetRisk()
        {
            AsyncTaskListener.LogMessage("Reading Risk...");

            try
            {
                var idoptioninputsymbol = ContextTMLDB.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == 1).ToArray()[0].idoptioninputsymbol;
                tbloptioninputdata[] tbloptioninputdatas = ContextTMLDB.tbloptioninputdatas.Where(item =>
                   item.idoptioninputsymbol == idoptioninputsymbol).ToArray();
                DateTime optioninputdatetime = new DateTime();
                for (int i = 0; i < tbloptioninputdatas.Length; i++)
                {
                    if (i != 0)
                    {
                        if (optioninputdatetime < tbloptioninputdatas[i].optioninputdatetime)
                        {
                            optioninputdatetime = tbloptioninputdatas[i].optioninputdatetime;
                        }
                    }
                    else
                    {
                        optioninputdatetime = tbloptioninputdatas[i].optioninputdatetime;
                    }
                }

                //--?-- From where this varable in query
                var OPTION_INPUT_TYPE_RISK_FREE_RATE = 1;

                //--?-- What difference between idoptioninputsymbol and idoptioninputsymbol2
                var idoptioninputsymbol2 = ContextTMLDB.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == OPTION_INPUT_TYPE_RISK_FREE_RATE).ToArray()[0].idoptioninputsymbol;

                RiskFreeInterestRate = ContextTMLDB.tbloptioninputdatas.Where(item =>
                    item.idoptioninputsymbol == idoptioninputsymbol2
                        && item.optioninputdatetime == optioninputdatetime).ToArray()[0].optioninputclose;

                AsyncTaskListener.LogMessage(string.Format("Risk = {0}", RiskFreeInterestRate));

                return true;
            }
            catch
            {
                return false;
            }
        }

        bool GetTickSize()
        {
            AsyncTaskListener.LogMessage("Reading Tick size...");

            try
            {
                var secondaryoptionticksize = ContextTMLDB.tblinstruments.Where(item => item.idinstrument == 36).ToArray()[0].secondaryoptionticksize;

                if (secondaryoptionticksize > 0)
                {
                    TickSize = secondaryoptionticksize;
                }
                else
                {
                    TickSize = ContextTMLDB.tblinstruments.Where(item => item.idinstrument == 36).ToArray()[0].optionticksize;
                }

                AsyncTaskListener.LogMessage(string.Format("Tick size = {0}", TickSize));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}