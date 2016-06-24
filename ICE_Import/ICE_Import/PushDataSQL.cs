using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {

        public void PushFuturesSQL(ref int globalCount, CancellationToken ct)
        {
            string SPFName = (cb_TestTables.Checked) ? "[cqgdb].test_SPF" : "[cqgdb].SPF";
            string SPDFName = (cb_TestTables.Checked) ? "[cqgdb].test_SPDF" : "[cqgdb].SPDF";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                StripNameHashSet = new HashSet<DateTime>();
                StripNameDateHashSet = new HashSet<Tuple<DateTime, DateTime>>();

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
                    try
                    {
                        if (newFuture)
                        {
                            DateTime expirationDate = TMLDBReader.GetExpirationDate(
                                "future",
                                IdInstrument,
                                future.StripName,
                                ref log);

                            using (SqlCommand cmd = new SqlCommand(SPFName, connection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@contractname", SqlDbType.VarChar).Value = contractname;
                                cmd.Parameters.Add("@month", SqlDbType.Char).Value = monthChar;
                                cmd.Parameters.Add("@monthint", SqlDbType.Int).Value = future.StripName.Month;
                                cmd.Parameters.Add("@year", SqlDbType.Int).Value = future.StripName.Year;
                                cmd.Parameters.Add("@idinstrument", SqlDbType.Int).Value = IdInstrument;
                                cmd.Parameters.Add("@expirationdate", SqlDbType.Date).Value = expirationDate;
                                cmd.Parameters.Add("@cqgsymbol", SqlDbType.VarChar).Value = contractname;

                                cmd.ExecuteNonQuery();
                            }
                            StripNameHashSet.Add(future.StripName);
                        }

                        using (SqlCommand cmd = new SqlCommand(SPDFName, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@monthChar", SqlDbType.Char).Value = monthChar;
                            cmd.Parameters.Add("@yearInt", SqlDbType.Int).Value = future.StripName.Year;
                            cmd.Parameters.Add("@spanDate", SqlDbType.Date).Value = future.Date;
                            cmd.Parameters.Add("@settlementPrice", SqlDbType.Float).Value = future.SettlementPrice.GetValueOrDefault();
                            cmd.Parameters.Add("@volume", SqlDbType.BigInt).Value = (long)future.Volume.GetValueOrDefault();
                            cmd.Parameters.Add("@openinterest", SqlDbType.BigInt).Value = (long)future.OpenInterest.GetValueOrDefault();

                            cmd.ExecuteNonQuery();
                        }

                        StripNameDateHashSet.Add(Tuple.Create(future.StripName, future.Date));
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        log += string.Format("ERROR message from {0} pushing {1}TBLCONTRACT table\n", DatabaseName, TablesPrefix);
                        log += ex.Message + "\n";
                    }
#endif
                    finally
                    {
                        globalCount++;
                        if (globalCount == ParsedData.FutureRecords.Count)
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
        }

        public void PushOptionsSQL(ref int globalCount, CancellationToken ct)
        {
            string SPOName = (cb_TestTables.Checked) ? "[cqgdb].test_SPO" : "[cqgdb].SPO";
            string SPODName = (cb_TestTables.Checked) ? "[cqgdb].test_SPOD" : "[cqgdb].SPOD";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {

                connection.Open();

                OptionDataHashSet = new HashSet<Tuple<DateTime, DateTime>>();

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

                        DateTime expirationDate = TMLDBReader.GetExpirationDate(
                            "option",
                            IdInstrument,
                            option.StripName,
                            ref log);


                        using (SqlCommand cmd = new SqlCommand(SPOName, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@optionname", SqlDbType.VarChar).Value = optionName;
                            cmd.Parameters.Add("@optionmonth", SqlDbType.Char).Value = monthChar;
                            cmd.Parameters.Add("@optionmonthint", SqlDbType.Int).Value = option.StripName.Month;
                            cmd.Parameters.Add("@optionyear", SqlDbType.Int).Value = option.StripName.Year;
                            cmd.Parameters.Add("@strikeprice", SqlDbType.Float).Value = option.StrikePrice.GetValueOrDefault();
                            cmd.Parameters.Add("@callorput", SqlDbType.Char).Value = option.OptionType;
                            cmd.Parameters.Add("@idinstrument", SqlDbType.BigInt).Value = IdInstrument;
                            cmd.Parameters.Add("@expirationdate", SqlDbType.Date).Value = expirationDate;
                            cmd.Parameters.Add("@cqgsymbol", SqlDbType.VarChar).Value = optionName;

                            cmd.ExecuteNonQuery();
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


                        using (SqlCommand cmd = new SqlCommand(SPODName, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@optionname", SqlDbType.VarChar).Value = optionName;
                            cmd.Parameters.Add("@datetime", SqlDbType.Date).Value = option.Date;
                            cmd.Parameters.Add("@price", SqlDbType.Float).Value = option.SettlementPrice.GetValueOrDefault();
                            cmd.Parameters.Add("@impliedvol", SqlDbType.Float).Value = impliedvol;
                            cmd.Parameters.Add("@timetoexpinyears", SqlDbType.Float).Value = futureYear - expirateYear;

                            cmd.ExecuteNonQuery();
                        }

                        //IdOptionHashSet.Add(idoption);
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
                        if (globalCount == ParsedData.FutureRecords.Count + ParsedData.OptionRecords.Count)
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
}
