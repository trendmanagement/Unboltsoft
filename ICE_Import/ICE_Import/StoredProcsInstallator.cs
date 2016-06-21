using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ICE_Import
{
    static class StoredProcsInstallator
    {
        static bool IsSPInstaled;
        const string storedProcsDir = "StoredProcs";
        const string storedProcFileExt = ".sql";
        const string testTablesPrefix = "test_";
        const string dropProcCommandPattern = "DROP PROCEDURE {0};";

        /// <summary>
        /// Install stored procedures from SQL files into DB
        /// </summary>
        public static bool Install(
            string connectionString,
            bool isTestTables,
            CancellationToken ct)
        {

            if (!IsSPInstaled)
            {
                AsyncTaskListener.LogMessage("Started installing stored procedures...");

                // Get paths of all files in "StoredProcs" directory
                string baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string subDir = Path.Combine(baseDir, storedProcsDir);
                string[] filePaths = Directory.GetFiles(subDir);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (string filePath in filePaths)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            connection.Close();
                            break;
                        }

                        // Do not install "test_" stored procedures if we are working with non-test tables and vice versa
                        string fileName = Path.GetFileName(filePath);
                        bool isTestStoredProc = fileName.Contains(testTablesPrefix);
                        bool skip = isTestStoredProc ^ isTestTables;
                        if (skip)
                        {
                            AsyncTaskListener.LogMessage("    " + fileName + " - skipped");
                            continue;
                        }

                        // Install the new stored procedure from SQL file into DB
                        string storedProcBody = File.ReadAllText(filePath);
                        var createProcCommand = new SqlCommand(storedProcBody, connection);
                        try
                        {
                            createProcCommand.ExecuteNonQuery();
                            IsSPInstaled = true;
                        }
                        catch (SqlException ex)
                        {
                            AsyncTaskListener.LogMessage(ex.Message);
                            //string msg = string.Format("There is already an object named '{0}' in the database.", fileName.Substring(0, fileName.Length - 4));
                            //AsyncTaskListener.LogMessage(msg);
                            //if (ex.Message == msg)
                            //{
                            //    continue;
                            //}

                            // Remove the old stored procedure from DB
                            string procName = fileName.Substring(0, fileName.Length - storedProcFileExt.Length);
                            string dropProcCommandBody = string.Format(dropProcCommandPattern, procName);
                            using (var dropProcCommand = new SqlCommand(dropProcCommandBody, connection))
                            {
                                try
                                {
                                    dropProcCommand.ExecuteNonQuery();
                                }
                                catch (SqlException exc)
                                {
                                    AsyncTaskListener.LogMessage(exc.Message);
                                    AsyncTaskListener.LogMessage("    " + fileName + " - FAILED");
                                    IsSPInstaled = false;
                                    return false;
                                }
                            }
                            // Try again
                            createProcCommand.ExecuteNonQuery();
                        }

                        AsyncTaskListener.LogMessage("    " + fileName + " - done");
                    }
                    connection.Close();
                }
                AsyncTaskListener.LogMessage("Completed installing stored procedures");
                return true;
            }
            else
            {
                AsyncTaskListener.LogMessage("SP's was installed.");
                return true;
            }

        }
    }
}
