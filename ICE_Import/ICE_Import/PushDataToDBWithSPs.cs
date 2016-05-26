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
        int spGlobalCount = 0;

        /// <summary>
        /// Push all data to DB with stored procedures. Update either test or non-test tables.
        /// </summary>
        async Task PushDataToDBWithSPs(CancellationToken ct)
        {
            if (DatabaseName != "TMLDB")
            {
                remoteContext = new DataClassesTMLDBDataContext(remoteConnectionStringPatternTMBLDB);
            }
            progressBar.Minimum = 0;
            progressBar.Maximum = ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Length;
            }

            DateTime start = DateTime.Now;

            try
            {
                await Task.Run(() => PushFuturesToDBWithSP(spGlobalCount, ct), ct);
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

                char monthchar = Utilities.MonthToMonthCode(future.StripName.Month);

                string contractname = Utilities.GenerateCQGSymbolFromSpan(
                    'F',
                    "CCE",
                    monthchar,
                    future.StripName.Year);

                string log = string.Empty;
                try
                {
                    if(DatabaseName == "TMLDB")
                    {
                        Context.test_SPF_Mod(
                            contractname,
                            monthchar,
                            future.StripName.Month,
                            future.StripName.Year,
                            idinstrument,
                            contractname);
                    }
                    else
                    {
                        DateTime expirationtime = new DateTime();
                        try
                        {
                            expirationtime = remoteContext.tblcontractexpirations.Where(item =>
                                item.optionmonthint == future.StripName.Month 
                                && item.optionyear == future.StripName.Year 
                                && item.idinstrument == idinstrument).ToArray()[0].expirationdate;
                        }
                        catch(Exception ex)
                        {
                            log += ex.Message;
                        }

                        Context.test_SPF(
                            contractname,
                            monthchar,
                            future.StripName.Month,
                            future.StripName.Year,
                            idinstrument,
                            expirationtime,
                            contractname);
                    }

                    Context.test_SPDF(
                        future.Date,
                        future.SettlementPrice.GetValueOrDefault(),
                        monthchar,
                        future.StripName.Year,
                        (long)future.Volume.GetValueOrDefault(),
                        (long)future.OpenInterest.GetValueOrDefault());

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
                    AsyncTaskListener.Update(spGlobalCount, log);
                    log = string.Empty;
                }
            }
        }

        /// <summary>
        /// Push options and optionsdata to DB with stored procedures.
        /// </summary>
        void PushOptionsToDBWithSP(int spGlobalCount, CancellationToken ct)
        {
            spGlobalCount += ParsedData.FutureRecords.Length;

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

                    #region Implied volume
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
                        Utilities.NormalizePrice(option.SettlementPrice.GetValueOrDefault()),
                        tickSize);
                    #endregion

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                    Context.test_SPO_Mod(
                        optionName,
                        monthchar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.SettlementPrice.GetValueOrDefault(),
                        option.OptionType,
                        idinstrument,
                        optionName);

                    Context.test_SPOD(
                        monthchar,
                        option.StripName.Year,
                        option.Date,
                        option.StrikePrice.GetValueOrDefault(),
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
                    AsyncTaskListener.Update(spGlobalCount, log);
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
            catch(Exception ex)
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