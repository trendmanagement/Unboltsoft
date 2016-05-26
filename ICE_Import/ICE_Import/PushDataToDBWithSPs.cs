using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        /// <summary>
        /// Push all data to DB with stored procedures. Update either test or non-test tables.
        /// </summary>
        async Task PushDataToDBWithSPs(CancellationToken ct)
        {
            if (DatabaseName != "TMLDB")
            {
                remoteContext = new DataClassesTMLDBDataContext(remoteConnectionStringPatternTMLDB);
            }
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
                    if (DatabaseName == "TMLDB")
                    {
                        StoredProcsSwitch.SPF_Mod(
                            contractname,
                            monthchar,
                            future.StripName.Month,
                            future.StripName.Year,
                            idinstrument,
                            contractname);
                    }
                    else
                    {
                        var records = remoteContext.tblcontractexpirations.Where(
                            item =>
                            item.optionmonthint == future.StripName.Month &&
                            item.optionyear == future.StripName.Year &&
                            item.idinstrument == idinstrument).ToArray();

                        DateTime expirationtime;
                        if (records.Length == 0)
                        {
                            log += string.Format("Failed to find expiration date for future with month = {0}, year = {1}, idinstrument = {2} in tblcontractexpirations", future.StripName.Month, future.StripName.Year, idinstrument);
                            expirationtime = new DateTime();
                        }
                        else
                        {
                            expirationtime = records[0].expirationdate;
                        }

                        StoredProcsSwitch.SPF(
                            contractname,
                            monthchar,
                            future.StripName.Month,
                            future.StripName.Year,
                            idinstrument,
                            expirationtime,
                            contractname);
                    }

                    StoredProcsSwitch.SPDF(
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
                        r,
                        Utilities.NormalizePrice(option.SettlementPrice),
                        tickSize);
                    #endregion

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expirateYear = option.Date.Year + option.Date.Month * 0.0833333;

                    StoredProcsSwitch.SPO_Mod(
                        optionName,
                        monthchar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.SettlementPrice.GetValueOrDefault(),
                        option.OptionType,
                        idinstrument,
                        optionName);

                    StoredProcsSwitch.SPOD(
                        monthchar,
                        option.StripName.Year,
                        option.Date,
                        option.StrikePrice.GetValueOrDefault(),
                        impliedvol,
                        futureYear - expirateYear);
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

        private double R(DataClassesTMLDBDataContext context)
        {
            double optioninputclose = 0;
            try
            {
                var idoptioninputsymbol = context.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == 1).ToArray()[0].idoptioninputsymbol;
                tbloptioninputdata[] tbloptioninputdatas = context.tbloptioninputdatas.Where(item =>
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
                var idoptioninputsymbol2 = context.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == OPTION_INPUT_TYPE_RISK_FREE_RATE).ToArray()[0].idoptioninputsymbol;

                optioninputclose = context.tbloptioninputdatas.Where(item =>
                    item.idoptioninputsymbol == idoptioninputsymbol2
                        && item.optioninputdatetime == optioninputdatetime).ToArray()[0].optioninputclose;

            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
            finally
            {
                LogMessage(string.Format("Risk = {0}", optioninputclose));
            }

            return optioninputclose;
        }

        private double TickSize(DataClassesTMLDBDataContext context)
        {
            try
            {
                var secondaryoptionticksize = context.tblinstruments.Where(item => item.idinstrument == 36).ToArray()[0].secondaryoptionticksize;

                if (secondaryoptionticksize > 0)
                {
                    tickSize = secondaryoptionticksize;
                }
                else
                {
                    tickSize = context.tblinstruments.Where(item => item.idinstrument == 36).ToArray()[0].optionticksize;
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
            finally
            {
                LogMessage(string.Format("Tick size = {0}", tickSize));
            }

            return tickSize;
        }

    }
}