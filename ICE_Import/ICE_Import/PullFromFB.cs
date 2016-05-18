using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        private async void PullDataFromDB()
        {
            cts = new CancellationTokenSource();
            
            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            List<tbloption> listOption = new List<tbloption>();

            int count = 100;

            try
            {
                EnableDisable(true);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLCONTRACT table", (isLocal) ? context.tblcontracts.Count() : count, locRem));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in context.tblcontracts
                                                         select item
                                                        ).Take((isLocal) ? context.tblcontracts.Count() : count).ToList();
                            }, cts.Token);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLDAILYCONTRACTSETTLEMENT table", (isLocal) ? context.tbldailycontractsettlements.Count() : count, locRem));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in context.tbldailycontractsettlements
                                                                            select item
                                                                            ).Take((isLocal) ? context.tbldailycontractsettlements.Count() : count).ToList();
                                }, cts.Token);

                //int count = remoteContext.tbloptions.Where(item => item.cqgsymbol == "somesymbol").Count();
                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONS table", (isLocal) ? context.tbloptions.Count() : count, locRem));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in context.tbloptions
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take((isLocal) ? context.tbloptions.Count() : count).ToList();
                    }, cts.Token);
                }
                catch (Exception ex)
                {
                    SetLogMessage(ex.Message);
                }
                finally
                {
                    bsOption.DataSource = listOption;
                }

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONDATAS table", (isLocal) ? context.tbloptiondatas.Count() : count, locRem));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in context.tbloptiondatas
                                                                   select item
                                                                  ).Take((isLocal) ? context.tbloptiondatas.Count() : count).ToList();
                                    }, cts.Token);

            }
            catch (OperationCanceledException cancel)
            {
                SetLogMessage(cancel.Message);
            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                EnableDisable(false);

                int totalCount =
                    isLocal ?
                    context.tblcontracts.Count() +
                    context.tbldailycontractsettlements.Count() +
                    context.tbloptions.Count() +
                    context.tbloptiondatas.Count()
                    :
                    4 * count;
                    
                SetLogMessage(string.Format("Pulled: {0} entities from {1} DB", totalCount, locRem));
            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

        private async void PullTastDataFromDB()
        {
            cts = new CancellationTokenSource();

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            List<test_tbloption> listOption = new List<test_tbloption>();

            int count = 100;

            try
            {
                EnableDisable(true);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLCONTRACT table", (isLocal) ? contextTest.test_tblcontracts.Count() : count, locRem));

                await Task.Run(() =>
                {
                    bsContract.DataSource = (from item in contextTest.test_tblcontracts
                                             select item
                                            ).Take((isLocal) ? contextTest.test_tblcontracts.Count() : count).ToList();
                }, cts.Token);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLDAILYCONTRACTSETTLEMENT table", (isLocal) ? contextTest.test_tbldailycontractsettlements.Count() : count, locRem));

                await Task.Run(() =>
                {
                    bsDailyContractSettlement.DataSource = (from item in contextTest.test_tbldailycontractsettlements
                                                            select item
                                                            ).Take((isLocal) ? contextTest.test_tbldailycontractsettlements.Count() : count).ToList();
                }, cts.Token);

                //int count = remoteContext.tbloptions.Where(item => item.cqgsymbol == "somesymbol").Count();
                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONS table", (isLocal) ? contextTest.test_tbloptions.Count() : count, locRem));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in contextTest.test_tbloptions
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take((isLocal) ? contextTest.test_tbloptions.Count() : count).ToList();
                    }, cts.Token);
                }
                catch (Exception ex)
                {
                    SetLogMessage(ex.Message);
                }
                finally
                {
                    bsOption.DataSource = listOption;
                }

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONDATAS table", (isLocal) ? contextTest.test_tbloptiondatas.Count() : count, locRem));
                await Task.Run(() =>
                {
                    bsOptionData.DataSource = (from item in contextTest.test_tbloptiondatas
                                               select item
                                              ).Take((isLocal) ? contextTest.test_tbloptiondatas.Count() : count).ToList();
                }, cts.Token);

            }
            catch (OperationCanceledException cancel)
            {
                SetLogMessage(cancel.Message);
            }
            catch (Exception ex)
            {
                SetLogMessage("ERROR");
                SetLogMessage(ex.Message);
            }
            finally
            {
                EnableDisable(false);

                int totalCount =
                    isLocal ?
                    contextTest.test_tblcontracts.Count() +
                    contextTest.test_tbldailycontractsettlements.Count() +
                    contextTest.test_tbloptions.Count() +
                    contextTest.test_tbloptiondatas.Count()
                    :
                    4 * count;

                SetLogMessage(string.Format("Pulled: {0} entities from {1} DB", totalCount, locRem));
            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

    }
}
