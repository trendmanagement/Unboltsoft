using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        void PushContractsTable(ref int globalCount, CancellationToken ct)
        {
            StripNameHashSet = new HashSet<DateTime>();
            StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            //Create tables
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
            tblDailyContract.Columns.Add("price", typeof(float));
            tblDailyContract.Columns.Add("volume", typeof(float));
            tblDailyContract.Columns.Add("openinterest", typeof(float));

            foreach (EOD_Futures future in ParsedData.FutureRecords)
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
                        IdInstrument,
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
                    (long)future.OpenInterest.GetValueOrDefault()
                    );

                StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));

                globalCount++;
                //if (globalCount == ParsedData.FutureRecords.Count)
                //{
                //    log += string.Format(
                //        "Pushed {0} entries to {1} {2}TBLCONTRACT table",
                //        globalCount, DatabaseName, TablesPrefix);
                //}
                AsyncTaskListener.Update(globalCount, log);
                log = string.Empty;
            }

            #region Pushing commands
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string SPFName = (!cb_TestTables.Checked) ? "[cqgdb].SPFTable" : "[cqgdb].test_SPFTable";
                string SPDFName = (!cb_TestTables.Checked) ? "[cqgdb].SPDFTable" : "[cqgdb].test_SPDFTable";

                using (SqlCommand cmd = new SqlCommand(SPFName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@contract", tblContract);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        AsyncTaskListener.LogMessage(ex.Message);
                    }
                }
                using (SqlCommand cmd = new SqlCommand("[cqgdb].SPDFTable", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@dailycontract", tblDailyContract);

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
            #endregion

            AsyncTaskListener.LogMessageFormat("Pushed {0} entries to {1} {2}TBLCONTRACT table", globalCount, DatabaseName, TablesPrefix);

        }
        void PushOptionsTable(ref int globalCount, CancellationToken ct)
        {
            bool newOption;

            OptionNameHashSet = new HashSet<string>();

            OptionDataHashSet = new HashSet<Tuple<DateTime, DateTime>>();

            //Create tables
            var tblOptions = new DataTable();
            tblOptions.Columns.Add("optionname", typeof(string));
            tblOptions.Columns.Add("optionmonth", typeof(char));
            tblOptions.Columns.Add("optionmonthint", typeof(int));
            tblOptions.Columns.Add("optionyear", typeof(int));
            tblOptions.Columns.Add("strikeprice", typeof(float));
            tblOptions.Columns.Add("callorput", typeof(char));
            tblOptions.Columns.Add("idinstrument", typeof(int));
            tblOptions.Columns.Add("expirationdate", typeof(DateTime));
            tblOptions.Columns.Add("cqgsymbol", typeof(string));

            var tblOptionDatas = new DataTable();
            tblOptionDatas.Columns.Add("optionname", typeof(string));
            tblOptionDatas.Columns.Add("datetime", typeof(DateTime));
            tblOptionDatas.Columns.Add("price", typeof(float));
            tblOptionDatas.Columns.Add("impliedvol", typeof(float));
            tblOptionDatas.Columns.Add("timetoexpinyears", typeof(float));

            string log = string.Empty;
            foreach (EOD_Options option in ParsedData.OptionRecords)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    char monthChar = Utilities.MonthToMonthCode(option.StripName.Month);

                    string optionName = Utilities.GenerateOptionCQGSymbolFromSpan(
                        option.OptionType,
                        CqgSymbol,
                        monthChar,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        0,
                        0,
                        IdInstrument);

                    newOption = !OptionNameHashSet.Contains(optionName);

                    if (newOption)
                    {
                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "option",
                            IdInstrument,
                            option.StripName,
                            ref log);

                        tblOptions.Rows.Add(
                        optionName,
                        monthChar,
                        option.StripName.Month,
                        option.StripName.Year,
                        option.StrikePrice.GetValueOrDefault(),
                        option.OptionType,
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

                    double impliedvol = OptionCalcs.CalculateOptionVolatilityNR(
                        option.OptionType,
                        1.56,
                        Utilities.NormalizePrice(option.StrikePrice),
                        0.5,
                        RiskFreeInterestRate,
                        Utilities.NormalizePrice(option.SettlementPrice),
                        TickSize);
                    #endregion

                    double futureYear = option.StripName.Year + option.StripName.Month / 12.0;
                    double expirateYear = option.Date.Year + option.Date.Month / 12.0;

                    tblOptionDatas.Rows.Add(
                    optionName,
                    option.Date,
                    option.SettlementPrice.GetValueOrDefault(),
                    impliedvol,
                    futureYear - expirateYear);
                    OptionDataHashSet.Add(Tuple.Create(option.Date, option.StripName));
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
                    //if (globalCount == ParsedData.FutureRecords.Count + ParsedData.OptionRecords.Count)
                    //{
                    //    log += string.Format("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);
                    //}
                    AsyncTaskListener.Update(globalCount, log);
                    log = string.Empty;
                }
            }

            #region Pushing commands
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string SPOName = (!cb_TestTables.Checked) ? "[cqgdb].SPOTable" : "[cqgdb].test_SPOTable";
                string SPODName = (!cb_TestTables.Checked) ? "[cqgdb].SPODTable" : "[cqgdb].test_SPODTable";

                using (SqlCommand cmd = new SqlCommand(SPOName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@option", tblOptions);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        AsyncTaskListener.LogMessage(ex.Message);
                    }
                }
                using (SqlCommand cmd = new SqlCommand(SPODName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@optiondata", tblOptionDatas);

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
            #endregion

            AsyncTaskListener.LogMessageFormat("Pushed {0} entries to {1} {2}TBLOPTIONS and {2}TBLOPTIONDATAS tables", globalCount, DatabaseName, TablesPrefix);

        }

        void DropTempTables()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("[cqgdb].DropTempTables", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
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
        }
    }
}