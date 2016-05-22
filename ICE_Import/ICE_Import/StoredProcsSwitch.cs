using System;

namespace ICE_Import
{
    /// <summary>
    /// Switch of test/non-test stored procedures.
    /// </summary>
    static class StoredProcsSwitch
    {
        public delegate object d_sp_updateContractTblFromSpanUpsert(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            DateTime? expirationdate,
            string cqgsymbol);

        public delegate int d_sp_updateOrInsertContractSettlementsFromSpanUpsert(
            int? futureContractID,
            DateTime? spanDate,
            double? settlementPrice);

        public delegate int d_sp_updateOrInsertTbloptionsInfoAndDataUpsert(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            DateTime? expirationdate,
            long? idcontract,
            string cqgsymbol,
            DateTime? datetime,
            double? price,
            double? impliedvol,
            double? timetoexpinyears);

        public static d_sp_updateContractTblFromSpanUpsert sp_updateContractTblFromSpanUpsert;
        public static d_sp_updateOrInsertContractSettlementsFromSpanUpsert sp_updateOrInsertContractSettlementsFromSpanUpsert;
        public static d_sp_updateOrInsertTbloptionsInfoAndDataUpsert sp_updateOrInsertTbloptionsInfoAndDataUpsert;

        public static void Update(DataClassesTMLDBDataContext context, bool isTestTables)
        {
            if (isTestTables)
            {
                sp_updateContractTblFromSpanUpsert = context.test_sp_updateContractTblFromSpanUpsert;
                sp_updateOrInsertContractSettlementsFromSpanUpsert = context.test_sp_updateOrInsertContractSettlementsFromSpanUpsert;
                sp_updateOrInsertTbloptionsInfoAndDataUpsert = context.test_sp_updateOrInsertTbloptionsInfoAndDataUpsert;
            }
            else
            {
                sp_updateContractTblFromSpanUpsert = context.sp_updateContractTblFromSpanUpsert;
                sp_updateOrInsertContractSettlementsFromSpanUpsert = context.sp_updateOrInsertContractSettlementsFromSpanUpsert;
                sp_updateOrInsertTbloptionsInfoAndDataUpsert = context.sp_updateOrInsertTbloptionsInfoAndDataUpsert;
            }
        }
    }
}
