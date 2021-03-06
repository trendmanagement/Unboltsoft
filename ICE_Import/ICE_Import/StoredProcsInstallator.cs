﻿using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ICE_Import
{
    static class StoredProcsInstallator
    {
        static bool[] isSPInstalled = new bool[2];  // 0 - non-test SPs, 1 - test SPs
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
            int spBunchIdx = isTestTables ? 1 : 0;

            if (isSPInstalled[spBunchIdx])
            {
                AsyncTaskListener.LogMessage("Stored procedures were installed before");
                return true;
            }

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
                        isSPInstalled[spBunchIdx] = true;
                    }
                    catch (SqlException ex)
                    {
                        AsyncTaskListener.LogMessage(ex.Message);

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
                                isSPInstalled[spBunchIdx] = false;
                                return false;
                            }
                        }
                        // Try again
                        Thread.Sleep(2000);
                        try
                        {
                            createProcCommand.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {

                        }
                    }

                    AsyncTaskListener.LogMessage("    " + fileName + " - done");
                }

                connection.Close();
            }

            AsyncTaskListener.LogMessage("Completed installing stored procedures");

            return true;
        }
    }
}
