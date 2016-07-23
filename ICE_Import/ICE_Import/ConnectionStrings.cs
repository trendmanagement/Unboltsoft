using System.Configuration;

namespace ICE_Import
{
    class ConnectionStrings
    {
        public string Local;
        public string TMLDB_Copy;
        public string TMLDB;

        public ConnectionStrings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStrings = config.ConnectionStrings.ConnectionStrings;

            // The first several connection strings in the collection are defined by the environment.
            // Only the last three ones are defined by our program.
            int n = connectionStrings.Count;
            Local = connectionStrings[n - 3].ConnectionString;
            TMLDB_Copy = connectionStrings[n - 2].ConnectionString;
            TMLDB = connectionStrings[n - 1].ConnectionString;
        }
    }
}
