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
        /// <summary>
        /// Push all data to DB with stored procedures.
        /// Update either test or non-test tables, either synchronously or asynchronously.
        /// </summary>
        async Task PushDataToDBWithSPs(CancellationToken ct)
        {
            progressBar.Maximum = ParsedData.FutureRecords.Count;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Count;
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
            StripNameHashSet = new HashSet<DateTime>();
            StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            foreach (EOD_Futures future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                bool newFuture = !StripNameHashSet.Contains(future.StripName);

                char monthChar = Utilities.MonthToMonthCode(future.StripName.Month);

                string contractname = Utilities.GenerateCQGSymbolFromSpan(
                    'F',
                    CqgSymbol,
                    monthChar,
                    future.StripName.Year);

                string log = string.Empty;
                try
                {
                    if (newFuture)
                    {
                        if (DatabaseName == "TMLDB")
                        {
                            StoredProcsHelper.SPF_Mod(
                                contractname,
                                monthChar,
                                future.StripName.Month,
                                future.StripName.Year,
                                (int)IdInstrument,
                                contractname);

                            StripNameHashSet.Add(future.StripName);
                        }
                        else
                        {
                            DateTime expirationDate = TMLDBReader.GetExpirationDate(
                                "future",
                                IdInstrument,
                                future.StripName,
                                ref log);

                            StoredProcsHelper.SPF(
                                contractname,
                                monthChar,
                                future.StripName.Month,
                                future.StripName.Year,
                                (int)IdInstrument,
                                expirationDate,
                                contractname);
                            StripNameHashSet.Add(future.StripName);
                        }
                    }

                    StoredProcsHelper.SPDF(
                        future.Date,
                        future.SettlementPrice.GetValueOrDefault(),
                        monthChar,
                        future.StripName.Year,
                        (long)future.Volume.GetValueOrDefault(),
                        (long)future.OpenInterest.GetValueOrDefault());

                    StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));
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
            IdOptionHashSet = new HashSet<long>();

            string log = string.Empty;
            foreach (EOD_Options option in ParsedData.OptionRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    char monthChar = Utilities.MonthToMonthCode(option.StripName.Month);

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        CqgSymbol,
                        monthChar,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        IdInstrument);

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

                    double futureYear = option.StripName.Year + option.StripName.Month / 12.0;
                    double expirateYear = option.Date.Year + option.Date.Month / 12.0;

                    DateTime expirationDate = TMLDBReader.GetExpirationDate(
                        "option",
                        IdInstrument,
                        option.StripName,
                        ref log);

                    StoredProcsHelper.SPO(
                        optionName,
                        monthChar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        option.OptionType,
                        IdInstrument,
                        expirationDate,
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
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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
                            int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                            log += string.Format(
                                "ERROR message from {0} pushing pushing {1}TBLOPTIONS table\n" +
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

                    IdOptionHashSet.Add(idoption);
                }
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
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Count + ParsedData.OptionRecords.Count)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }
    }
}