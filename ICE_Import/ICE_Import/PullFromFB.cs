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
            int count = isLocal ? int.MaxValue : 100;
            try
            {
                EnableDisable(true);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLCONTRACT table", count, locRem));

                await Task.Run(() =>
                            {
                                bsContract.DataSource = (from item in context.tblcontracts
                                                         select item
                                                        ).Take(count).ToList();
                            }, cts.Token);

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLDAILYCONTRACTSETTLEMENT table", count, locRem));

                await Task.Run(() =>
                                {
                                    bsDailyContractSettlement.DataSource = (from item in context.tbldailycontractsettlements
                                                                            select item
                                                                            ).Take(count).ToList();
                                }, cts.Token);

                //int count = remoteContext.tbloptions.Where(item => item.cqgsymbol == "somesymbol").Count();
                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONS table", count, locRem));
                try
                {
                    await Task.Run(() =>
                    {
                        listOption = (from item in context.tbloptions
                                          //where item.cqgsymbol == "somesymbol"
                                      select item
                                              ).Take(count).ToList();
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

                SetLogMessage(string.Format("Started pulling {0} entities from {1} TBLOPTIONDATAS table", count, locRem));
                await Task.Run(() =>
                                    {
                                        bsOptionData.DataSource = (from item in context.tbloptiondatas
                                                                   select item
                                                                  ).Take(count).ToList();
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
    }
}
