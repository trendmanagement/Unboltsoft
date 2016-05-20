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
        private async void PullDataFromDB()
        {
            var context_ = Context;
            var tblcontracts_ = context_.tblcontracts;
            var tbldailycontractsettlements_ = context_.tbldailycontractsettlements;
            var tbloptions_ = context_.tbloptions;
            var tbloptiondatas_ = context_.tbloptiondatas;

            cts = new CancellationTokenSource();

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<tbloption>();

            int count = 100;

            try
            {
                EnableDisable(true);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLCONTRACT table", (isLocal) ? tblcontracts_.Count() : count, locRem));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take((isLocal) ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLDAILYCONTRACTSETTLEMENT table", (isLocal) ? tbldailycontractsettlements_.Count() : count, locRem));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take((isLocal) ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONS table", (isLocal) ? tbloptions_.Count() : count, locRem));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take((isLocal) ? tbloptions_.Count() : count).ToList();
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

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONDATAS table", (isLocal) ? tbloptiondatas_.Count() : count, locRem));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take((isLocal) ? tbloptiondatas_.Count() : count).ToList();
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
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
                    :
                    4 * count;

                SetLogMessage(string.Format("Pulled: {0} entities from {1} DB", totalCount, locRem));
            }

            dataGridViewOption.DataSource = bsOption;
            dataGridViewOptionData.DataSource = bsOptionData;
            dataGridViewContract.DataSource = bsContract;
            dataGridViewDailyContract.DataSource = bsDailyContractSettlement;
        }

        private async void PullDataFromDBTest()
        {
            var context_ = TestContext;
            var tblcontracts_ = context_.test_tblcontracts;
            var tbldailycontractsettlements_ = context_.test_tbldailycontractsettlements;
            var tbloptions_ = context_.test_tbloptions;
            var tbloptiondatas_ = context_.test_tbloptiondatas;

            cts = new CancellationTokenSource();

            BindingSource bsOption = new BindingSource();
            BindingSource bsOptionData = new BindingSource();
            BindingSource bsContract = new BindingSource();
            BindingSource bsDailyContractSettlement = new BindingSource();

            var listOption = new List<test_tbloption>();

            int count = 100;

            try
            {
                EnableDisable(true);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLCONTRACT table", (isLocal) ? tblcontracts_.Count() : count, locRem));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in tblcontracts_
                                                         select item
                                                        ).Take((isLocal) ? tblcontracts_.Count() : count).ToList();
                            }, cts.Token);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLDAILYCONTRACTSETTLEMENT table", (isLocal) ? tbldailycontractsettlements_.Count() : count, locRem));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in tbldailycontractsettlements_
                                                                            select item
                                                                            ).Take((isLocal) ? tbldailycontractsettlements_.Count() : count).ToList();
                                }, cts.Token);

                //int count = tbloptions_.Where(item => item.cqgsymbol == "somesymbol").Count();
                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONS table", (isLocal) ? tbloptions_.Count() : count, locRem));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in tbloptions_
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take((isLocal) ? tbloptions_.Count() : count).ToList();
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

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONDATAS table", (isLocal) ? tbloptiondatas_.Count() : count, locRem));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in tbloptiondatas_
                                                                   select item
                                                                  ).Take((isLocal) ? tbloptiondatas_.Count() : count).ToList();
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
                    tblcontracts_.Count() +
                    tbldailycontractsettlements_.Count() +
                    tbloptions_.Count() +
                    tbloptiondatas_.Count()
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