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
                LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLCONTRACT table", tblcontracts_.Count(), DatabaseName, TablesPrefix));

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
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", tbldailycontractsettlements_.Count(), DatabaseName, TablesPrefix));

                try
                {
					var currentOption = new tbloption();
                    await Task.Run(() =>
                    {
						foreach(var tuple in optionNameHashSet)
						{
							isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
							if(isID)
							{
								try
								{
									currentOption = (from item in tbloptions_
													where item.idcontract == idcontract 
													&& item.optionmonthint == tuple.Item1.Month
													&& item.optionyear == tuple.Item1.Year
													&& item.strikeprice == tuple.Item2
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
							}
							else
							{
								continue;
							}
							optionList.Add(currentOption);
							idoptionDictionary.Add(Tuple.Create(tuple.Item1, currentOption.strikeprice), currentOption.idoption);
						}
                    }, cts.Token);
					LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONS table", tbloptions_.Count(), DatabaseName, TablesPrefix));
                }
#if !DEBUG
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
#endif
                finally
                {
                }

				var currentOptionData = new tbloptiondata();
				long idoption;

                await Task.Run(() =>
                                    {
										foreach(var tuple in optionNameDataHashSet)
										{
											isID = idoptionDictionary.TryGetValue(Tuple.Create(tuple.Item1, tuple.Item4), out idoption);
											if(isID)
											{
												try
												{
													currentOptionData = (from item in tbloptiondatas_
																	where item.idoption == idoption 
																	&& item.datetime == tuple.Item2
																	&& item.price == tuple.Item3
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
											optionDataList.Add(currentOptionData);
										}
                                    }, cts.Token);
			LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", tbloptiondatas_.Count(), DatabaseName, TablesPrefix));

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
                LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLCONTRACT table", tblcontracts_.Count(), DatabaseName, TablesPrefix));

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
				LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", tbldailycontractsettlements_.Count(), DatabaseName, TablesPrefix));

                try
                {
					var currentOption = new test_tbloption();
                    await Task.Run(() =>
                    {
						foreach(var tuple in optionNameHashSet)
						{
							isID = idcontractDictionary.TryGetValue(tuple.Item1, out idcontract);
							if(isID)
							{
								try
								{
									currentOption = (from item in tbloptions_
													where item.idcontract == idcontract 
													&& item.optionmonthint == tuple.Item1.Month
													&& item.optionyear == tuple.Item1.Year
													&& item.strikeprice == tuple.Item2
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
							}
							else
							{
								continue;
							}
							optionList.Add(currentOption);
							idoptionDictionary.Add(Tuple.Create(tuple.Item1, currentOption.strikeprice), currentOption.idoption);
						}
                    }, cts.Token);
					LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONS table", tbloptions_.Count(), DatabaseName, TablesPrefix));
                }
#if !DEBUG
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
#endif
                finally
                {
                }

				var currentOptionData = new test_tbloptiondata();
				long idoption;

                await Task.Run(() =>
                                    {
										foreach(var tuple in optionNameDataHashSet)
										{
											isID = idoptionDictionary.TryGetValue(Tuple.Create(tuple.Item1, tuple.Item4), out idoption);
											if(isID)
											{
												try
												{
													currentOptionData = (from item in tbloptiondatas_
																	where item.idoption == idoption 
																	&& item.datetime == tuple.Item2
																	&& item.price == tuple.Item3
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
											optionDataList.Add(currentOptionData);
										}
                                    }, cts.Token);
			LogMessage(string.Format("Pulled {0} entries from {1} {2}TBLOPTIONDATAS table", tbloptiondatas_.Count(), DatabaseName, TablesPrefix));

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