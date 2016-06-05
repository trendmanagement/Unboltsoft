using System;
using System.Collections.Generic;
using System.Linq;

namespace ICE_Import
{
    class TMLDBReader
    {
        public DataClassesTMLDBDataContext Context;

        private Dictionary<Tuple<long, DateTime>, DateTime> IdInstrAndStripNameToExpDate;

        public TMLDBReader(DataClassesTMLDBDataContext contextTMLDB)
        {
            this.Context = contextTMLDB;
            this.IdInstrAndStripNameToExpDate = new Dictionary<Tuple<long, DateTime>, DateTime>();
        }

        public bool GetThreeParams(string productName, ref long idInstrument, ref string cqgSymbol, ref double tickSize)
        {
            AsyncTaskListener.LogMessage("Reading ID Instrument, CQG Symbol and Tick Size from TMLDB...");

            tblinstrument record;
            try
            {
                record = Context.tblinstruments.Where(item => item.description == productName).First();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            idInstrument = record.idinstrument;

            cqgSymbol = record.cqgsymbol;

            double secondaryoptionticksize = record.secondaryoptionticksize;
            tickSize = (secondaryoptionticksize > 0) ? secondaryoptionticksize : record.optionticksize;
            
            AsyncTaskListener.LogMessageFormat(
                "ID Instrument = {0}\nCQG Symbol = {1}\nTick Size = {2}",
                idInstrument,
                cqgSymbol,
                tickSize);

            return true;
        }

        public bool GetRisk(ref double riskFreeInterestRate)
        {
            AsyncTaskListener.LogMessage("Reading Risk Free Interest Rate from TMLDB...");

            try
            {
                var idoptioninputsymbol = Context.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == 1).First().idoptioninputsymbol;
                tbloptioninputdata[] tbloptioninputdatas = Context.tbloptioninputdatas.Where(item =>
                    item.idoptioninputsymbol == idoptioninputsymbol).ToArray();
                DateTime optioninputdatetime = new DateTime();
                for (int i = 0; i < tbloptioninputdatas.Length; i++)
                {
                    if (i != 0)
                    {
                        if (optioninputdatetime < tbloptioninputdatas[i].optioninputdatetime)
                        {
                            optioninputdatetime = tbloptioninputdatas[i].optioninputdatetime;
                        }
                    }
                    else
                    {
                        optioninputdatetime = tbloptioninputdatas[i].optioninputdatetime;
                    }
                }

                //--?-- From where this varable in query
                var OPTION_INPUT_TYPE_RISK_FREE_RATE = 1;

                //--?-- What difference between idoptioninputsymbol and idoptioninputsymbol2
                var idoptioninputsymbol2 = Context.tbloptioninputsymbols.Where(item2 =>
                    item2.idoptioninputtype == OPTION_INPUT_TYPE_RISK_FREE_RATE).First().idoptioninputsymbol;

                riskFreeInterestRate = Context.tbloptioninputdatas.Where(item =>
                    item.idoptioninputsymbol == idoptioninputsymbol2
                        && item.optioninputdatetime == optioninputdatetime).First().optioninputclose;

                AsyncTaskListener.LogMessageFormat(
                    "Risk Free Interest Rate = {0}",
                    riskFreeInterestRate);

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public DateTime GetExpirationDate(
            string name,
            long idInstrument,
            DateTime stripName,
            ref string log)
        {
            var key = Tuple.Create(idInstrument, stripName);

            DateTime expirationDate;
            bool found = IdInstrAndStripNameToExpDate.TryGetValue(key, out expirationDate);
            if (found)
            {
                return expirationDate;
            }
            else
            {
                tblcontractexpiration record;
                try
                {
                    record = Context.tblcontractexpirations.First(
                        item =>
                        item.idinstrument == idInstrument &&
                        item.optionyear == stripName.Year &&
                        item.optionmonthint == stripName.Month);
                }
                catch (InvalidOperationException)
                {
                    log += string.Format(
                        "Failed to find expirationdate for {0} with idinstrument = {1}, optionyear = {2}, optionmonthint = {3} in tblcontractexpirations.",
                        name,
                        idInstrument,
                        stripName.Year,
                        stripName.Month);
                    return new DateTime();
                }

                expirationDate = record.expirationdate;

                IdInstrAndStripNameToExpDate.Add(key, expirationDate);

                return expirationDate;
            }
        }
    }
}
