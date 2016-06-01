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
            ConnectionStringsSection csSection = config.ConnectionStrings;
            Local = csSection.ConnectionStrings[1].ConnectionString;
            TMLDB_Copy = csSection.ConnectionStrings[2].ConnectionString;
            TMLDB = csSection.ConnectionStrings[3].ConnectionString;
        }
    }
}
