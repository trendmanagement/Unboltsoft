﻿using System;

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

        public delegate int dt_testSPF(
            string contractname,
            char? month,
            int? monthint,
            int? year,
            int? idinstrument,
            DateTime? expirationdate, 
            string cqgsymbol);

        public delegate int dt_testSPDF(
            DateTime? spanDate,
            double? settlementPrice,
            char? monthChar, 
            int? yearInt,
            long? volume,
            long? openinterest);

        public delegate int dt_testSPO(
            string optionname,
            char? optionmonth,
            int? optionmonthint,
            int? optionyear,
            double? strikeprice,
            char? callorput,
            long? idinstrument,
            DateTime? expirationdate, 
            string cqgsymbol);

        public delegate int dt_testSPOD(
            char? optionmonth,
            int? optionyear,
            DateTime? datetime,
            double? price,
            double? impliedvol,
            double? timetoexpinyears);

        public static dt_testSPF testSPF;
        public static dt_testSPDF testSPDF;
        public static dt_testSPO testSPO;
        public static dt_testSPOD testSPOD;
        public static d_sp_updateContractTblFromSpanUpsert sp_updateContractTblFromSpanUpsert;
        public static d_sp_updateOrInsertContractSettlementsFromSpanUpsert sp_updateOrInsertContractSettlementsFromSpanUpsert;
        public static d_sp_updateOrInsertTbloptionsInfoAndDataUpsert sp_updateOrInsertTbloptionsInfoAndDataUpsert;

        public static void Update(DataClassesTMLDBDataContext context, bool isTestTables)
        {
            if (isTestTables)
            {
                testSPF = context.test_SPF;
                testSPDF = context.test_SPDF;
                testSPO = context.test_SPO;
                testSPOD = context.test_SPOD;
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
