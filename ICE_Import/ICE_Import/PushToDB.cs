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
