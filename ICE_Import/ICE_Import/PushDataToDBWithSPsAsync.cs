using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Data.Linq;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        async Task PushDataToDBWithSPsAsync(CancellationToken ct)
        {
            var stripNameHashSet = new HashSet<DateTime>();

            const int packetSize = 30; // must be even

            progressBar.Maximum = ParsedData.FutureRecords.Length;
            if (!ParsedData.FuturesOnly)
            {
                progressBar.Maximum += ParsedData.OptionRecords.Length;
            }

            int globalCount = 0;

            var queries = new List<string>(packetSize);

            var sb = new StringBuilder();

            int idinstrument = 36;

            AsyncTaskListener.Init("Pushing of FUTURES data started");

            foreach (var future in ParsedData.FutureRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                bool newFuture = !stripNameHashSet.Contains(future.StripName);
                char monthchar = Utilities.MonthToMonthCode(future.StripName.Month);

                if (newFuture)
                {
                    string contractname = Utilities.GenerateCQGSymbolFromSpan(
                        'F',
                        "CCE",
                        monthchar,
                        future.StripName.Year);

                    #region Get expirationtime
                    DateTime expirationtime;
                    int key = future.StripName.Month + future.StripName.Year + idinstrument;
                    if (expirationtimeDictionary.ContainsKey(key))
                    {
                        expirationtime = expirationtimeDictionary[key];
                    }
                    else
                    {
                        expirationtime = GetExpirationTime(future.StripName.Year, future.StripName.Month, idinstrument);
                        int newKey = future.StripName.Month + future.StripName.Year + idinstrument;
                        expirationtimeDictionary.Add(newKey, expirationtime);
                    }
                    #endregion

                    sb.AppendFormat("cqgdb.{0}SPF ", storedProcPrefix);
                    Utilities.AppendHelper(sb, contractname);
                    Utilities.AppendHelper(sb, monthchar);
                    Utilities.AppendHelper(sb, future.StripName.Month);
                    Utilities.AppendHelper(sb, future.StripName.Year);
                    Utilities.AppendHelper(sb, idinstrument);
                    Utilities.AppendHelper(sb, expirationtime);
                    Utilities.AppendHelper(sb, contractname, true);

                    stripNameHashSet.Add(future.StripName);

                    queries.Add(sb.ToString());

                    sb.Clear();
                }

                sb.AppendFormat("cqgdb.{0}SPDF ", storedProcPrefix);
                Utilities.AppendHelper(sb, future.Date);
                Utilities.AppendHelper(sb, future.SettlementPrice.GetValueOrDefault());
                Utilities.AppendHelper(sb, monthchar);
                Utilities.AppendHelper(sb, future.StripName.Year);
                Utilities.AppendHelper(sb, future.Volume.GetValueOrDefault());
                Utilities.AppendHelper(sb, future.OpenInterest.GetValueOrDefault(), true);

                queries.Add(sb.ToString());

                sb.Clear();

                if (queries.Count == packetSize)
                {
                    await ConnectDBAndExecuteQueryAsyncWithTransaction(queries);
                }

                globalCount++;

                AsyncTaskListener.Update(globalCount);
            }

            if (queries.Count != 0)
            {
                await ConnectDBAndExecuteQueryAsyncWithTransaction(queries);
            }

            AsyncTaskListener.LogMessage("Pushing of FUTURES data complete");

            if (ParsedData.FuturesOnly)
            {
                return;
            }

            AsyncTaskListener.LogMessage("Pushing of OPTIONS data started");

            foreach (var option in ParsedData.OptionRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                
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

                #region Get expirationtime
                DateTime expirationtime;
                int key = option.StripName.Month + option.StripName.Year + idinstrument;
                if (expirationtimeDictionary.ContainsKey(key))
                {
                    expirationtime = expirationtimeDictionary[key];
                }
                else
                {
                    expirationtime = GetExpirationTime(option.StripName.Year, option.StripName.Month, idinstrument);
                    int newKey = option.StripName.Month + option.StripName.Year + idinstrument;
                    expirationtimeDictionary.Add(newKey, expirationtime);
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
                    riskFreeInterestRate,
                    Utilities.NormalizePrice(option.SettlementPrice),
                    tickSize);
                #endregion

                double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                double expirateYear = option.Date.Year + option.Date.Month * 0.0833333;

                sb.AppendFormat("cqgdb.{0}SPO ", storedProcPrefix);
                Utilities.AppendHelper(sb, optionName);
                Utilities.AppendHelper(sb, monthchar);
                Utilities.AppendHelper(sb, option.StripName.Month);
                Utilities.AppendHelper(sb, option.StripName.Year);
                Utilities.AppendHelper(sb, option.StrikePrice.GetValueOrDefault());
                Utilities.AppendHelper(sb, option.OptionType);
                Utilities.AppendHelper(sb, idinstrument);
                Utilities.AppendHelper(sb, expirationtime);
                Utilities.AppendHelper(sb, optionName, true);

                queries.Add(sb.ToString());

                sb.Clear();

                #region Get idoption
                long idoption = 0;

                if (cb_TestTables.Checked)
                {
                    var test_tbloptions = new List<test_tbloption>();
                    try
                    {
                        //var optlist = context.tbloptions.Where(item => item.optionname == optionName).ToList();
                        var test_optlist = context.test_tbloptions.Where(item => item.optionname == optionName).ToList();

                        foreach (var item in test_optlist)
                        {
                            test_tbloptions.Add(item);
                        }
                        int countContracts = test_tbloptions.Count;
                        if (countContracts > 0)
                        {
                            idoption = test_tbloptions[0].idoption;
                        }
                    }
                    catch (Exception ex)
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Length - ParsedData.FutureRecords.Length;
                        AsyncTaskListener.LogMessage(
                            string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                            "Can't read N: {2} from DB\n",
                            databaseName, tablesPrefix, erc));
                        AsyncTaskListener.LogMessage(ex.Message + "\n");
                        continue;
                    }
                }
                else
                {
                    var tbloptions = new List<tbloption>();
                    try
                    {
                        //var optlist = context.tbloptions.Where(item => item.optionname == optionName).ToList();
                        var optlist = context.tbloptions.Where(item => item.optionname == optionName).ToList();

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
                        AsyncTaskListener.LogMessage(
                            string.Format(
                            "ERROR message from {0} pushing pushing {1}TBLOPTIONS table \n" +
                            "Can't read N: {2} from DB\n",
                            databaseName, tablesPrefix, erc));
                        AsyncTaskListener.LogMessage(ex.Message + "\n");
                        continue;
                    }
                }
                #endregion

                sb.AppendFormat("cqgdb.{0}SPOD ", storedProcPrefix);
                Utilities.AppendHelper(sb, idoption);
                Utilities.AppendHelper(sb, option.Date);
                Utilities.AppendHelper(sb, option.SettlementPrice.GetValueOrDefault());
                Utilities.AppendHelper(sb, impliedvol);
                Utilities.AppendHelper(sb, futureYear - expirateYear, true);

                queries.Add(sb.ToString());

                sb.Clear();

                if (queries.Count == packetSize)
                {
                    await ConnectDBAndExecuteQueryAsyncWithTransaction(queries);
                }

                globalCount++;

                AsyncTaskListener.Update(globalCount);
            }

            if (queries.Count != 0)
            {
                await ConnectDBAndExecuteQueryAsyncWithTransaction(queries);
            }

            AsyncTaskListener.LogMessage("Pushing of OPTIONS data complete");
        }
        
        async Task ConnectDBAndExecuteQueryAsyncWithTransaction(List<string> queryStringToUpdate)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlTransaction trans = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand("", connection, trans))
                        {
                            command.CommandType = CommandType.Text;

                            command.CommandTimeout = 0;

                            foreach (var commandString in queryStringToUpdate)
                            {
                                command.CommandText = commandString;

                                try
                                {
                                    await command.ExecuteNonQueryAsync();
                                }
                                catch (Exception e)
                                {
                                    LogMessage(e.GetType().ToString());
                                    LogMessage(e.Message);
                                    LogMessage(commandString);
                                }
                            }
                        }
                        try
                        {
                            trans.Commit();
                        }
                        catch (Exception e)
                        {
                            LogMessage(e.Message);
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                LogMessage(e.Message);
            }
            finally
            {
                queryStringToUpdate.Clear();
            }
        }
    }
}
