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
            if (!ParsedData.FuturesOnly)
            {
                progressBarLoad.Maximum += ParsedData.OptionRecords.Length;
            }
            Utilities utilites = new Utilities();
            EnableDisable(true);
            DateTime start = DateTime.Now;
            await Task.Run(() => PushFutures(globalCount, utilites, ct), ct);
            LogMessage("Time with Stored Procedures - " + (DateTime.Now - start));
            await Task.Run(() => PushDailyFutures(globalCount, utilites, ct), ct);
            LogMessage("Time with Stored Procedures - " + (DateTime.Now - start));

            if (!ParsedData.FuturesOnly)
            {
                await Task.Run(() => PushOptions(globalCount, utilites, ct), ct);
            }
            DateTime stop = DateTime.Now;
            LogMessage("Pushed to DB: " + globalCount.ToString() + " entities");
            LogMessage("Time with Stored Procedures - " + (stop - start));

            EnableDisable(false);
        }

        /// <summary>
        /// Push method futures to db with stored procedures
        /// </summary>
        /// <param name="globalCount"></param>
        /// <param name="utilites"></param>
        /// <param name="ct"></param>
        void PushFutures(int globalCount, Utilities utilites, CancellationToken ct)
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
                    Context.test_sp_updateContractTblFromSpanUpsert(contractName, 
                                                               monthchar, 
                                                               future.StripName.Month, 
                                                               future.StripName.Year, 
                                                               idinstrument, 
                                                               future.Date, 
                                                               contractName);

                    test_tblcontract contract = Context.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    Context.test_sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract, 
                                                                                       future.StripName, 
                                                                                       (future.SettlementPrice != null)? future.SettlementPrice : 0);
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                }
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {2}TBLCONTRACT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
                finally
                {
                    globalCount ++;
                    if (globalCount == ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entities to {1} {2}TBLCONTRACT table", globalCount, DatabaseName, TablesPrefix);
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

                    test_tblcontract contract = Context.test_tblcontracts.Where(item => item.month == monthchar && item.year == future.StripName.Year).ToArray()[0];

                    Context.test_sp_updateOrInsertContractSettlementsFromSpanUpsert((int)contract.idcontract,
                                                                                       future.StripName,
                                                                                       (future.SettlementPrice != null) ? future.SettlementPrice : 0);
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                }
                catch (Exception ex)
                {
                    log += string.Format("ERROR message from {0} pushing {1}TBLDAILYCONTRACTSETTLEMENT table \n", DatabaseName, TablesPrefix);
                    log += ex.Message + "\n";
                }
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length)
                    {
                        log += string.Format("Pushed {0} entities to {1} {2}TBLDAILYCONTRACTSETTLEMENT table", globalCount - ParsedData.FutureRecords.Length, DatabaseName, TablesPrefix);
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

                    long idContract = Context.test_tblcontracts.Where(item => item.month == monthchar && item.year == option.StripName.Year).ToList()[0].idcontract;


                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - r(f) = 0.08, foreign risk-free interest rate in the U.S. is 8% per annum
                    // currentOptionPrice               - option.SettlementPrice 

                    double impliedvol = OptionCalcs.calculateOptionVolatility(
                        option.OptionType,
                        1.56,
                        (option.StrikePrice != null) ? (double)option.StrikePrice : 0,
                        0.5,
                        0.08,
                        (option.SettlementPrice != null) ? (double)option.SettlementPrice : 0);

                    double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                    double expiranteYear = option.Date.Year + option.Date.Month * 0.0833333;

                    Context.test_sp_updateOrInsertTbloptionsInfoAndDataUpsert(
                        optionName,
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
                        futureYear - expiranteYear);
                }
                catch (OperationCanceledException cancel)
                {
                    log += string.Format("Cancel message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n", DatabaseName, TablesPrefix);
                    log += cancel.Message + "\n";
                }
                catch (Exception ex)
                {
                    int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                    log += string.Format(
                        "ERROR message from {0} pushing {1}TBLOPTIONS and {1}TBLOPTIONDATAS tables\n" +
                        "Can't push entity N: {2}\n",
                        DatabaseName, TablesPrefix, erc);
                    log += ex.Message + "\n";
                    continue;
                }
                finally
                {
                    globalCount++;
                    if (globalCount == ParsedData.FutureRecords.Length + ParsedData.FutureRecords.Length + ParsedData.OptionRecords.Length)
                    {
                        log += string.Format("Pushed {0} entities to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
                    }
                    Invoke(new Action(() => ValuesFromTask(log, globalCount)));
                    log = String.Empty;
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
