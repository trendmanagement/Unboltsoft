using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        bool useOldRFI;
        double oldRFI;

        async Task PushDataToDB(CancellationToken ct)
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
                AsyncTaskListener.Init("Pushing of FUTURES data started");
                await Task.Run(() => PushContractsTable(ref globalCount, ct), ct);
                LogElapsedTime(DateTime.Now - start);
                AsyncTaskListener.LogMessage("Pushing of FUTURES data complete");

                if (!ParsedData.FuturesOnly)
                {
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data started");
                    await Task.Run(() => PushOptionsTable(ref globalCount, ct), ct);
                    LogElapsedTime(DateTime.Now - start);
                    AsyncTaskListener.LogMessage("Pushing of OPTIONS data complete");
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

        void PushContractsTable(ref int globalCount, CancellationToken ct)
        {
            StripNameHashSet = new HashSet<DateTime>();
            StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            // Create tables
            var tblContract = new DataTable();
            tblContract.Columns.Add("contractname", typeof(string));
            tblContract.Columns.Add("month", typeof(char));
            tblContract.Columns.Add("monthint", typeof(int));
            tblContract.Columns.Add("year", typeof(int));
            tblContract.Columns.Add("idinstrument", typeof(int));
            tblContract.Columns.Add("expirationdate", typeof(DateTime));
            tblContract.Columns.Add("cqgsymbol", typeof(string));

            var tblDailyContract = new DataTable();
            tblDailyContract.Columns.Add("idinstrument", typeof(int));
            tblDailyContract.Columns.Add("month", typeof(char));
            tblDailyContract.Columns.Add("year", typeof(int));
            tblDailyContract.Columns.Add("date", typeof(DateTime));
            tblDailyContract.Columns.Add("price", typeof(double));
            tblDailyContract.Columns.Add("volume", typeof(double));
            tblDailyContract.Columns.Add("openinterest", typeof(double));

            foreach (EOD_Future future in ParsedData.FutureRecords)
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
                if (newFuture)
                {
                    DateTime expirationDate = TMLDBReader.GetExpirationDate(
                        "future",
                        (long)IdInstrument,
                        future.StripName,
                        ref log);

                    tblContract.Rows.Add(
                        contractname,
                        monthChar,
                        future.StripName.Month,
                        future.StripName.Year,
                        (int)IdInstrument,
                        expirationDate,
                        contractname);

                    StripNameHashSet.Add(future.StripName);
                }

                tblDailyContract.Rows.Add(
                    IdInstrument,
                    monthChar,
                    future.StripName.Year,
                    future.Date,
                    future.SettlementPrice.GetValueOrDefault(),
                    (long)future.Volume.GetValueOrDefault(),
                    (long)future.OpenInterest.GetValueOrDefault());

                StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));

                globalCount++;
                AsyncTaskListener.Update(globalCount, log);
                log = string.Empty;
            }

            #region Pushing commands
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                PushOneTable(tblContract, "contract", "SPFTable", connection);
                PushOneTable(tblDailyContract, "dailycontract", "SPDFTable", connection);
            }
            #endregion

            AsyncTaskListener.LogMessageFormat("Pushed {0} entries to {1} {2}TBLCONTRACT table", globalCount, DatabaseName, TablesPrefix);

        }

        void PushOptionsTable(ref int globalCount, CancellationToken ct)
        {
            bool newOption;

            OptionNameHashSet = new HashSet<string>();

            OptionDataList = new List<Tuple<string, DateTime, double>>();

            // Create tables
            var tblOptions = new DataTable();
            tblOptions.Columns.Add("monthforfuture", typeof(int));
            tblOptions.Columns.Add("yearforfuture", typeof(int));
            tblOptions.Columns.Add("optionname", typeof(string));
            tblOptions.Columns.Add("optionmonth", typeof(char));
            tblOptions.Columns.Add("optionmonthint", typeof(int));
            tblOptions.Columns.Add("optionyear", typeof(int));
            tblOptions.Columns.Add("strikeprice", typeof(decimal));
            tblOptions.Columns.Add("callorput", typeof(char));
            tblOptions.Columns.Add("idinstrument", typeof(int));
            tblOptions.Columns.Add("expirationdate", typeof(DateTime));
            tblOptions.Columns.Add("cqgsymbol", typeof(string));

            var tblOptionDatas = new DataTable();
            tblOptionDatas.Columns.Add("optionname", typeof(string));
            tblOptionDatas.Columns.Add("datetime", typeof(DateTime));
            tblOptionDatas.Columns.Add("price", typeof(double));
            tblOptionDatas.Columns.Add("impliedvol", typeof(double));
            tblOptionDatas.Columns.Add("timetoexpinyears", typeof(double));

            string log = string.Empty;
            foreach (var option in ParsedData.OptionsRecordsSelected)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    char monthChar = Utilities.MonthToMonthCode(option.Option.StripName.Month);

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.Option.OptionType,
                        CqgSymbol,
                        monthChar,
                        option.Option.StripName.Year,
                        (double)option.Option.StrikePrice.GetValueOrDefault() * (double)ParsedData.NormalizeConst);

                    newOption = !OptionNameHashSet.Contains(optionName);

                    if (newOption)
                    {
                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "option",
                            (long)IdInstrument,
                            option.Option.StripName,
                            ref log);

                        tblOptions.Rows.Add(
                            option.FutureStripName.Month,
                            option.FutureStripName.Year,
                            optionName,
                            monthChar,
                            option.Option.StripName.Month,
                            option.Option.StripName.Year,
                            option.Option.StrikePrice.GetValueOrDefault(),
                            option.Option.OptionType,
                            IdInstrument,
                            expirationDate,
                            optionName);

                        OptionNameHashSet.Add(optionName);
                    }

                    #region Implied Volatility
                    // callPutFlag                      - tableOption.callorput
                    // S - stock price                  - 1.56
                    // X - strike price of option       - option.StrikePrice
                    // T - time to expiration in years  - 0.5
                    // r - risk-free interest rate      - from table tbloptioninputdata
                    // currentOptionPrice               - option.SettlementPrice 
                    // tickSize                         - from table tblinstruments (secondaryoptionticksize or optionticksize)

                    double riskFreeInterestRate = double.NaN;

                    try
                    {
                        riskFreeInterestRate = RiskFreeInterestRates.Find(item => item.optioninputdatetime == option.Option.Date).optioninputclose;
                        if (oldRFI != double.NaN)
                        {
                            oldRFI = riskFreeInterestRate;
                        }
                        else
                        {
                            oldRFI = 0;
                        }
                    }
                    catch (Exception)
                    {
                        if (!useOldRFI)
                        {
                            useOldRFI = true;
                        }
                        else
                        {
                            riskFreeInterestRate = oldRFI;
                        }
                    }

                    if (riskFreeInterestRate != double.NaN)
                    {
                        double impliedvol = OptionCalcs.CalculateOptionVolatilityNR(
                        option.Option.OptionType,
                        1.56,
                        Utilities.NormalizePrice(option.Option.StrikePrice),
                        0.5,
                        riskFreeInterestRate,
                        Utilities.NormalizePrice(option.Option.SettlementPrice),
                        (double)TickSize);

                        if (object.ReferenceEquals(impliedvol, null) || double.IsNaN(impliedvol) || double.IsInfinity(impliedvol))
                        {
                            impliedvol = 0;
                        }

                        #endregion

                        double futureYear = option.Option.StripName.Year + option.Option.StripName.Month / 12.0;
                        double expirateYear = option.Option.Date.Year + option.Option.Date.Month / 12.0;

                        tblOptionDatas.Rows.Add(
                            optionName,
                            option.Option.Date,
                            option.Option.SettlementPrice.GetValueOrDefault(),
                            impliedvol,
                            futureYear - expirateYear);

                        OptionDataList.Add(Tuple.Create(optionName, option.Option.Date, option.Option.SettlementPrice.GetValueOrDefault()));
                    }
                    else
                    {
                        int erc = globalCount - ParsedData.FutureRecords.Count - ParsedData.FutureRecords.Count;
                        AsyncTaskListener.LogMessageFormat("Cant find riskFreeInterestRate for item #{0}", erc);
                    }
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
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }

            #region Pushing commands
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                PushOneTable(tblOptions, "option", "SPOTable", connection);
                PushOneTable(tblOptionDatas, "optiondata", "SPODTable", connection);
            }
            #endregion

            AsyncTaskListener.LogMessageFormat("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
        }

        void PushOneTable(DataTable table, string tableType, string spNameRoot, SqlConnection connection)
        {
            string spNamePrefix = cb_TestTables.Checked ? "[cqgdb].test_" : "[cqgdb].";
            string spName = spNamePrefix + spNameRoot;

            using (SqlCommand cmd = new SqlCommand(spName, connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@" + tableType, table);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    AsyncTaskListener.LogMessage(ex.Message);
                }
            }
        }

        void DropTempTables()
        {
            string exeption2 = "Cannot drop the table 'tempOptionData', because it does not exist or you do not have permission.";
            string exeption3 = "Cannot drop the table 'temp', because it does not exist or you do not have permission.";
            string exeption1 = exeption3 + "\r\n" + exeption2;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "DROP TABLE temp; DROP TABLE tempOptionData;";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message == exeption1)
                        {
                            AsyncTaskListener.LogMessage("No tables to drop");
                        }
                        else if (ex.Message == exeption2)
                        {
                            AsyncTaskListener.LogMessage("Dropped 'temp' table");
                        }
                        else if (ex.Message == exeption3)
                        {
                            AsyncTaskListener.LogMessage("Dropped 'tempOptionData' table");
                        }
                        else
                        {
                            AsyncTaskListener.LogMessage(ex.Message);
                        }
                    }
                }
            }
        }
    }
}