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

            var tblcontracts_ = Context.tblcontracts;
            var tbldailycontractsettlements_ = Context.tbldailycontractsettlements;
            var tbloptions_ = Context.tbloptions;
            var tbloptiondatas_ = Context.tbloptiondatas;

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<tbloption>();

            int count = 100;

            try
            {
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLCONTRACT table", IsLocalDB ? tblcontracts_.Count() : count, DatabaseName, TablesPrefix));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take(IsLocalDB ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", IsLocalDB ? tbldailycontractsettlements_.Count() : count, DatabaseName, TablesPrefix));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take(IsLocalDB ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONS table", IsLocalDB ? tbloptions_.Count() : count, DatabaseName, TablesPrefix));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take(IsLocalDB ? tbloptions_.Count() : count).ToList();
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

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONDATAS table", IsLocalDB ? tbloptiondatas_.Count() : count, DatabaseName, TablesPrefix));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take(IsLocalDB ? tbloptiondatas_.Count() : count).ToList();
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
                    IsLocalDB ?
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
                    :
                    4 * count;

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, DatabaseName));

            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
            EnableDisable(false);
        }

        async void PullDataFromDBTest()
        {
            cts = new CancellationTokenSource();

            var tblcontracts_ = Context.test_tblcontracts;
            var tbldailycontractsettlements_ = Context.test_tbldailycontractsettlements;
            var tbloptions_ = Context.test_tbloptions;
            var tbloptiondatas_ = Context.test_tbloptiondatas;

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<test_tbloption>();

            int count = 100;

            try
            {
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLCONTRACT table", IsLocalDB ? tblcontracts_.Count() : count, DatabaseName, TablesPrefix));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take(IsLocalDB ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLDAILYCONTRACTSETTLEMENT table", IsLocalDB ? tbldailycontractsettlements_.Count() : count, DatabaseName, TablesPrefix));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take(IsLocalDB ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONS table", IsLocalDB ? tbloptions_.Count() : count, DatabaseName, TablesPrefix));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take(IsLocalDB ? tbloptions_.Count() : count).ToList();
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

                LogMessage(string.Format("Started pulling {0} entries from {1} {2}TBLOPTIONDATAS table", IsLocalDB ? tbloptiondatas_.Count() : count, DatabaseName, TablesPrefix));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take(IsLocalDB ? tbloptiondatas_.Count() : count).ToList();
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
                    IsLocalDB ?
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
                    :
                    4 * count;

                LogMessage(string.Format("Pulled: {0} entries from {1} DB", totalCount, DatabaseName));

            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
            EnableDisable(false);
        }

    }
}