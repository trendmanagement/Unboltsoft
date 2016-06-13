// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING
// WARNING                                                                         WARNING
// WARNING    DO NOT EDIT THIS .CS FILE, BECAUSE ALL YOUR CHANGES WILL BE LOST!    WARNING
// WARNING    EDIT CORRESPONDING .TT FILE INSTEAD!                                 WARNING
// WARNING                                                                         WARNING
// WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING WARNING

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        async void PullDataFromDB()
        {
            cts = new CancellationTokenSource();

            Dictionary<DateTime, long> idcontractDictionary = null;
            List<tblcontract> contractList = null;
            List<tbldailycontractsettlement> dailyContractList = null;
            List<tbloption> optionList = null;
            List<tbloptiondata> optionDataList = null;
            
            try
            {
                AsyncTaskListener.LogMessage("Started pulling FUTURES data...");
                await Task.Run(() => PullFutures(out idcontractDictionary, out contractList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLCONTRACT table", contractList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling DAILY FUTURES data...");
                await Task.Run(() => PullDailyFutures(idcontractDictionary, out dailyContractList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", dailyContractList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling OPTIONS data...");
                await Task.Run(() => PullOptions(out optionList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLOPTIONS table", optionList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling DAILY OPTIONS data...");
                await Task.Run(() => PullDailyOptions(out optionDataList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", optionDataList.Count, DatabaseName, TablesPrefix);
            }
            catch (OperationCanceledException cancel)
            {
                AsyncTaskListener.LogMessage(cancel.Message);
            }
#if !DEBUG
            catch (Exception ex)
            {
                AsyncTaskListener.LogMessage("ERROR");
                AsyncTaskListener.LogMessage(ex.Message);
            }
#endif
            finally
            {
                int totalCount =
                    contractList.Count() +
                    dailyContractList.Count() +
                    optionList.Count() +
                    optionDataList.Count();
                AsyncTaskListener.LogMessageFormat("Pulled: {0} entries from {1} DB", totalCount, DatabaseName);
            }

            dataGridViewContract.DataSource = contractList;
            dataGridViewDailyContract.DataSource = dailyContractList;
            dataGridViewOption.DataSource = optionList;
            dataGridViewOptionData.DataSource = optionDataList;

            EnableDisable(false);
        }

        void PullFutures(
            out Dictionary<DateTime, long> idcontractDictionary,
            out List<tblcontract> contractList)
        {
            idcontractDictionary = new Dictionary<DateTime, long>();
            contractList = new List<tblcontract>();

            var tblcontracts = Context.tblcontracts;

            foreach (var stripName in StripNameHashSet)
            {
                tblcontract currentContract;

                try
                {
                    currentContract = (from item in tblcontracts
                                       where item.monthint == stripName.Month &&
                                       item.year == stripName.Year &&
                                       item.idinstrument == IdInstrument
                                       select item
                                       ).First();
                }
                catch (SqlException)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
        
                idcontractDictionary.Add(stripName, currentContract.idcontract);
                contractList.Add(currentContract);
            }

            contractList = contractList.OrderBy(item => item.idcontract).ToList();
        }

        void PullDailyFutures(
            Dictionary<DateTime, long> idcontractDictionary,
            out List<tbldailycontractsettlement> dailyContractList)
        {
            dailyContractList = new List<tbldailycontractsettlement>();

            var tbldailycontractsettlements = Context.tbldailycontractsettlements;

            foreach (var tuple in StripNameDateHashSet)
            {
                tbldailycontractsettlement currentDailyContract;

                long idcontract;
                bool isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
                if (isID)
                {
                    try
                    {
                        currentDailyContract = (from item in tbldailycontractsettlements
                                                where item.idcontract == idcontract &&
                                                item.date == tuple.Item2
                                                select item
                                                ).First();
                    }
                    catch (SqlException)
                    {
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                dailyContractList.Add(currentDailyContract);
            }

            dailyContractList = dailyContractList.OrderBy(item => item.idcontract).OrderBy(item => item.date).ToList();
        }

        void PullOptions(
            out List<tbloption> optionList)
        {
            optionList = new List<tbloption>();

            var tbloptions = Context.tbloptions;

            try
            {
                optionList = (from item in tbloptions
                              where 
                              item.optionyear >= StripNameHashSet.Min().Year &&
                              item.optionyear <= StripNameHashSet.Max().Year &&
                              item.idinstrument == IdInstrument
                              select item
                              ).ToList();
            }
            catch (SqlException)
            {
            }

            optionList = optionList.OrderBy(item => item.idoption).ToList();
        }

        void PullDailyOptions(
            out List<tbloptiondata> optionDataList)
        {
            optionDataList = new List<tbloptiondata>();

            var tbloptiondatas = Context.tbloptiondatas;


			if (IdOptionHashSet.Count == 0)
            {

                try
                {
                    optionDataList = (from item in tbloptiondatas
                                             select item).ToList();
                }
                catch (SqlException)
                {
                }

            }
            else
            {
                foreach (var id in IdOptionHashSet)
                {
					IEnumerable<tbloptiondata> currentOptionData;

                    try
                    {
                        currentOptionData = (from item in tbloptiondatas
                                             where
                                             item.idoption == id
                                             select item);
                    }
                    catch (SqlException)
                    {
                        continue;
                    }

                    //optionDataList.AddRange(currentOptionData);
                }
            }

            optionDataList = optionDataList.OrderBy(item => item.idoption).ToList();
        }

        async void PullDataFromDBTest()
        {
            cts = new CancellationTokenSource();

            Dictionary<DateTime, long> idcontractDictionary = null;
            List<test_tblcontract> contractList = null;
            List<test_tbldailycontractsettlement> dailyContractList = null;
            List<test_tbloption> optionList = null;
            List<test_tbloptiondata> optionDataList = null;
            
            try
            {
                AsyncTaskListener.LogMessage("Started pulling FUTURES data...");
                await Task.Run(() => PullFuturesTest(out idcontractDictionary, out contractList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLCONTRACT table", contractList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling DAILY FUTURES data...");
                await Task.Run(() => PullDailyFuturesTest(idcontractDictionary, out dailyContractList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", dailyContractList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling OPTIONS data...");
                await Task.Run(() => PullOptionsTest(out optionList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLOPTIONS table", optionList.Count, DatabaseName, TablesPrefix);

                AsyncTaskListener.LogMessage("Started pulling DAILY OPTIONS data...");
                await Task.Run(() => PullDailyOptions(out optionDataList), cts.Token);
                AsyncTaskListener.LogMessageFormat("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", optionDataList.Count, DatabaseName, TablesPrefix);
            }
            catch (OperationCanceledException cancel)
            {
                AsyncTaskListener.LogMessage(cancel.Message);
            }
#if !DEBUG
            catch (Exception ex)
            {
                AsyncTaskListener.LogMessage("ERROR");
                AsyncTaskListener.LogMessage(ex.Message);
            }
#endif
            finally
            {
                int totalCount =
                    contractList.Count() +
                    dailyContractList.Count() +
                    optionList.Count() +
                    optionDataList.Count();
                AsyncTaskListener.LogMessageFormat("Pulled: {0} entries from {1} DB", totalCount, DatabaseName);
            }

            dataGridViewContract.DataSource = contractList;
            dataGridViewDailyContract.DataSource = dailyContractList;
            dataGridViewOption.DataSource = optionList;
            dataGridViewOptionData.DataSource = optionDataList;

            EnableDisable(false);
        }

        void PullFuturesTest(
            out Dictionary<DateTime, long> idcontractDictionary,
            out List<test_tblcontract> contractList)
        {
            idcontractDictionary = new Dictionary<DateTime, long>();
            contractList = new List<test_tblcontract>();

            var tblcontracts = Context.test_tblcontracts;

            foreach (var stripName in StripNameHashSet)
            {
                test_tblcontract currentContract;

                try
                {
                    currentContract = (from item in tblcontracts
                                       where item.monthint == stripName.Month &&
                                       item.year == stripName.Year &&
                                       item.idinstrument == IdInstrument
                                       select item
                                       ).First();
                }
                catch (SqlException)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
        
                idcontractDictionary.Add(stripName, currentContract.idcontract);
                contractList.Add(currentContract);
            }

            contractList = contractList.OrderBy(item => item.idcontract).ToList();
        }

        void PullDailyFuturesTest(
            Dictionary<DateTime, long> idcontractDictionary,
            out List<test_tbldailycontractsettlement> dailyContractList)
        {
            dailyContractList = new List<test_tbldailycontractsettlement>();

            var tbldailycontractsettlements = Context.test_tbldailycontractsettlements;

            foreach (var tuple in StripNameDateHashSet)
            {
                test_tbldailycontractsettlement currentDailyContract;

                long idcontract;
                bool isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
                if (isID)
                {
                    try
                    {
                        currentDailyContract = (from item in tbldailycontractsettlements
                                                where item.idcontract == idcontract &&
                                                item.date == tuple.Item2
                                                select item
                                                ).First();
                    }
                    catch (SqlException)
                    {
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                dailyContractList.Add(currentDailyContract);
            }

            dailyContractList = dailyContractList.OrderBy(item => item.idcontract).OrderBy(item => item.date).ToList();
        }

        void PullOptionsTest(
            out List<test_tbloption> optionList)
        {
            optionList = new List<test_tbloption>();

            var tbloptions = Context.test_tbloptions;

            try
            {
                optionList = (from item in tbloptions
                              where 
                              item.optionyear >= StripNameHashSet.Min().Year &&
                              item.optionyear <= StripNameHashSet.Max().Year &&
                              item.idinstrument == IdInstrument
                              select item
                              ).ToList();
            }
            catch (SqlException)
            {
            }

            optionList = optionList.OrderBy(item => item.idoption).ToList();
        }

        void PullDailyOptions(
            out List<test_tbloptiondata> optionDataList)
        {
            optionDataList = new List<test_tbloptiondata>();

            var tbloptiondatas = Context.test_tbloptiondatas;


			if (IdOptionHashSet.Count == 0)
            {

                try
                {
                    optionDataList = (from item in tbloptiondatas
                                             select item).ToList();
                }
                catch (SqlException)
                {
                }

            }
            else
            {
                foreach (var id in IdOptionHashSet)
                {
					IEnumerable<test_tbloptiondata> currentOptionData;

                    try
                    {
                        currentOptionData = (from item in tbloptiondatas
                                             where
                                             item.idoption == id
                                             select item);
                    }
                    catch (SqlException)
                    {
                        continue;
                    }

                    //optionDataList.AddRange(currentOptionData);
                }
            }

            optionDataList = optionDataList.OrderBy(item => item.idoption).ToList();
        }

    }
}