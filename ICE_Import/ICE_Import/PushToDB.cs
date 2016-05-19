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
        int globalCount = 0;

        private async Task PushDataToDBStoredProcedures(CancellationToken ct)
        {
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBarLoad.Maximum += ParsedData.OptionRecords.Length;
            }
            Utilities utilites = new Utilities();
            EnableDisable(true);
            DateTime start = DateTime.Now;
            await Task.Run(() => PushFutures(globalCount, utilites, ct), ct);
            SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));
            await Task.Run(() => PushDailyFutures(globalCount, utilites, ct), ct);
            SetLogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

            if (!ParsedData.FuturesOnly)
            {
                await Task.Run(() => PushOptions(globalCount, utilites, ct), ct);
            }
            DateTime stop = DateTime.Now;
            SetLogMessage("Pushed to DB: " + globalCount.ToString() + " entities");
            SetLogMessage("Time with Stored Procedures - " + (stop - start));

            EnableDisable(false);
        }

        void PushFutures(int globalCount,  Utilities utilites, CancellationToken ct)
        {
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                int idinstrument = 36;
                char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());
                string contractName = utilites.generateCQGSymbolFromSpan('F', "CCE", monthchar, future.StripName.Year);
                string log = String.Empty;
                try
                {
                    Context.sp_updateContractTblFromSpanUpsert(contractName, 
                                                               monthchar, 
                                                               future.StripName.Month, 
                                                               future.StripName.Year, 
                                                               idinstrument, 
                                                               future.Date, 
                                                               contractName);

                    tblcontract contract = Context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    Context.sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract, 
                                                                               future.StripName, 
                                                                               (future.SettlementPrice != null)? future.SettlementPrice : 0);
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
                    globalCount ++;
                    if (globalCount == ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entities to {1} TBLCONTRACT table", globalCount, locRem);
                    }
                    Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                    log = String.Empty;
                }
            }
        }

        void PushDailyFutures(int globalCount, Utilities utilites, CancellationToken ct)
        {
            globalCount += ParsedData.FutureRecords.Length;
            foreach (EOD_Futures_578 future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                char monthchar = Convert.ToChar(((MonthCodes)future.StripName.Month).ToString());
                string log = String.Empty;
                try
                {

                    tblcontract contract = Context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    Context.sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract,
                                                                               future.StripName,
                                                                               (future.SettlementPrice != null) ? future.SettlementPrice : 0);
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
                        log += string.Format("Pushed {0} entities to {1} TBLDAILYCONTRACTSETTLEMENT table", globalCount - ParsedData.FutureRecords.Length, locRem);
                    }
                    Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                    log = String.Empty;
                }
            }

        }

        void PushOptions(int globalCount, Utilities utilites, CancellationToken ct)
        {
            globalCount += ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length;

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

                    long idContract = Context.tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;


                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 

                    double impliedvol = OptionCalcs.calculateOptionVolatility(option.OptionType,
                                                                            1.56,
                                                                            (option.StrikePrice != null) ? (double)option.StrikePrice : 0,
                                                                            0.5,
                                                                            0.08,
                                                                            (option.SettlementPrice != null) ? (double)option.SettlementPrice : 0);

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                    Context.sp_updateOrInsertTbloptionsInfoAndDataUpsert(optionName,
                                                                         monthchar,
                                                                         option.StripName.Month,
                                                                         option.StripName.Year,
                                                                         option.StrikePrice,
                                                                         option.OptionType,
                                                                         idinstrument,
                                                                         option.Date,
                                                                         idContract,
                                                                         optionName,
                                                                         option.StripName,
                                                                         option.StrikePrice,
                                                                         impliedvol,
                                                                         futureYear - expiranteYear
                                                                         );
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
                        log += string.Format("Pushed {0} entities to {1} TBLOPTIONS and TBLOPTIONDATAS tables", globalCount, locRem);
                    }
                    Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                    log = String.Empty;
                }
            }
        }
    }
}
