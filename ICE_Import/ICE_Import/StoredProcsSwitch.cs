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
        public static d_sp_updateContractTblFromSpanUpsert sp_updateContractTblFromSpanUpsert;
        public static d_sp_updateOrInsertContractSettlementsFromSpanUpsert sp_updateOrInsertContractSettlementsFromSpanUpsert;
        public static d_sp_updateOrInsertTbloptionsInfoAndDataUpsert sp_updateOrInsertTbloptionsInfoAndDataUpsert;

        public static void Update(DataClassesTMLDBDataContext context, bool isTestTables)
        {
            if (isTestTables)
            {
                SPF = context.test_SPF;
                SPF_Mod = context.test_SPF_Mod;
                SPDF = context.test_SPDF;
                SPO = context.test_SPO;
                SPO_Mod = context.test_SPO_Mod;
                SPOD = context.test_SPOD;
                sp_updateContractTblFromSpanUpsert = context.test_sp_updateContractTblFromSpanUpsert;
                sp_updateOrInsertContractSettlementsFromSpanUpsert = context.test_sp_updateOrInsertContractSettlementsFromSpanUpsert;
                sp_updateOrInsertTbloptionsInfoAndDataUpsert = context.test_sp_updateOrInsertTbloptionsInfoAndDataUpsert;
            }
            else
            {
                SPF = context.SPF;
                SPF_Mod = context.SPF_Mod;
                SPDF = context.SPDF;
                SPO = context.SPO;
                SPO_Mod = context.SPO_Mod;
                SPOD = context.SPOD;
                sp_updateContractTblFromSpanUpsert = context.sp_updateContractTblFromSpanUpsert;
                sp_updateOrInsertContractSettlementsFromSpanUpsert = context.sp_updateOrInsertContractSettlementsFromSpanUpsert;
                sp_updateOrInsertTbloptionsInfoAndDataUpsert = context.sp_updateOrInsertTbloptionsInfoAndDataUpsert;
            }
        }
    }
}
