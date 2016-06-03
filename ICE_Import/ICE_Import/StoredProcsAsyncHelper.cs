using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    /// <summary>
    /// Helper class for calling stored procedures asynchronously.
    /// </summary>
    class StoredProcsAsyncHelper
    {
        const int PacketSize = 30;

        string ConnectionString;
        string StoredProcPrefix;

        List<string> Commands;

        public StoredProcsAsyncHelper(string connectionString, bool isTestTables)
        {
            ConnectionString = connectionString;

            StoredProcPrefix = isTestTables ? "test_" : string.Empty;

            Commands = new List<string>(PacketSize);
        }

        public int SPF(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            DateTime? expirationdate,
            string cqgsymbol)
        {
            PushCommand(
                "SPF",
                contractname,
                month,
                monthint,
                year,
                idinstrument,
                expirationdate,
                cqgsymbol);

            return -1;
        }

        public int SPF_Mod(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            string cqgsymbol)
        {
            PushCommand(
                "SPF_Mod",
                contractname,
                month,
                monthint,
                year,
                idinstrument,
                cqgsymbol);

            return -1;
        }

        public int SPDF(
            DateTime? spanDate,
            double? settlementPrice,
            char? monthChar,
            int? yearInt,
            long? volume,
            long? openinterest)
        {
            PushCommand(
                "SPDF",
                spanDate,
                settlementPrice,
                monthChar,
                yearInt,
                volume,
                openinterest);

            return -1;
        }

        public int SPO(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            DateTime? expirationdate,
            string cqgsymbol)
        {
            PushCommand(
                "SPO",
                optionname,
                optionmonth,
                optionmonthint,
                optionyear,
                strikeprice,
                callorput,
                idinstrument,
                expirationdate,
                cqgsymbol);

            return -1;
        }

        public int SPO_Mod(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            string cqgsymbol)
        {
            PushCommand(
                "SPO_Mod",
                optionname,
                optionmonth,
                optionmonthint,
                optionyear,
                strikeprice,
                callorput,
                idinstrument,
                cqgsymbol);

            return -1;
        }

        public int SPOD(
            int? idoption,
            DateTime? datetime,
            double? price,
            double? impliedvol,
            double? timetoexpinyears)
        {
            PushCommand(
                "SPOD",
                idoption,
                datetime,
                price,
                impliedvol,
                timetoexpinyears);

            return -1;
        }

        async void PushCommand(string procNameRoot, params object[] args)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("cqgdb.{0}{1} ", StoredProcPrefix, procNameRoot);

            for (int i = 0; i < args.Length; i++)
            {
                bool isLast = (i == args.Length - 1);
                AppendHelper(sb, args[i], isLast);
            }

            Commands.Add(sb.ToString());

            if (Commands.Count == PacketSize)
            {
                await ConnectAndPushCommandsAsync(new List<string>(Commands));
                Commands.Clear();
            }
        }

        public void ClearCommands()
        {
            Commands.Clear();
        }

        async public void FlushCommands()
        {
            if (Commands.Count != 0)
            {
                await ConnectAndPushCommandsAsync(new List<string>(Commands));
                Commands.Clear();
            }
        }

        void AppendHelper(StringBuilder sb, object value, bool isLast)
        {
            Type type = value.GetType();
            if (type == typeof(string) || type == typeof(char))
            {
                sb.AppendFormat("'{0}'", value);
            }
            else if (type == typeof(DateTime))
            {
                sb.AppendFormat("'{0}'", ((DateTime)value).ToString("yyyy-MM-dd"));
            }
            else
            {
                sb.Append(value);
            }
            if (!isLast)
            {
                sb.Append(',');
            }
        }
        
        async Task ConnectAndPushCommandsAsync(List<string> commands)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlTransaction trans = connection.BeginTransaction())
                    {
                        using (var sqlCommand = new SqlCommand("", connection, trans))
                        {
                            sqlCommand.CommandType = CommandType.Text;

                            sqlCommand.CommandTimeout = 0;

                            foreach (string strCommand in commands)
                            {
                                sqlCommand.CommandText = strCommand;

                                try
                                {
                                    await sqlCommand.ExecuteNonQueryAsync();
                                }
                                catch (Exception e)
                                {
                                    AsyncTaskListener.LogMessage(e.GetType().ToString());
                                    AsyncTaskListener.LogMessage(e.Message);
                                    AsyncTaskListener.LogMessage(strCommand);
                                }
                            }
                        }
                        try
                        {
                            trans.Commit();
                        }
                        catch (Exception e)
                        {
                            AsyncTaskListener.LogMessage(e.Message);
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                AsyncTaskListener.LogMessage(e.Message);
            }
        }
    }
}
