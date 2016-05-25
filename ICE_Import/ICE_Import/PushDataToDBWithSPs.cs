using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        /// <summary>
        /// Push all data to DB with stored procedures. Update either test or non-test tables.
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
                await Task.Run(() => PushFuturesToDBWithSP(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);

                if (!ParsedData.FuturesOnly)
                {
                    await Task.Run(() => PushOptionsToDBWithSP(ref globalCount, ct), ct);
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

            LogMessage(string.Format("Pushed to DB: {0} entries", globalCount));
        }

        /// <summary>
        /// Push futures to DB with stored procedures.
        /// </summary>
        void PushFuturesToDBWithSP(ref int globalCount, CancellationToken ct)
        {
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

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
                    Context.test_SPF(
                        contractname,
                        monthchar,
                        future.StripName.Month,
                        future.StripName.Year,
                        idinstrument,
                        future.Date,
                        contractname);

                    Context.test_SPDF(
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

                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 
                    double impliedvol = OptionCalcs.CalculateOptionVolatility(
                        option.OptionType,
                        1.56,
                        Utilities.NormalizePrice(option.StrikePrice),
                        0.5,
                        0.08,
                        Utilities.NormalizePrice(option.SettlementPrice));

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                    Context.test_SPO(
                        optionName,
                        monthchar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.SettlementPrice.GetValueOrDefault(),
                        option.OptionType,
                        idinstrument,
                        option.Date,
                        optionName);

                    Context.test_SPOD(
                        monthchar,
                        option.StripName.Year,
                        option.Date,
                        option.StrikePrice.GetValueOrDefault(),
                        impliedvol,
                        futureYear - expiranteYear);
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
                        log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
                    }
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }
        }
    }
}