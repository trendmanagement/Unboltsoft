using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        int globalCount = 0;

        /// <summary>
        /// Colling push data methods to db with stored procedures with TestOF model
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task PushDataToDBStoredProcedures(CancellationToken ct)
        {
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length;
            if (!ParsedData.justFuture)
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

            if (!ParsedData.justFuture)
            {
                await Task.Run(() => PushOptions(globalCount, utilites, ct), ct);
            }
            DateTime stop = DateTime.Now;
            SetLogMessage("Pushed to DB: " + globalCount.ToString() + " entities");
            SetLogMessage("Time with Stored Procedures - " + (stop - start));

            EnableDisable(false);
        }

        /// <summary>
        /// Push method futures to db with stored procedures
        /// </summary>
        /// <param name="globalCount"></param>
        /// <param name="utilites"></param>
        /// <param name="ct"></param>
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
                    contextTest.test_sp_updateContractTblFromSpanUpsert(contractName, 
                                                                       monthchar, 
                                                                       future.StripName.Month, 
                                                                       future.StripName.Year, 
                                                                       idinstrument, 
                                                                       future.Date, 
                                                                       contractName);

                    test_tblcontract contract = contextTest.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    contextTest.test_sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract,
                                                                               future.StripName,
                                                                               (future.SettlementPrice != null) ? future.SettlementPrice : 0);
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

        /// <summary>
        /// Push method dailyfutures to db with stored procedures
        /// </summary>
        /// <param name="globalCount"></param>
        /// <param name="utilites"></param>
        /// <param name="ct"></param>
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

                    test_tblcontract contract = contextTest.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    contextTest.test_sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract,
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

        /// <summary>
        /// Push method options and options data to db with stored procedures
        /// </summary>
        /// <param name="globalCount"></param>
        /// <param name="utilites"></param>
        /// <param name="ct"></param>
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

                    long idContract = context.tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;


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

                    contextTest.test_sp_updateOrInsertTbloptionsInfoAndDataUpsert(optionName,
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

        /// <summary>
        /// Push data with OF model
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task PushDataToDB(CancellationToken ct)
        {
            int count = 0;
            int globalCount = 0;
            int number;
            int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //int currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length;
            if (isLocal)
            {
                progressBarLoad.Maximum += ParsedData.FutureRecords.Length + ((!ParsedData.justFuture)? ParsedData.OptionRecords.Length : 0);
            }
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
                                    List<tblcontract> tblcontracts = new List<tblcontract>();
                                    foreach(tblcontract item in context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToList())
                                    {
                                        tblcontracts.Add(item);
                                    }
                                    int countContracts = tblcontracts.Count;
                                    if(countContracts != 0)
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

                                var tableFuture = new tblcontract
                                {
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
                                context.tblcontracts.InsertOnSubmit(tableFuture);
                                context.SubmitChanges();
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
                            tblcontract contract = context.tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                            #region Find data in DB like pushed
                            try
                            {
                                List<tbldailycontractsettlement> tdcs = new List<tbldailycontractsettlement>();
                                foreach(tbldailycontractsettlement item in context.tbldailycontractsettlements.Where(item => item.idcontract == contract.idcontract && item.date == future.Date).ToList())
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

                            tbldailycontractsettlement tableDCS = new tbldailycontractsettlement
                            {
                                //idcontract must generete by DB
                                idcontract = contract.idcontract,
                                date = future.Date,
                                settlement = (future.SettlementPrice != null) ? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            context.tbldailycontractsettlements.InsertOnSubmit(tableDCS);
                            context.SubmitChanges();
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

                if (!ParsedData.justFuture)
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

                                tbloption TO = new tbloption();

                                #region Find data in DB like pushed
                                List<tbloption> tbloptions = new List<tbloption>();
                                try
                                {

                                    List<tbloption> optlist = context.tbloptions.Where(item => item.optionmonth == monthchar && item.optionyear == option.StripName.Year && item.optionname == optionName).ToList();
                                    foreach (tbloption item in optlist)
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
                                        idContract = context.tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;
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

                                    tbloption tableOption = new tbloption
                                    {
                                        //idoption must generete by DB
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
                                    context.tbloptions.InsertOnSubmit(tableOption);
                                    context.SubmitChanges();

                                    #region Find id for fist time pushed option
                                    List<tbloption> tblopt = new List<tbloption>();
                                    try
                                    {
                                        foreach (tbloption item in context.tbloptions.Where(item => item == tableOption).ToList())
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
                                List<tbloptiondata> tdcs = new List<tbloptiondata>();
                                try
                                {
                                    foreach (tbloptiondata item in context.tbloptiondatas.Where(item => item.timetoexpinyears == (futureYear - expiranteYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList())
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

                                tbloptiondata tableOptionData = new tbloptiondata
                                {
                                    //idoptiondata must generate by DB
                                    idoption = TO.idoption,
                                    datetime = option.Date,
                                    price = (option.StrikePrice != null) ? (double)option.StrikePrice : 1,
                                    impliedvol = impliedvol,
                                    timetoexpinyears = futureYear - expiranteYear
                                };

                                context.tbloptiondatas.InsertOnSubmit(tableOptionData);
                                context.SubmitChanges();
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

        /// <summary>
        /// Push data with TestOF model
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task PushDataToDBTest(CancellationToken ct)
        {
            int count = 0;
            int globalCount = 0;
            //int number;
            //int percent = (int.TryParse((ParsedData.FutureRecords.Length / 100).ToString(), out number)) ? number : 0;
            //int currentPercent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length + ((!ParsedData.justFuture) ? ParsedData.OptionRecords.Length : 0);
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
                                    List<test_tblcontract> tblcontracts = new List<test_tblcontract>();
                                    foreach (test_tblcontract item in contextTest.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToList())
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

                                var tableFuture = new test_tblcontract
                                {
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
                                contextTest.test_tblcontracts.InsertOnSubmit(tableFuture);
                                contextTest.SubmitChanges();
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
                            test_tblcontract contract = contextTest.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                            #region Find data in DB like pushed
                            try
                            {
                                List<test_tbldailycontractsettlement> tdcs = new List<test_tbldailycontractsettlement>();
                                foreach (test_tbldailycontractsettlement item in contextTest.test_tbldailycontractsettlements.Where(item => item.idcontract == contract.idcontract && item.date == future.Date).ToList())
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

                            test_tbldailycontractsettlement tableDCS = new test_tbldailycontractsettlement
                            {
                                //idcontract must generete by DB
                                idcontract = contract.idcontract,
                                date = future.Date,
                                settlement = (future.SettlementPrice != null) ? (double)future.SettlementPrice : 0,
                                volume = (future.Volume != null) ? (long)future.Volume : 0,
                                openinterest = (future.OpenInterest != null) ? (long)future.OpenInterest : 0
                            };

                            contextTest.test_tbldailycontractsettlements.InsertOnSubmit(tableDCS);
                            contextTest.SubmitChanges();
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

                if (!ParsedData.justFuture)
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

                                test_tbloption TO = new test_tbloption();

                                #region Find data in DB like pushed
                                List<test_tbloption> tbloptions = new List<test_tbloption>();
                                try
                                {

                                    List<test_tbloption> optlist = contextTest.test_tbloptions.Where(item => item.optionmonth == monthchar && item.optionyear == option.StripName.Year && item.optionname == optionName).ToList();
                                    foreach (test_tbloption item in optlist)
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
                                        idContract = contextTest.test_tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;
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

                                    test_tbloption tableOption = new test_tbloption
                                    {
                                        //idoption must generete by DB
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
                                    contextTest.test_tbloptions.InsertOnSubmit(tableOption);
                                    contextTest.SubmitChanges();

                                    #region Find id for fist time pushed option
                                    List<test_tbloption> tblopt = new List<test_tbloption>();
                                    try
                                    {
                                        foreach (test_tbloption item in contextTest.test_tbloptions.Where(item => item == tableOption).ToList())
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
                                List<test_tbloptiondata> tdcs = new List<test_tbloptiondata>();
                                try
                                {
                                    foreach (test_tbloptiondata item in contextTest.test_tbloptiondatas.Where(item => item.timetoexpinyears == (futureYear - expiranteYear) && item.datetime == option.Date && item.idoption == TO.idoption).ToList())
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

                                test_tbloptiondata tableOptionData = new test_tbloptiondata
                                {
                                    //idoptiondata must generate by DB
                                    idoption = TO.idoption,
                                    datetime = option.Date,
                                    price = (option.StrikePrice != null) ? (double)option.StrikePrice : 1,
                                    impliedvol = impliedvol,
                                    timetoexpinyears = futureYear - expiranteYear
                                };

                                contextTest.test_tbloptiondatas.InsertOnSubmit(tableOptionData);
                                contextTest.SubmitChanges();
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

        /// <summary>
        /// Example async method
        /// </summary>
        /// <param name="queryStringToUpdate"></param>
        /// <param name="connString1BuilderInternal"></param>
        /// <returns></returns>
        public static async Task ConnectDBAndExecuteQueryAsyncWithTransaction(List<string> queryStringToUpdate, SqlConnectionStringBuilder connString1BuilderInternal)//, SqlConnection connection)
        {
            try
            {
                //http://executeautomation.com/blog/using-async-and-await-for-asynchronous-operation-also-with-multi-threading/
                //http://stackoverflow.com/questions/200986/c-sql-how-to-execute-a-batch-of-storedprocedure
                //http://stackoverflow.com/questions/17008902/sending-several-sql-commands-in-a-single-transaction

                //string sqlQuery = "insert into tblMasterLookup Values (" + ThreadNumber + ",'Test','2.0','Myapplication',GetDate()) waitfor delay '00:00:30'";
                //foreach (var commandString in queryStringToUpdate)

                //while()
                {
                    //string connectionString = @"Server=.\SQLEXPRESS;Database=AUTODATA;Password=abc123;User ID=sa";
                    using (SqlConnection connection = new SqlConnection(connString1BuilderInternal.ToString()))
                    {
                        await connection.OpenAsync();

                        using (SqlTransaction trans = connection.BeginTransaction())
                        {

                            using (SqlCommand command = new SqlCommand("", connection, trans))
                            {
                                command.CommandType = CommandType.Text;
                                //IAsyncResult result = command.BeginExecuteNonQuery();
                                //Console.WriteLine("Command complete. Affected {0} rows.",
                                //command.EndExecuteNonQuery(result));

                                //DateTime currenttime = DateTime.Now;
                                //Console.WriteLine("Executed Thread.. " + Thread.CurrentThread.ManagedThreadId);
                                command.CommandTimeout = 0;

                                foreach (var commandString in queryStringToUpdate)
                                {
                                    command.CommandText = commandString;

                                    try
                                    {
                                        await command.ExecuteNonQueryAsync();
                                        //command.ExecuteNonQuery();
                                    }
                                    catch (Exception)
                                    {
                                        //trans.Rollback();
                                        Console.WriteLine(commandString);
                                    }
                                    //trans.Commit();

                                }

                                //long elapsedTicks = DateTime.Now.Ticks - currenttime.Ticks;
                                //TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                                //TSErrorCatch.debugWriteOut("TEST 2 seconds " + elapsedSpan.TotalSeconds);
                            }
                            try
                            {
                                trans.Commit();
                            }
                            catch (Exception)
                            {
                                //trans.Rollback();
                                Console.WriteLine("didn't work Transaction "); ;
                            }

                        }

                        connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
