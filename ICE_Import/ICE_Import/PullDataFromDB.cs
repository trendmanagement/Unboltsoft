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

            var tblcontracts_ = context.tblcontracts;
            var tbldailycontractsettlements_ = context.tbldailycontractsettlements;
            var tbloptions_ = context.tbloptions;
            var tbloptiondatas_ = context.tbloptiondatas;

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<tbloption>();

            int count = 100;

            try
            {
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLCONTRACT table", isLocalDB ? tblcontracts_.Count() : count, databaseName, tablesPrefix));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take(isLocalDB ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", isLocalDB ? tbldailycontractsettlements_.Count() : count, databaseName, tablesPrefix));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take(isLocalDB ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONS table", isLocalDB ? tbloptions_.Count() : count, databaseName, tablesPrefix));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take(isLocalDB ? tbloptions_.Count() : count).ToList();
                    }, cts.Token);
                }
#if !DEBUG
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
#endif
                finally
                {
                    bsOption.DataSource = listOption;
                }

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONDATAS table", isLocalDB ? tbloptiondatas_.Count() : count, databaseName, tablesPrefix));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take(isLocalDB ? tbloptiondatas_.Count() : count).ToList();
                                    }, cts.Token);

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
                    isLocalDB ?
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
                    :
                    4 * count;

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, databaseName));

                EnableDisable(false);
            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

        async void PullDataFromDBTest()
        {
            cts = new CancellationTokenSource();

            var tblcontracts_ = context.test_tblcontracts;
            var tbldailycontractsettlements_ = context.test_tbldailycontractsettlements;
            var tbloptions_ = context.test_tbloptions;
            var tbloptiondatas_ = context.test_tbloptiondatas;

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<test_tbloption>();

            int count = 100;

            try
            {
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLCONTRACT table", isLocalDB ? tblcontracts_.Count() : count, databaseName, tablesPrefix));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take(isLocalDB ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", isLocalDB ? tbldailycontractsettlements_.Count() : count, databaseName, tablesPrefix));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take(isLocalDB ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONS table", isLocalDB ? tbloptions_.Count() : count, databaseName, tablesPrefix));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take(isLocalDB ? tbloptions_.Count() : count).ToList();
                    }, cts.Token);
                }
#if !DEBUG
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
#endif
                finally
                {
                    bsOption.DataSource = listOption;
                }

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONDATAS table", isLocalDB ? tbloptiondatas_.Count() : count, databaseName, tablesPrefix));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take(isLocalDB ? tbloptiondatas_.Count() : count).ToList();
                                    }, cts.Token);

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
                    isLocalDB ?
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
                    :
                    4 * count;

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, databaseName));

                EnableDisable(false);
            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

    }
}