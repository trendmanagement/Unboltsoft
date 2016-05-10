using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class DataBaseForm : Form
    {
        private DataGridViewTextBoxColumn idcontractDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn monthDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn monthintDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn yearDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idinstrumentDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn expirationdateDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn cqgsymbolDataGridViewTextBoxColumn1;
        private BindingSource tblcontractsBindingSource;
        private IContainer components;
        private TestDatabaseDataSet2 testDatabaseDataSet2;
        private TabPage tabPageDailyContract;
        private DataGridView dataGridViewDailyContract;
        private DataGridViewTextBoxColumn iddailycontractsettlementsDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idcontractDataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn settlementDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn volumeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn openinterestDataGridViewTextBoxColumn;
        private BindingSource tbldailycontractsettlementsBindingSource;
        private TestDatabaseDataSet3 testDatabaseDataSet3;
        private TestDatabaseDataSetTableAdapters.tbloptionsTableAdapter tbloptionsTableAdapter;
        private BindingSource testDatabaseDataSetBindingSource;
        private TestDatabaseDataSet testDatabaseDataSet;
        private TestDatabaseDataSet1TableAdapters.tbloptiondataTableAdapter tbloptiondataTableAdapter;
        private TestDatabaseDataSet2TableAdapters.tblcontractsTableAdapter tblcontractsTableAdapter;
        private TestDatabaseDataSet3TableAdapters.tbldailycontractsettlementsTableAdapter tbldailycontractsettlementsTableAdapter;
        private Button buttonLoad;
        private RichTextBox richTextBoxLog;
        private DataGridViewTextBoxColumn contractnameDataGridViewTextBoxColumn;
        private ProgressBar progressBarLoad;
        private DataGridView dataGridViewContract;
        private Button buttonCancel;
        private TabControl tabControlOption;
        private TabPage tabPageOption;
        private DataGridView dataGridViewOption;
        private DataGridViewTextBoxColumn idoptionDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn optionnameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn optionmanthDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn optionmanthintDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn optionyearDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn strikepriceDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn callorputDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idinstrumentDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn expirationdateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idcontractDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn cqgsymbolDataGridViewTextBoxColumn;
        private BindingSource tbloptionsBindingSource;
        private TabPage tabPageOptionData;
        private DataGridView dataGridViewOptionData;
        private DataGridViewTextBoxColumn idoptiondataDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn idoptionDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn datetimeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn priceDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn impliedvolDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn timetoexpinyearDataGridViewTextBoxColumn;
        private BindingSource tbloptiondataBindingSource;
        private TestDatabaseDataSet1 testDatabaseDataSet1;
        private TabPage tabPageContract;
        private CheckBox checkBoxCheckDB;
        CancellationTokenSource cts;

        public DataBaseForm()
        {
            InitializeComponent();
            this.Resize += DataBaseForm_Resize;
            StaticData.ParseComplete += StaticData_ParseComplete;
        }

        private void DataBaseForm_Resize(object sender, EventArgs e)
        {
            tabControlOption.Size = new Size()
            {
                Width = this.Width - 15,
                Height = this.Height - 175
            };
            richTextBoxLog.Location = new Point()
            {
                X = 7,
                Y = tabControlOption.Height + 7 + 15 + 25
            };
            richTextBoxLog.Width = tabControlOption.Width - 15;
            richTextBoxLog.Height = 90;
            buttonLoad.Location = new Point()
            {
                X = buttonLoad.Location.X,
                Y = tabControlOption.Height + 5
            };
            checkBoxCheckDB.Location = new Point()
            {
                X = checkBoxCheckDB.Location.X,
                Y = tabControlOption.Height + 7
            };
            progressBarLoad.Location = new Point()
            {
                X = progressBarLoad.Location.X,
                Y = tabControlOption.Height + 7 + 25
            };
            progressBarLoad.Width = tabControlOption.Width - 15;
            buttonCancel.Location = new Point()
            {
                X = buttonCancel.Location.X,
                Y = tabControlOption.Height + 5
            };

        }

        private void DataBaseForm_Load(object sender, EventArgs e)
        {
            buttonCancel.Enabled = false;

            DataBaseForm_Resize(sender, e);

            // TODO: This line of code loads data into the 'testDatabaseDataSet3.tbldailycontractsettlements' table. You can move, or remove it, as needed.
            this.tbldailycontractsettlementsTableAdapter.Fill(this.testDatabaseDataSet3.tbldailycontractsettlements);
            // TODO: This line of code loads data into the 'testDatabaseDataSet2.tblcontracts' table. You can move, or remove it, as needed.
            this.tblcontractsTableAdapter.Fill(this.testDatabaseDataSet2.tblcontracts);
            // TODO: This line of code loads data into the 'testDatabaseDataSet1.tbloptiondata' table. You can move, or remove it, as needed.
            this.tbloptiondataTableAdapter.Fill(this.testDatabaseDataSet1.tbloptiondata);

            OptionsDataContext context = new OptionsDataContext();
            BindingSource bindingSourceBaners = new BindingSource();
            bindingSourceBaners.DataSource = (from item in context.tbloptions
                                              select item
                                              ).ToList();
            dataGridViewOption.DataSource = bindingSourceBaners;
            this.tbloptionsTableAdapter.Fill(this.testDatabaseDataSet.tbloptions);

            if (StaticData.records == null)
            {
                buttonLoad.Enabled = false;
            }
            else
            {
                richTextBoxLog.Text += "Count entities: " + StaticData.records.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.records.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private void StaticData_ParseComplete()
        {
            if (buttonLoad.Enabled != true)
            {
                richTextBoxLog.Text += "Count entities: " + StaticData.records.Length.ToString() + "\n";
                richTextBoxLog.Text += "Type of entity: " + StaticData.records.GetType().Name.Trim('[', ']') + "\n";
                buttonLoad.Enabled = true;
            }
        }

        private async Task GetDataEOD_Options_578(CancellationToken ct)
        {
            int count = 0;
            int number;
            int persent = (int.TryParse((StaticData.records.Length / 100).ToString(), out number)) ? number : 0;
            int currentPersent = 0;
            progressBarLoad.Minimum = 0;
            progressBarLoad.Maximum = StaticData.records.Length;

            try
            {
                OptionsDataContext context = new OptionsDataContext();
                buttonLoad.Enabled = false;
                buttonCancel.Enabled = true;

                foreach (EOD_Options_578 option in StaticData.records)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        await Task.Run(() =>
                        {
                            tbloption tableOption = new tbloption
                            {
                                idoption = long.Parse(count.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString()),
                                optionname = option.ProductName,
                                optionmanth = option.Date.Month.ToString().ToCharArray()[0],
                                optionmanthint = option.Date.Month,
                                optionyear = option.Date.Year,
                                strikeprice = (double)option.StrikePrice,
                                callorput = 'c',
                                idinstrument = 1,
                                expirationdate = DateTime.Now,
                                idcontract = 1,
                                cqgsymbol = "somesymbol"
                            };
                            context.tbloptions.InsertOnSubmit(tableOption);
                            context.SubmitChanges();
                            count++;
                        }, ct);
                        
                    }
                    catch (OperationCanceledException cancel)
                    {
                        richTextBoxLog.Text += cancel.Message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        richTextBoxLog.Text += "ERROR" + "\n";
                        richTextBoxLog.Text += ex.Message + "\n";
                        richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                        richTextBoxLog.ScrollToCaret();
                    }
                    finally
                    {
                        progressBarLoad.Value = (ct.IsCancellationRequested)? 0 : count;

                        if (count % (10 * persent) == 0)
                        {
                            currentPersent += 10;
                            richTextBoxLog.Text += "Current progress: " + currentPersent.ToString() + "% - " + count.ToString() + " entities" + "\n";
                            richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                            richTextBoxLog.ScrollToCaret();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBoxLog.Text += "ERROR" + "\n";
                richTextBoxLog.Text += ex.Message + "\n";
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
            finally
            {
                buttonLoad.Enabled = true;
                buttonCancel.Enabled = false;
                richTextBoxLog.Text += "Was loaded to DataBase " + count.ToString() + " entities" + "\n";
                if (StaticData.records.Length > count)
                {
                    richTextBoxLog.Text += "Can't add " + (StaticData.records.Length - count).ToString() + " to Db" + "\n";
                }
                richTextBoxLog.Select(richTextBoxLog.Text.Length, richTextBoxLog.Text.Length);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.idcontractDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.monthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.monthintDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yearDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idinstrumentDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.expirationdateDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cqgsymbolDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tblcontractsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.testDatabaseDataSet2 = new ICE_Import.TestDatabaseDataSet2();
            this.tabPageDailyContract = new System.Windows.Forms.TabPage();
            this.dataGridViewDailyContract = new System.Windows.Forms.DataGridView();
            this.iddailycontractsettlementsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idcontractDataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.settlementDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.volumeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openinterestDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbldailycontractsettlementsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.testDatabaseDataSet3 = new ICE_Import.TestDatabaseDataSet3();
            this.tbloptionsTableAdapter = new ICE_Import.TestDatabaseDataSetTableAdapters.tbloptionsTableAdapter();
            this.testDatabaseDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.testDatabaseDataSet = new ICE_Import.TestDatabaseDataSet();
            this.tbloptiondataTableAdapter = new ICE_Import.TestDatabaseDataSet1TableAdapters.tbloptiondataTableAdapter();
            this.tblcontractsTableAdapter = new ICE_Import.TestDatabaseDataSet2TableAdapters.tblcontractsTableAdapter();
            this.tbldailycontractsettlementsTableAdapter = new ICE_Import.TestDatabaseDataSet3TableAdapters.tbldailycontractsettlementsTableAdapter();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.contractnameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.progressBarLoad = new System.Windows.Forms.ProgressBar();
            this.dataGridViewContract = new System.Windows.Forms.DataGridView();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tabControlOption = new System.Windows.Forms.TabControl();
            this.tabPageOption = new System.Windows.Forms.TabPage();
            this.dataGridViewOption = new System.Windows.Forms.DataGridView();
            this.idoptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.optionnameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.optionmanthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.optionmanthintDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.optionyearDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.strikepriceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.callorputDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idinstrumentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.expirationdateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idcontractDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cqgsymbolDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbloptionsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabPageOptionData = new System.Windows.Forms.TabPage();
            this.dataGridViewOptionData = new System.Windows.Forms.DataGridView();
            this.idoptiondataDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idoptionDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datetimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.impliedvolDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timetoexpinyearDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbloptiondataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.testDatabaseDataSet1 = new ICE_Import.TestDatabaseDataSet1();
            this.tabPageContract = new System.Windows.Forms.TabPage();
            this.checkBoxCheckDB = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tblcontractsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet2)).BeginInit();
            this.tabPageDailyContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbldailycontractsettlementsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSetBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).BeginInit();
            this.tabControlOption.SuspendLayout();
            this.tabPageOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbloptionsBindingSource)).BeginInit();
            this.tabPageOptionData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbloptiondataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet1)).BeginInit();
            this.tabPageContract.SuspendLayout();
            this.SuspendLayout();
            // 
            // idcontractDataGridViewTextBoxColumn1
            // 
            this.idcontractDataGridViewTextBoxColumn1.DataPropertyName = "idcontract";
            this.idcontractDataGridViewTextBoxColumn1.HeaderText = "idcontract";
            this.idcontractDataGridViewTextBoxColumn1.Name = "idcontractDataGridViewTextBoxColumn1";
            // 
            // monthDataGridViewTextBoxColumn
            // 
            this.monthDataGridViewTextBoxColumn.DataPropertyName = "month";
            this.monthDataGridViewTextBoxColumn.HeaderText = "month";
            this.monthDataGridViewTextBoxColumn.Name = "monthDataGridViewTextBoxColumn";
            // 
            // monthintDataGridViewTextBoxColumn
            // 
            this.monthintDataGridViewTextBoxColumn.DataPropertyName = "monthint";
            this.monthintDataGridViewTextBoxColumn.HeaderText = "monthint";
            this.monthintDataGridViewTextBoxColumn.Name = "monthintDataGridViewTextBoxColumn";
            // 
            // yearDataGridViewTextBoxColumn
            // 
            this.yearDataGridViewTextBoxColumn.DataPropertyName = "year";
            this.yearDataGridViewTextBoxColumn.HeaderText = "year";
            this.yearDataGridViewTextBoxColumn.Name = "yearDataGridViewTextBoxColumn";
            // 
            // idinstrumentDataGridViewTextBoxColumn1
            // 
            this.idinstrumentDataGridViewTextBoxColumn1.DataPropertyName = "idinstrument";
            this.idinstrumentDataGridViewTextBoxColumn1.HeaderText = "idinstrument";
            this.idinstrumentDataGridViewTextBoxColumn1.Name = "idinstrumentDataGridViewTextBoxColumn1";
            // 
            // expirationdateDataGridViewTextBoxColumn1
            // 
            this.expirationdateDataGridViewTextBoxColumn1.DataPropertyName = "expirationdate";
            this.expirationdateDataGridViewTextBoxColumn1.HeaderText = "expirationdate";
            this.expirationdateDataGridViewTextBoxColumn1.Name = "expirationdateDataGridViewTextBoxColumn1";
            // 
            // cqgsymbolDataGridViewTextBoxColumn1
            // 
            this.cqgsymbolDataGridViewTextBoxColumn1.DataPropertyName = "cqgsymbol";
            this.cqgsymbolDataGridViewTextBoxColumn1.HeaderText = "cqgsymbol";
            this.cqgsymbolDataGridViewTextBoxColumn1.Name = "cqgsymbolDataGridViewTextBoxColumn1";
            // 
            // tblcontractsBindingSource
            // 
            this.tblcontractsBindingSource.DataMember = "tblcontracts";
            this.tblcontractsBindingSource.DataSource = this.testDatabaseDataSet2;
            // 
            // testDatabaseDataSet2
            // 
            this.testDatabaseDataSet2.DataSetName = "TestDatabaseDataSet2";
            this.testDatabaseDataSet2.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // tabPageDailyContract
            // 
            this.tabPageDailyContract.Controls.Add(this.dataGridViewDailyContract);
            this.tabPageDailyContract.Location = new System.Drawing.Point(4, 22);
            this.tabPageDailyContract.Name = "tabPageDailyContract";
            this.tabPageDailyContract.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDailyContract.Size = new System.Drawing.Size(654, 225);
            this.tabPageDailyContract.TabIndex = 3;
            this.tabPageDailyContract.Text = "DailyContract";
            this.tabPageDailyContract.UseVisualStyleBackColor = true;
            // 
            // dataGridViewDailyContract
            // 
            this.dataGridViewDailyContract.AutoGenerateColumns = false;
            this.dataGridViewDailyContract.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDailyContract.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iddailycontractsettlementsDataGridViewTextBoxColumn,
            this.idcontractDataGridViewTextBoxColumn2,
            this.dateDataGridViewTextBoxColumn,
            this.settlementDataGridViewTextBoxColumn,
            this.volumeDataGridViewTextBoxColumn,
            this.openinterestDataGridViewTextBoxColumn});
            this.dataGridViewDailyContract.DataSource = this.tbldailycontractsettlementsBindingSource;
            this.dataGridViewDailyContract.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDailyContract.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewDailyContract.Name = "dataGridViewDailyContract";
            this.dataGridViewDailyContract.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewDailyContract.TabIndex = 0;
            // 
            // iddailycontractsettlementsDataGridViewTextBoxColumn
            // 
            this.iddailycontractsettlementsDataGridViewTextBoxColumn.DataPropertyName = "iddailycontractsettlements";
            this.iddailycontractsettlementsDataGridViewTextBoxColumn.HeaderText = "iddailycontractsettlements";
            this.iddailycontractsettlementsDataGridViewTextBoxColumn.Name = "iddailycontractsettlementsDataGridViewTextBoxColumn";
            // 
            // idcontractDataGridViewTextBoxColumn2
            // 
            this.idcontractDataGridViewTextBoxColumn2.DataPropertyName = "idcontract";
            this.idcontractDataGridViewTextBoxColumn2.HeaderText = "idcontract";
            this.idcontractDataGridViewTextBoxColumn2.Name = "idcontractDataGridViewTextBoxColumn2";
            // 
            // dateDataGridViewTextBoxColumn
            // 
            this.dateDataGridViewTextBoxColumn.DataPropertyName = "date";
            this.dateDataGridViewTextBoxColumn.HeaderText = "date";
            this.dateDataGridViewTextBoxColumn.Name = "dateDataGridViewTextBoxColumn";
            // 
            // settlementDataGridViewTextBoxColumn
            // 
            this.settlementDataGridViewTextBoxColumn.DataPropertyName = "settlement";
            this.settlementDataGridViewTextBoxColumn.HeaderText = "settlement";
            this.settlementDataGridViewTextBoxColumn.Name = "settlementDataGridViewTextBoxColumn";
            // 
            // volumeDataGridViewTextBoxColumn
            // 
            this.volumeDataGridViewTextBoxColumn.DataPropertyName = "volume";
            this.volumeDataGridViewTextBoxColumn.HeaderText = "volume";
            this.volumeDataGridViewTextBoxColumn.Name = "volumeDataGridViewTextBoxColumn";
            // 
            // openinterestDataGridViewTextBoxColumn
            // 
            this.openinterestDataGridViewTextBoxColumn.DataPropertyName = "openinterest";
            this.openinterestDataGridViewTextBoxColumn.HeaderText = "openinterest";
            this.openinterestDataGridViewTextBoxColumn.Name = "openinterestDataGridViewTextBoxColumn";
            // 
            // tbldailycontractsettlementsBindingSource
            // 
            this.tbldailycontractsettlementsBindingSource.DataMember = "tbldailycontractsettlements";
            this.tbldailycontractsettlementsBindingSource.DataSource = this.testDatabaseDataSet3;
            // 
            // testDatabaseDataSet3
            // 
            this.testDatabaseDataSet3.DataSetName = "TestDatabaseDataSet3";
            this.testDatabaseDataSet3.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // tbloptionsTableAdapter
            // 
            this.tbloptionsTableAdapter.ClearBeforeFill = true;
            // 
            // testDatabaseDataSetBindingSource
            // 
            this.testDatabaseDataSetBindingSource.DataSource = this.testDatabaseDataSet;
            this.testDatabaseDataSetBindingSource.Position = 0;
            // 
            // testDatabaseDataSet
            // 
            this.testDatabaseDataSet.DataSetName = "TestDatabaseDataSet";
            this.testDatabaseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // tbloptiondataTableAdapter
            // 
            this.tbloptiondataTableAdapter.ClearBeforeFill = true;
            // 
            // tblcontractsTableAdapter
            // 
            this.tblcontractsTableAdapter.ClearBeforeFill = true;
            // 
            // tbldailycontractsettlementsTableAdapter
            // 
            this.tbldailycontractsettlementsTableAdapter.ClearBeforeFill = true;
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(7, 254);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(138, 23);
            this.buttonLoad.TabIndex = 8;
            this.buttonLoad.Text = "Load data to DataBase";
            this.buttonLoad.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.BackColor = System.Drawing.SystemColors.InfoText;
            this.richTextBoxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLog.Location = new System.Drawing.Point(4, 295);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(654, 90);
            this.richTextBoxLog.TabIndex = 7;
            this.richTextBoxLog.Text = "";
            // 
            // contractnameDataGridViewTextBoxColumn
            // 
            this.contractnameDataGridViewTextBoxColumn.DataPropertyName = "contractname";
            this.contractnameDataGridViewTextBoxColumn.HeaderText = "contractname";
            this.contractnameDataGridViewTextBoxColumn.Name = "contractnameDataGridViewTextBoxColumn";
            // 
            // progressBarLoad
            // 
            this.progressBarLoad.Location = new System.Drawing.Point(7, 281);
            this.progressBarLoad.Name = "progressBarLoad";
            this.progressBarLoad.Size = new System.Drawing.Size(648, 10);
            this.progressBarLoad.TabIndex = 10;
            // 
            // dataGridViewContract
            // 
            this.dataGridViewContract.AutoGenerateColumns = false;
            this.dataGridViewContract.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewContract.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idcontractDataGridViewTextBoxColumn1,
            this.contractnameDataGridViewTextBoxColumn,
            this.monthDataGridViewTextBoxColumn,
            this.monthintDataGridViewTextBoxColumn,
            this.yearDataGridViewTextBoxColumn,
            this.idinstrumentDataGridViewTextBoxColumn1,
            this.expirationdateDataGridViewTextBoxColumn1,
            this.cqgsymbolDataGridViewTextBoxColumn1});
            this.dataGridViewContract.DataSource = this.tblcontractsBindingSource;
            this.dataGridViewContract.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewContract.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewContract.Name = "dataGridViewContract";
            this.dataGridViewContract.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewContract.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(152, 253);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(82, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // tabControlOption
            // 
            this.tabControlOption.Controls.Add(this.tabPageOption);
            this.tabControlOption.Controls.Add(this.tabPageOptionData);
            this.tabControlOption.Controls.Add(this.tabPageContract);
            this.tabControlOption.Controls.Add(this.tabPageDailyContract);
            this.tabControlOption.Location = new System.Drawing.Point(0, 1);
            this.tabControlOption.Name = "tabControlOption";
            this.tabControlOption.SelectedIndex = 0;
            this.tabControlOption.Size = new System.Drawing.Size(662, 251);
            this.tabControlOption.TabIndex = 6;
            // 
            // tabPageOption
            // 
            this.tabPageOption.Controls.Add(this.dataGridViewOption);
            this.tabPageOption.Location = new System.Drawing.Point(4, 22);
            this.tabPageOption.Name = "tabPageOption";
            this.tabPageOption.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOption.Size = new System.Drawing.Size(654, 225);
            this.tabPageOption.TabIndex = 0;
            this.tabPageOption.Text = "Option";
            this.tabPageOption.UseVisualStyleBackColor = true;
            // 
            // dataGridViewOption
            // 
            this.dataGridViewOption.AutoGenerateColumns = false;
            this.dataGridViewOption.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOption.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idoptionDataGridViewTextBoxColumn,
            this.optionnameDataGridViewTextBoxColumn,
            this.optionmanthDataGridViewTextBoxColumn,
            this.optionmanthintDataGridViewTextBoxColumn,
            this.optionyearDataGridViewTextBoxColumn,
            this.strikepriceDataGridViewTextBoxColumn,
            this.callorputDataGridViewTextBoxColumn,
            this.idinstrumentDataGridViewTextBoxColumn,
            this.expirationdateDataGridViewTextBoxColumn,
            this.idcontractDataGridViewTextBoxColumn,
            this.cqgsymbolDataGridViewTextBoxColumn});
            this.dataGridViewOption.DataSource = this.tbloptionsBindingSource;
            this.dataGridViewOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOption.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewOption.Name = "dataGridViewOption";
            this.dataGridViewOption.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewOption.TabIndex = 0;
            // 
            // idoptionDataGridViewTextBoxColumn
            // 
            this.idoptionDataGridViewTextBoxColumn.DataPropertyName = "idoption";
            this.idoptionDataGridViewTextBoxColumn.HeaderText = "idoption";
            this.idoptionDataGridViewTextBoxColumn.Name = "idoptionDataGridViewTextBoxColumn";
            // 
            // optionnameDataGridViewTextBoxColumn
            // 
            this.optionnameDataGridViewTextBoxColumn.DataPropertyName = "optionname";
            this.optionnameDataGridViewTextBoxColumn.HeaderText = "optionname";
            this.optionnameDataGridViewTextBoxColumn.Name = "optionnameDataGridViewTextBoxColumn";
            // 
            // optionmanthDataGridViewTextBoxColumn
            // 
            this.optionmanthDataGridViewTextBoxColumn.DataPropertyName = "optionmanth";
            this.optionmanthDataGridViewTextBoxColumn.HeaderText = "optionmanth";
            this.optionmanthDataGridViewTextBoxColumn.Name = "optionmanthDataGridViewTextBoxColumn";
            // 
            // optionmanthintDataGridViewTextBoxColumn
            // 
            this.optionmanthintDataGridViewTextBoxColumn.DataPropertyName = "optionmanthint";
            this.optionmanthintDataGridViewTextBoxColumn.HeaderText = "optionmanthint";
            this.optionmanthintDataGridViewTextBoxColumn.Name = "optionmanthintDataGridViewTextBoxColumn";
            // 
            // optionyearDataGridViewTextBoxColumn
            // 
            this.optionyearDataGridViewTextBoxColumn.DataPropertyName = "optionyear";
            this.optionyearDataGridViewTextBoxColumn.HeaderText = "optionyear";
            this.optionyearDataGridViewTextBoxColumn.Name = "optionyearDataGridViewTextBoxColumn";
            // 
            // strikepriceDataGridViewTextBoxColumn
            // 
            this.strikepriceDataGridViewTextBoxColumn.DataPropertyName = "strikeprice";
            this.strikepriceDataGridViewTextBoxColumn.HeaderText = "strikeprice";
            this.strikepriceDataGridViewTextBoxColumn.Name = "strikepriceDataGridViewTextBoxColumn";
            // 
            // callorputDataGridViewTextBoxColumn
            // 
            this.callorputDataGridViewTextBoxColumn.DataPropertyName = "callorput";
            this.callorputDataGridViewTextBoxColumn.HeaderText = "callorput";
            this.callorputDataGridViewTextBoxColumn.Name = "callorputDataGridViewTextBoxColumn";
            // 
            // idinstrumentDataGridViewTextBoxColumn
            // 
            this.idinstrumentDataGridViewTextBoxColumn.DataPropertyName = "idinstrument";
            this.idinstrumentDataGridViewTextBoxColumn.HeaderText = "idinstrument";
            this.idinstrumentDataGridViewTextBoxColumn.Name = "idinstrumentDataGridViewTextBoxColumn";
            // 
            // expirationdateDataGridViewTextBoxColumn
            // 
            this.expirationdateDataGridViewTextBoxColumn.DataPropertyName = "expirationdate";
            this.expirationdateDataGridViewTextBoxColumn.HeaderText = "expirationdate";
            this.expirationdateDataGridViewTextBoxColumn.Name = "expirationdateDataGridViewTextBoxColumn";
            // 
            // idcontractDataGridViewTextBoxColumn
            // 
            this.idcontractDataGridViewTextBoxColumn.DataPropertyName = "idcontract";
            this.idcontractDataGridViewTextBoxColumn.HeaderText = "idcontract";
            this.idcontractDataGridViewTextBoxColumn.Name = "idcontractDataGridViewTextBoxColumn";
            // 
            // cqgsymbolDataGridViewTextBoxColumn
            // 
            this.cqgsymbolDataGridViewTextBoxColumn.DataPropertyName = "cqgsymbol";
            this.cqgsymbolDataGridViewTextBoxColumn.HeaderText = "cqgsymbol";
            this.cqgsymbolDataGridViewTextBoxColumn.Name = "cqgsymbolDataGridViewTextBoxColumn";
            // 
            // tbloptionsBindingSource
            // 
            this.tbloptionsBindingSource.DataMember = "tbloptions";
            this.tbloptionsBindingSource.DataSource = this.testDatabaseDataSet;
            // 
            // tabPageOptionData
            // 
            this.tabPageOptionData.Controls.Add(this.dataGridViewOptionData);
            this.tabPageOptionData.Location = new System.Drawing.Point(4, 22);
            this.tabPageOptionData.Name = "tabPageOptionData";
            this.tabPageOptionData.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptionData.Size = new System.Drawing.Size(654, 225);
            this.tabPageOptionData.TabIndex = 1;
            this.tabPageOptionData.Text = "OptionData";
            this.tabPageOptionData.UseVisualStyleBackColor = true;
            // 
            // dataGridViewOptionData
            // 
            this.dataGridViewOptionData.AutoGenerateColumns = false;
            this.dataGridViewOptionData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOptionData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idoptiondataDataGridViewTextBoxColumn,
            this.idoptionDataGridViewTextBoxColumn1,
            this.datetimeDataGridViewTextBoxColumn,
            this.priceDataGridViewTextBoxColumn,
            this.impliedvolDataGridViewTextBoxColumn,
            this.timetoexpinyearDataGridViewTextBoxColumn});
            this.dataGridViewOptionData.DataSource = this.tbloptiondataBindingSource;
            this.dataGridViewOptionData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOptionData.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewOptionData.Name = "dataGridViewOptionData";
            this.dataGridViewOptionData.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewOptionData.TabIndex = 0;
            // 
            // idoptiondataDataGridViewTextBoxColumn
            // 
            this.idoptiondataDataGridViewTextBoxColumn.DataPropertyName = "idoptiondata";
            this.idoptiondataDataGridViewTextBoxColumn.HeaderText = "idoptiondata";
            this.idoptiondataDataGridViewTextBoxColumn.Name = "idoptiondataDataGridViewTextBoxColumn";
            // 
            // idoptionDataGridViewTextBoxColumn1
            // 
            this.idoptionDataGridViewTextBoxColumn1.DataPropertyName = "idoption";
            this.idoptionDataGridViewTextBoxColumn1.HeaderText = "idoption";
            this.idoptionDataGridViewTextBoxColumn1.Name = "idoptionDataGridViewTextBoxColumn1";
            // 
            // datetimeDataGridViewTextBoxColumn
            // 
            this.datetimeDataGridViewTextBoxColumn.DataPropertyName = "datetime";
            this.datetimeDataGridViewTextBoxColumn.HeaderText = "datetime";
            this.datetimeDataGridViewTextBoxColumn.Name = "datetimeDataGridViewTextBoxColumn";
            // 
            // priceDataGridViewTextBoxColumn
            // 
            this.priceDataGridViewTextBoxColumn.DataPropertyName = "price";
            this.priceDataGridViewTextBoxColumn.HeaderText = "price";
            this.priceDataGridViewTextBoxColumn.Name = "priceDataGridViewTextBoxColumn";
            // 
            // impliedvolDataGridViewTextBoxColumn
            // 
            this.impliedvolDataGridViewTextBoxColumn.DataPropertyName = "impliedvol";
            this.impliedvolDataGridViewTextBoxColumn.HeaderText = "impliedvol";
            this.impliedvolDataGridViewTextBoxColumn.Name = "impliedvolDataGridViewTextBoxColumn";
            // 
            // timetoexpinyearDataGridViewTextBoxColumn
            // 
            this.timetoexpinyearDataGridViewTextBoxColumn.DataPropertyName = "timetoexpinyear";
            this.timetoexpinyearDataGridViewTextBoxColumn.HeaderText = "timetoexpinyear";
            this.timetoexpinyearDataGridViewTextBoxColumn.Name = "timetoexpinyearDataGridViewTextBoxColumn";
            // 
            // tbloptiondataBindingSource
            // 
            this.tbloptiondataBindingSource.DataMember = "tbloptiondata";
            this.tbloptiondataBindingSource.DataSource = this.testDatabaseDataSet1;
            // 
            // testDatabaseDataSet1
            // 
            this.testDatabaseDataSet1.DataSetName = "TestDatabaseDataSet1";
            this.testDatabaseDataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // tabPageContract
            // 
            this.tabPageContract.Controls.Add(this.dataGridViewContract);
            this.tabPageContract.Location = new System.Drawing.Point(4, 22);
            this.tabPageContract.Name = "tabPageContract";
            this.tabPageContract.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageContract.Size = new System.Drawing.Size(654, 225);
            this.tabPageContract.TabIndex = 2;
            this.tabPageContract.Text = "Contract";
            this.tabPageContract.UseVisualStyleBackColor = true;
            // 
            // checkBoxCheckDB
            // 
            this.checkBoxCheckDB.AutoSize = true;
            this.checkBoxCheckDB.Checked = true;
            this.checkBoxCheckDB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCheckDB.Location = new System.Drawing.Point(240, 257);
            this.checkBoxCheckDB.Name = "checkBoxCheckDB";
            this.checkBoxCheckDB.Size = new System.Drawing.Size(102, 17);
            this.checkBoxCheckDB.TabIndex = 9;
            this.checkBoxCheckDB.Text = "Local DataBase";
            this.checkBoxCheckDB.UseVisualStyleBackColor = true;
            // 
            // DataBaseForm
            // 
            this.ClientSize = new System.Drawing.Size(663, 387);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.progressBarLoad);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControlOption);
            this.Controls.Add(this.checkBoxCheckDB);
            this.Name = "DataBaseForm";
            ((System.ComponentModel.ISupportInitialize)(this.tblcontractsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet2)).EndInit();
            this.tabPageDailyContract.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbldailycontractsettlementsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSetBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).EndInit();
            this.tabControlOption.ResumeLayout(false);
            this.tabPageOption.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbloptionsBindingSource)).EndInit();
            this.tabPageOptionData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbloptiondataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.testDatabaseDataSet1)).EndInit();
            this.tabPageContract.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
