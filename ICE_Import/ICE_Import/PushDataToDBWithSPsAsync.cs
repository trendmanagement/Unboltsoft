using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        async Task PushDataToDBWithSPsAsync(CancellationToken ct)
        {
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

                char monthchar = Utilities.MonthToMonthCode(future.StripName.Month);

                string contractname = Utilities.GenerateCQGSymbolFromSpan(
                    'F',
                    "CCE",
                    monthchar,
                    future.StripName.Year);

                sb.Append("cqgdb.test_SPF ");
                Utilities.AppendHelper(sb, contractname);
                Utilities.AppendHelper(sb, monthchar);
                Utilities.AppendHelper(sb, future.StripName.Month);
                Utilities.AppendHelper(sb, future.StripName.Year);
                Utilities.AppendHelper(sb, idinstrument);
                Utilities.AppendHelper(sb, future.Date);
                Utilities.AppendHelper(sb, contractname, true);

                queries.Add(sb.ToString());

                sb.Clear();

                sb.Append("cqgdb.test_SPDF ");
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
                    Utilities.NormalizePrice(option.SettlementPrice.GetValueOrDefault()),
                    tickSize);
                #endregion

                double futureYear = option.StripName.Year + option.StripName.Month * 0.0833333;
                double expirateYear = option.Date.Year + option.Date.Month * 0.0833333;

                sb.Append("cqgdb.test_SPO ");
                Utilities.AppendHelper(sb, optionName);
                Utilities.AppendHelper(sb, monthchar);
                Utilities.AppendHelper(sb, option.StripName.Month);
                Utilities.AppendHelper(sb, option.StripName.Year);
                Utilities.AppendHelper(sb, option.SettlementPrice.GetValueOrDefault());
                Utilities.AppendHelper(sb, option.OptionType);
                Utilities.AppendHelper(sb, idinstrument);
                Utilities.AppendHelper(sb, option.Date);
                Utilities.AppendHelper(sb, optionName, true);

                queries.Add(sb.ToString());

                sb.Clear();

                sb.Append("cqgdb.test_SPOD ");
                Utilities.AppendHelper(sb, monthchar);
                Utilities.AppendHelper(sb, option.StripName.Year);
                Utilities.AppendHelper(sb, option.Date);
                Utilities.AppendHelper(sb, option.StrikePrice.GetValueOrDefault());
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
                using (var connection = new SqlConnection(ConnectionString))
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
