namespace ICE_Import
{
    partial class FormDB
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.buttonPush = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPull = new System.Windows.Forms.Button();
            this.tabPageDailyContract = new System.Windows.Forms.TabPage();
            this.dataGridViewDailyContract = new System.Windows.Forms.DataGridView();
            this.tabPageContract = new System.Windows.Forms.TabPage();
            this.dataGridViewContract = new System.Windows.Forms.DataGridView();
            this.tabPageOptionData = new System.Windows.Forms.TabPage();
            this.dataGridViewOptionData = new System.Windows.Forms.DataGridView();
            this.tabPageOption = new System.Windows.Forms.TabPage();
            this.dataGridViewOption = new System.Windows.Forms.DataGridView();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.buttonToCSV = new System.Windows.Forms.Button();
            this.cb_StoredProcs = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_TMLDB = new System.Windows.Forms.RadioButton();
            this.rb_TMLDBCopy = new System.Windows.Forms.RadioButton();
            this.rb_LocalDB = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_TestTables = new System.Windows.Forms.CheckBox();
            this.cb_AsyncUpdate = new System.Windows.Forms.CheckBox();
            this.labelRPS1 = new System.Windows.Forms.Label();
            this.labelRPS2 = new System.Windows.Forms.Label();
            this.buttonCheckPushedData = new System.Windows.Forms.Button();
            this.tabPageDailyContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).BeginInit();
            this.tabPageContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).BeginInit();
            this.tabPageOptionData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).BeginInit();
            this.tabPageOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).BeginInit();
            this.tabControl.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.BackColor = System.Drawing.SystemColors.InfoText;
            this.richTextBoxLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.richTextBoxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLog.Location = new System.Drawing.Point(0, 375);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(664, 90);
            this.richTextBoxLog.TabIndex = 12;
            this.richTextBoxLog.Text = "";
            // 
            // buttonPush
            // 
            this.buttonPush.Location = new System.Drawing.Point(276, 257);
            this.buttonPush.Name = "buttonPush";
            this.buttonPush.Size = new System.Drawing.Size(85, 23);
            this.buttonPush.TabIndex = 4;
            this.buttonPush.Text = "Push to DB";
            this.buttonPush.UseVisualStyleBackColor = true;
            this.buttonPush.Click += new System.EventHandler(this.buttonPush_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 358);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(640, 10);
            this.progressBar.TabIndex = 11;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(276, 329);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(85, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonPull
            // 
            this.buttonPull.Location = new System.Drawing.Point(276, 286);
            this.buttonPull.Name = "buttonPull";
            this.buttonPull.Size = new System.Drawing.Size(85, 23);
            this.buttonPull.TabIndex = 5;
            this.buttonPull.Text = "Pull from DB";
            this.buttonPull.UseVisualStyleBackColor = true;
            this.buttonPull.Click += new System.EventHandler(this.buttonPull_Click);
            // 
            // tabPageDailyContract
            // 
            this.tabPageDailyContract.Controls.Add(this.dataGridViewDailyContract);
            this.tabPageDailyContract.Location = new System.Drawing.Point(4, 22);
            this.tabPageDailyContract.Name = "tabPageDailyContract";
            this.tabPageDailyContract.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDailyContract.Size = new System.Drawing.Size(654, 225);
            this.tabPageDailyContract.TabIndex = 1;
            this.tabPageDailyContract.Text = "tbldailycontractsettlements";
            this.tabPageDailyContract.UseVisualStyleBackColor = true;
            // 
            // dataGridViewDailyContract
            // 
            this.dataGridViewDailyContract.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDailyContract.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewDailyContract.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewDailyContract.Name = "dataGridViewDailyContract";
            this.dataGridViewDailyContract.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewDailyContract.TabIndex = 0;
            // 
            // tabPageContract
            // 
            this.tabPageContract.Controls.Add(this.dataGridViewContract);
            this.tabPageContract.Location = new System.Drawing.Point(4, 22);
            this.tabPageContract.Name = "tabPageContract";
            this.tabPageContract.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageContract.Size = new System.Drawing.Size(654, 225);
            this.tabPageContract.TabIndex = 0;
            this.tabPageContract.Text = "tblcontracts";
            this.tabPageContract.UseVisualStyleBackColor = true;
            // 
            // dataGridViewContract
            // 
            this.dataGridViewContract.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewContract.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewContract.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewContract.Name = "dataGridViewContract";
            this.dataGridViewContract.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewContract.TabIndex = 1;
            // 
            // tabPageOptionData
            // 
            this.tabPageOptionData.Controls.Add(this.dataGridViewOptionData);
            this.tabPageOptionData.Location = new System.Drawing.Point(4, 22);
            this.tabPageOptionData.Name = "tabPageOptionData";
            this.tabPageOptionData.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOptionData.Size = new System.Drawing.Size(654, 225);
            this.tabPageOptionData.TabIndex = 3;
            this.tabPageOptionData.Text = "tbloptiondata";
            this.tabPageOptionData.UseVisualStyleBackColor = true;
            // 
            // dataGridViewOptionData
            // 
            this.dataGridViewOptionData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOptionData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOptionData.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewOptionData.Name = "dataGridViewOptionData";
            this.dataGridViewOptionData.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewOptionData.TabIndex = 0;
            // 
            // tabPageOption
            // 
            this.tabPageOption.Controls.Add(this.dataGridViewOption);
            this.tabPageOption.Location = new System.Drawing.Point(4, 22);
            this.tabPageOption.Name = "tabPageOption";
            this.tabPageOption.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageOption.Size = new System.Drawing.Size(654, 225);
            this.tabPageOption.TabIndex = 3;
            this.tabPageOption.Text = "tbloption";
            this.tabPageOption.UseVisualStyleBackColor = true;
            // 
            // dataGridViewOption
            // 
            this.dataGridViewOption.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOption.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewOption.Name = "dataGridViewOption";
            this.dataGridViewOption.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewOption.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageContract);
            this.tabControl.Controls.Add(this.tabPageDailyContract);
            this.tabControl.Controls.Add(this.tabPageOption);
            this.tabControl.Controls.Add(this.tabPageOptionData);
            this.tabControl.Location = new System.Drawing.Point(-1, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(662, 251);
            this.tabControl.TabIndex = 0;
            // 
            // buttonToCSV
            // 
            this.buttonToCSV.Location = new System.Drawing.Point(567, 257);
            this.buttonToCSV.Name = "buttonToCSV";
            this.buttonToCSV.Size = new System.Drawing.Size(85, 23);
            this.buttonToCSV.TabIndex = 8;
            this.buttonToCSV.Text = "To CSV Form";
            this.buttonToCSV.UseVisualStyleBackColor = true;
            this.buttonToCSV.Click += new System.EventHandler(this.buttonToCSV_Click);
            // 
            // cb_StoredProcs
            // 
            this.cb_StoredProcs.AutoSize = true;
            this.cb_StoredProcs.Checked = true;
            this.cb_StoredProcs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_StoredProcs.Location = new System.Drawing.Point(6, 44);
            this.cb_StoredProcs.Name = "cb_StoredProcs";
            this.cb_StoredProcs.Size = new System.Drawing.Size(136, 17);
            this.cb_StoredProcs.TabIndex = 1;
            this.cb_StoredProcs.Text = "Use Stored Procedures";
            this.cb_StoredProcs.UseVisualStyleBackColor = true;
            this.cb_StoredProcs.CheckedChanged += new System.EventHandler(this.cb_StoredProcs_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_TMLDB);
            this.groupBox1.Controls.Add(this.rb_TMLDBCopy);
            this.groupBox1.Controls.Add(this.rb_LocalDB);
            this.groupBox1.Location = new System.Drawing.Point(12, 257);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(104, 95);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database";
            // 
            // rb_TMLDB
            // 
            this.rb_TMLDB.AutoSize = true;
            this.rb_TMLDB.Location = new System.Drawing.Point(7, 66);
            this.rb_TMLDB.Name = "rb_TMLDB";
            this.rb_TMLDB.Size = new System.Drawing.Size(62, 17);
            this.rb_TMLDB.TabIndex = 2;
            this.rb_TMLDB.Tag = "3";
            this.rb_TMLDB.Text = "TMLDB";
            this.rb_TMLDB.UseVisualStyleBackColor = true;
            this.rb_TMLDB.CheckedChanged += new System.EventHandler(this.rb_DB_CheckedChanged);
            // 
            // rb_TMLDBCopy
            // 
            this.rb_TMLDBCopy.AutoSize = true;
            this.rb_TMLDBCopy.Location = new System.Drawing.Point(7, 43);
            this.rb_TMLDBCopy.Name = "rb_TMLDBCopy";
            this.rb_TMLDBCopy.Size = new System.Drawing.Size(92, 17);
            this.rb_TMLDBCopy.TabIndex = 1;
            this.rb_TMLDBCopy.Tag = "2";
            this.rb_TMLDBCopy.Text = "TMLDB_Copy";
            this.rb_TMLDBCopy.UseVisualStyleBackColor = true;
            this.rb_TMLDBCopy.CheckedChanged += new System.EventHandler(this.rb_DB_CheckedChanged);
            // 
            // rb_LocalDB
            // 
            this.rb_LocalDB.AutoSize = true;
            this.rb_LocalDB.Checked = true;
            this.rb_LocalDB.Location = new System.Drawing.Point(7, 20);
            this.rb_LocalDB.Name = "rb_LocalDB";
            this.rb_LocalDB.Size = new System.Drawing.Size(51, 17);
            this.rb_LocalDB.TabIndex = 0;
            this.rb_LocalDB.TabStop = true;
            this.rb_LocalDB.Tag = "1";
            this.rb_LocalDB.Text = "Local";
            this.rb_LocalDB.UseVisualStyleBackColor = true;
            this.rb_LocalDB.CheckedChanged += new System.EventHandler(this.rb_DB_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cb_TestTables);
            this.groupBox2.Controls.Add(this.cb_StoredProcs);
            this.groupBox2.Controls.Add(this.cb_AsyncUpdate);
            this.groupBox2.Location = new System.Drawing.Point(122, 257);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(148, 95);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // cb_TestTables
            // 
            this.cb_TestTables.AutoSize = true;
            this.cb_TestTables.Checked = true;
            this.cb_TestTables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_TestTables.Location = new System.Drawing.Point(6, 21);
            this.cb_TestTables.Name = "cb_TestTables";
            this.cb_TestTables.Size = new System.Drawing.Size(116, 17);
            this.cb_TestTables.TabIndex = 0;
            this.cb_TestTables.Text = "Use \"test_\" Tables";
            this.cb_TestTables.UseVisualStyleBackColor = true;
            this.cb_TestTables.CheckedChanged += new System.EventHandler(this.cb_TestTables_CheckedChanged);
            // 
            // cb_AsyncUpdate
            // 
            this.cb_AsyncUpdate.AutoSize = true;
            this.cb_AsyncUpdate.Checked = true;
            this.cb_AsyncUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_AsyncUpdate.Location = new System.Drawing.Point(6, 67);
            this.cb_AsyncUpdate.Name = "cb_AsyncUpdate";
            this.cb_AsyncUpdate.Size = new System.Drawing.Size(110, 17);
            this.cb_AsyncUpdate.TabIndex = 2;
            this.cb_AsyncUpdate.Text = "Do Async Update";
            this.cb_AsyncUpdate.UseVisualStyleBackColor = true;
            this.cb_AsyncUpdate.CheckedChanged += new System.EventHandler(this.cb_AsyncUpdate_CheckedChanged);
            // 
            // labelRPS1
            // 
            this.labelRPS1.AutoSize = true;
            this.labelRPS1.Location = new System.Drawing.Point(577, 334);
            this.labelRPS1.Name = "labelRPS1";
            this.labelRPS1.Size = new System.Drawing.Size(32, 13);
            this.labelRPS1.TabIndex = 9;
            this.labelRPS1.Text = "RPS:";
            // 
            // labelRPS2
            // 
            this.labelRPS2.AutoSize = true;
            this.labelRPS2.Location = new System.Drawing.Point(615, 334);
            this.labelRPS2.Name = "labelRPS2";
            this.labelRPS2.Size = new System.Drawing.Size(0, 13);
            this.labelRPS2.TabIndex = 10;
            // 
            // buttonCheckPushedData
            // 
            this.buttonCheckPushedData.Location = new System.Drawing.Point(367, 257);
            this.buttonCheckPushedData.Name = "buttonCheckPushedData";
            this.buttonCheckPushedData.Size = new System.Drawing.Size(115, 23);
            this.buttonCheckPushedData.TabIndex = 7;
            this.buttonCheckPushedData.Text = "Check Pushed Data";
            this.buttonCheckPushedData.UseVisualStyleBackColor = true;
            this.buttonCheckPushedData.Click += new System.EventHandler(this.buttonCheckPushedData_Click);
            // 
            // FormDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 465);
            this.Controls.Add(this.buttonCheckPushedData);
            this.Controls.Add(this.labelRPS1);
            this.Controls.Add(this.labelRPS2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonToCSV);
            this.Controls.Add(this.buttonPull);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonPush);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(680, 504);
            this.Name = "FormDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ICE Import (DB Form)";
            this.Load += new System.EventHandler(this.FormDB_Load);
            this.tabPageDailyContract.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).EndInit();
            this.tabPageContract.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).EndInit();
            this.tabPageOptionData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).EndInit();
            this.tabPageOption.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Button buttonPush;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonPull;
        private System.Windows.Forms.TabPage tabPageDailyContract;
        private System.Windows.Forms.DataGridView dataGridViewDailyContract;
        private System.Windows.Forms.TabPage tabPageContract;
        private System.Windows.Forms.DataGridView dataGridViewContract;
        private System.Windows.Forms.TabPage tabPageOptionData;
        private System.Windows.Forms.DataGridView dataGridViewOptionData;
        private System.Windows.Forms.TabPage tabPageOption;
        private System.Windows.Forms.DataGridView dataGridViewOption;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Button buttonToCSV;
        private System.Windows.Forms.CheckBox cb_StoredProcs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_TMLDB;
        private System.Windows.Forms.RadioButton rb_TMLDBCopy;
        private System.Windows.Forms.RadioButton rb_LocalDB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cb_TestTables;
        private System.Windows.Forms.CheckBox cb_AsyncUpdate;
        private System.Windows.Forms.Label labelRPS1;
        private System.Windows.Forms.Label labelRPS2;
        private System.Windows.Forms.Button buttonCheckPushedData;
    }
}