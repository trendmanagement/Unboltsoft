using System;

namespace ICE_Import
{
    /// <summary>
    /// Helper class that provides switching of:
    /// 1. The stored procedures we use (to populate test or non-test tables).
    /// 2. The way we call stored procedures (synchronously or asynchronously).
    /// </summary>
    static class StoredProcsHelper
    {
        public delegate int d_SPF(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            DateTime? expirationdate, 
            string cqgsymbol);

        public delegate int d_SPF_Mod(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            string cqgsymbol);

        public delegate int d_SPDF(
            DateTime? spanDate,
            double? settlementPrice,
            char? monthChar, 
            int? yearInt,
            long? volume,
            long? openinterest);

        public delegate int d_SPO(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            DateTime? expirationdate, 
            string cqgsymbol);

        public delegate int d_SPO_Mod(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            string cqgsymbol);

        public delegate int d_SPOD(
            int? idoption,
            DateTime? datetime,
            double? price,
            double? impliedvol,
            double? timetoexpinyears);

        public static d_SPF SPF;
        public static d_SPF_Mod SPF_Mod;
        public static d_SPDF SPDF;
        public static d_SPO SPO;
        public static d_SPO_Mod SPO_Mod;
        public static d_SPOD SPOD;

        private static StoredProcsAsyncHelper StoredProcsAsyncHelper;

        public static void Switch(
            DataClassesTMLDBDataContext context,
            bool isTestTables,
            bool isAsyncUpdate,
            string connectionString)
        {
            if (!isAsyncUpdate)
            {
                StoredProcsAsyncHelper = null;
                if (isTestTables)
                {
                    SPF = context.test_SPF;
                    SPF_Mod = context.test_SPF_Mod;
                    SPDF = context.test_SPDF;
                    SPO = context.test_SPO;
                    SPO_Mod = context.test_SPO_Mod;
                    SPOD = context.test_SPOD;
                }
                else
                {
                    SPF = context.SPF;
                    SPF_Mod = context.SPF_Mod;
                    SPDF = context.SPDF;
                    SPO = context.SPO;
                    SPO_Mod = context.SPO_Mod;
                    SPOD = context.SPOD;
                }
            }
            else
            {
                StoredProcsAsyncHelper = new StoredProcsAsyncHelper(connectionString, isTestTables);
                SPF = StoredProcsAsyncHelper.SPF;
                SPF_Mod = StoredProcsAsyncHelper.SPF_Mod;
                SPDF = StoredProcsAsyncHelper.SPDF;
                SPO = StoredProcsAsyncHelper.SPO;
                SPO_Mod = StoredProcsAsyncHelper.SPO_Mod;
                SPOD = StoredProcsAsyncHelper.SPOD;
            }
        }

        public static void InitAsyncHelper()
        {
            StoredProcsAsyncHelper.ClearCommands();
        }

        public static void FinalizeAsyncHelper()
        {
            StoredProcsAsyncHelper.FlushCommands();
        }
    }
}
