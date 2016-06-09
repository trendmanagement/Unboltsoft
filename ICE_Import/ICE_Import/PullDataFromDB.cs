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

            var tblcontracts_ = Context.tblcontracts;
            var tbldailycontractsettlements_ = Context.tbldailycontractsettlements;
            var tbloptions_ = Context.tbloptions;
            var tbloptiondatas_ = Context.tbloptiondatas;

            var listOption = new List<tbloption>();

			var contractList = new List<tblcontract>();
            var dailyContractList = new List<tbldailycontractsettlement>();
			var idcontractDictionary = new Dictionary<DateTime, long>();
            var optionList = new List<tbloption>();
            var optionDataList = new List<tbloptiondata>();
			var idoptionDictionary = new Dictionary<Tuple<DateTime, double>, long>();

            try
            {
				var currentContract = new tblcontract();
                await Task.Run(() =>
                            {
								foreach(var stripName in stripNameHashSet)
								{
									try
									{
										currentContract = (from item in tblcontracts_
															where item.monthint == stripName.Month 
															&& item.year == stripName.Year
															&& item.idinstrument == IdInstrument
															select item
															).ToList()[0];
									}
									catch(SqlException)
									{
										continue;
									}
									catch(ArgumentOutOfRangeException)
									{
										continue;
									}

									contractList.Add(currentContract);
									idcontractDictionary.Add(stripName, currentContract.idcontract);
								}
                            }, cts.Token);
                LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLCONTRACT table", contractList.Count, DatabaseName, TablesPrefix));

				var currentDailyContract = new tbldailycontractsettlement();
				long idcontract;
				bool isID;
                await Task.Run(() =>
                                {
								 foreach(var tuple in stripNameDateHashSet)
								 {
									isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
									if(isID)
									{
										try
										{
											currentDailyContract = (from item in tbldailycontractsettlements_
																	where item.idcontract == idcontract 
																	&& item.date == tuple.Item2
																	select item
																	).ToList()[0];
										}
										catch(SqlException)
										{
											continue;
										}
										catch(ArgumentOutOfRangeException)
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
                                }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", dailyContractList.Count, DatabaseName, TablesPrefix));

				var currentOption = new tbloption();
                await Task.Run(() =>
                {
							try
							{
								optionList = (from item in tbloptions_
												where 
												item.optionyear >= stripNameHashSet.Min().Year
												&& item.optionyear <= stripNameHashSet.Max().Year
												&& item.idinstrument == IdInstrument
												select item
												).ToList();
							}
							catch(SqlException)
							{

							}
                }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONS table", optionList.Count, DatabaseName, TablesPrefix));

				var currentOptionData = new tbloptiondata();

                await Task.Run(() =>
                                    {
									foreach (var id in IdOptionList)
									{
										try
										{
											currentOptionData = (from item in tbloptiondatas_
															where 
															item.idoption == id
															select item
															).ToList()[0];
										}
										catch(SqlException)
										{
											continue;
										}
										catch(ArgumentOutOfRangeException)
										{
											continue;
										}
										optionDataList.Add(currentOptionData);
									}

                                    }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", optionDataList.Count, DatabaseName, TablesPrefix));

            }
            catch (OperationCanceledException cancel)
            {
                LogMessage(cancel.Message);
            }
#if !DEBUG
            catch (Exception ex)
            {
                LogMessage("ERROR");
                LogMessage(ex.Message);
            }
#endif
            finally
            {
                int totalCount =
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count();

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, DatabaseName));
            }

			dataGridViewContract.DataSource = contractList;
            dataGridViewDailyContract.DataSource = dailyContractList;
            dataGridViewOption.DataSource = optionList;
            dataGridViewOptionData.DataSource = optionDataList;

			EnableDisable(false);
        }

        async void PullDataFromDBTest()
        {
            cts = new CancellationTokenSource();

            var tblcontracts_ = Context.test_tblcontracts;
            var tbldailycontractsettlements_ = Context.test_tbldailycontractsettlements;
            var tbloptions_ = Context.test_tbloptions;
            var tbloptiondatas_ = Context.test_tbloptiondatas;

            var listOption = new List<test_tbloption>();

			var contractList = new List<test_tblcontract>();
            var dailyContractList = new List<test_tbldailycontractsettlement>();
			var idcontractDictionary = new Dictionary<DateTime, long>();
            var optionList = new List<test_tbloption>();
            var optionDataList = new List<test_tbloptiondata>();
			var idoptionDictionary = new Dictionary<Tuple<DateTime, double>, long>();

            try
            {
				var currentContract = new test_tblcontract();
                await Task.Run(() =>
                            {
								foreach(var stripName in stripNameHashSet)
								{
									try
									{
										currentContract = (from item in tblcontracts_
															where item.monthint == stripName.Month 
															&& item.year == stripName.Year
															&& item.idinstrument == IdInstrument
															select item
															).ToList()[0];
									}
									catch(SqlException)
									{
										continue;
									}
									catch(ArgumentOutOfRangeException)
									{
										continue;
									}

									contractList.Add(currentContract);
									idcontractDictionary.Add(stripName, currentContract.idcontract);
								}
                            }, cts.Token);
                LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLCONTRACT table", contractList.Count, DatabaseName, TablesPrefix));

				var currentDailyContract = new test_tbldailycontractsettlement();
				long idcontract;
				bool isID;
                await Task.Run(() =>
                                {
								 foreach(var tuple in stripNameDateHashSet)
								 {
									isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
									if(isID)
									{
										try
										{
											currentDailyContract = (from item in tbldailycontractsettlements_
																	where item.idcontract == idcontract 
																	&& item.date == tuple.Item2
																	select item
																	).ToList()[0];
										}
										catch(SqlException)
										{
											continue;
										}
										catch(ArgumentOutOfRangeException)
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
                                }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", dailyContractList.Count, DatabaseName, TablesPrefix));

				var currentOption = new test_tbloption();
                await Task.Run(() =>
                {
							try
							{
								optionList = (from item in tbloptions_
												where 
												item.optionyear >= stripNameHashSet.Min().Year
												&& item.optionyear <= stripNameHashSet.Max().Year
												&& item.idinstrument == IdInstrument
												select item
												).ToList();
							}
							catch(SqlException)
							{

							}
                }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONS table", optionList.Count, DatabaseName, TablesPrefix));

				var currentOptionData = new test_tbloptiondata();

                await Task.Run(() =>
                                    {
									foreach (var id in IdOptionList)
									{
										try
										{
											currentOptionData = (from item in tbloptiondatas_
															where 
															item.idoption == id
															select item
															).ToList()[0];
										}
										catch(SqlException)
										{
											continue;
										}
										catch(ArgumentOutOfRangeException)
										{
											continue;
										}
										optionDataList.Add(currentOptionData);
									}

                                    }, cts.Token);
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", optionDataList.Count, DatabaseName, TablesPrefix));

            }
            catch (OperationCanceledException cancel)
            {
                LogMessage(cancel.Message);
            }
#if !DEBUG
            catch (Exception ex)
            {
                LogMessage("ERROR");
                LogMessage(ex.Message);
            }
#endif
            finally
            {
                int totalCount =
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count();

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, DatabaseName));
            }

			dataGridViewContract.DataSource = contractList;
            dataGridViewDailyContract.DataSource = dailyContractList;
            dataGridViewOption.DataSource = optionList;
            dataGridViewOptionData.DataSource = optionDataList;

			EnableDisable(false);
        }

    }
}