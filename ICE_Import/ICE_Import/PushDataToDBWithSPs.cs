using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        int spGlobalCount = 0;

        /// <summary>
        /// Push all data to DB with stored procedures. Update either test or non-test tables.
        /// </summary>
        async Task PushDataToDBWithSPs(CancellationToken ct)
        {
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = 2 * ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBarLoad.Maximum += ParsedData.OptionRecords.Length;
            }

            DateTime start = DateTime.Now;

            try
            {
                await Task.Run(() => PushFuturesToDBWithSP(spGlobalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                await Task.Run(() => PushDailyFuturesToDBWithSP(spGlobalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                if (!ParsedData.FuturesOnly)
                {
                    await Task.Run(() => PushOptionsToDBWithSP(spGlobalCount, ct), ct);
                    LogElapsedTime(DateTime.Now - start);
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

            LogMessage(string.Format("Pushed to DB: {0} entries", spGlobalCount));
        }

        /// <summary>
        /// Push futures to DB with stored procedures.
        /// </summary>
        void PushFuturesToDBWithSP(int spGlobalCount, CancellationToken ct)
        {
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                int idinstrument = 36;

                string contractname = Utilities.GenerateCQGSymbolFromSpan(
                    'F',
                    "CCE",
                    future.Date.Month,
                    future.Date.Year);

                string log = string.Empty;
                try
                {
                    char monthchar = Utilities.MonthToMonthCode(future.Date.Month);

                    StoredProcsSwitch.sp_updateContractTblFromSpanUpsert(
                        contractname,
                        monthchar,
                        future.Date.Month,
                        future.Date.Year,
                        idinstrument,
                        future.StripName,
                        contractname);
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
                    spGlobalCount++;
                    if (spGlobalCount == ParsedData.FutureRecords.Length)
                    {
                        log += string.Format(
                            "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                            spGlobalCount, DatabaseName, TablesPrefix);
                    }
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, spGlobalCount);
                    log = string.Empty;
                }
            }
        }

        /// <summary>
        /// Push dailyfutures to DB with stored procedures.
        /// </summary>
        void PushDailyFuturesToDBWithSP(int spGlobalCount, CancellationToken ct)
        {
            spGlobalCount += ParsedData.FutureRecords.Length;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                string log = string.Empty;
                try
                {
                    int idcontract = GetIdContract1(future.StripName);

                    StoredProcsSwitch.sp_updateOrInsertContractSettlementsFromSpanUpsert(
                        idcontract,
                        future.Date,
                        future.SettlementPrice.GetValueOrDefault());
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
                    spGlobalCount++;
                    if (spGlobalCount == 2 * ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", spGlobalCount - ParsedData.FutureRecords.Length, DatabaseName, TablesPrefix);
                    }
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, spGlobalCount);
                    log = string.Empty;
                }
            }
        }

        /// <summary>
        /// Push options and optionsdata to DB with stored procedures.
        /// </summary>
        void PushOptionsToDBWithSP(int spGlobalCount, CancellationToken ct)
        {
            spGlobalCount += 2 * ParsedData.FutureRecords.Length;

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

                    char monthchar = Utilities.MonthToMonthCode(option.Date.Month);
                    long idcontract = GetIdContract2(monthchar, option.StripName.Year);

                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 
                    double impliedvol = OptionCalcs.CalculateOptionVolatility(
                        option.OptionType,
                        1.56,
                        option.StrikePrice.GetValueOrDefault(),
                        0.5,
                        0.08,
                        option.SettlementPrice.GetValueOrDefault());

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                    StoredProcsSwitch.sp_updateOrInsertTbloptionsInfoAndDataUpsert(
                        optionName,
                        monthchar,
                        option.Date.Month,
                        option.Date.Year,
                        option.StrikePrice,
                        option.OptionType,
                        idinstrument,
                        option.Date,
                        idcontract,
                        optionName,
                        option.StripName,
                        option.StrikePrice,
                        impliedvol,
                        futureYear - expiranteYear);
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
                    int erc = spGlobalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
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
                    spGlobalCount++;
                    if (spGlobalCount == 2 * ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                    {
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", spGlobalCount, DatabaseName, TablesPrefix);
                    }
                    UpdateTextBoxAndProgressBarFromAsyncTask(log, spGlobalCount);
                    log = string.Empty;
                }
            }
        }

        int GetIdContract1(DateTime futureStripName)
        {
            if (IsTestTables)
            {
                return (int)Context.test_tblcontracts.Where(item => item.expirationdate == futureStripName).First().idcontract;
            }
            else
            {
                return (int)Context.tblcontracts.Where(item => item.expirationdate == futureStripName).First().idcontract;
            }
        }

        long GetIdContract2(char monthchar, int optionStripNameYear)
        {
            if (IsTestTables)
            {
                return Context.test_tblcontracts.Where(item => item.month == monthchar && item.year == optionStripNameYear).First().idcontract;
            }
            else
            {
                return Context.tblcontracts.Where(item => item.month == monthchar && item.year == optionStripNameYear).First().idcontract;
            }
        }
    }
}