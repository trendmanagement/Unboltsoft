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
            this.checkBoxLocalDB = new System.Windows.Forms.CheckBox();
            this.progressBarLoad = new System.Windows.Forms.ProgressBar();
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
            this.tabControlOption = new System.Windows.Forms.TabControl();
            this.buttonToCSV = new System.Windows.Forms.Button();
            this.checkBoxUseSP = new System.Windows.Forms.CheckBox();
            this.tabPageDailyContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).BeginInit();
            this.tabPageContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).BeginInit();
            this.tabPageOptionData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).BeginInit();
            this.tabPageOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).BeginInit();
            this.tabControlOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.BackColor = System.Drawing.SystemColors.InfoText;
            this.richTextBoxLog.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.richTextBoxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLog.Location = new System.Drawing.Point(0, 296);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(664, 90);
            this.richTextBoxLog.TabIndex = 1;
            this.richTextBoxLog.Text = "";
            // 
            // buttonPush
            // 
            this.buttonPush.Location = new System.Drawing.Point(12, 253);
            this.buttonPush.Name = "buttonPush";
            this.buttonPush.Size = new System.Drawing.Size(95, 23);
            this.buttonPush.TabIndex = 2;
            this.buttonPush.Text = "Push to DB";
            this.buttonPush.UseVisualStyleBackColor = true;
            this.buttonPush.Click += new System.EventHandler(this.buttonPush_Click);
            // 
            // checkBoxLocalDB
            // 
            this.checkBoxLocalDB.AutoSize = true;
            this.checkBoxLocalDB.Checked = true;
            this.checkBoxLocalDB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalDB.Location = new System.Drawing.Point(309, 257);
            this.checkBoxLocalDB.Name = "checkBoxLocalDB";
            this.checkBoxLocalDB.Size = new System.Drawing.Size(70, 17);
            this.checkBoxLocalDB.TabIndex = 3;
            this.checkBoxLocalDB.Text = "Local DB";
            this.checkBoxLocalDB.UseVisualStyleBackColor = true;
            this.checkBoxLocalDB.CheckedChanged += new System.EventHandler(this.checkBoxLocalDB_CheckedChanged);
            // 
            // progressBarLoad
            // 
            this.progressBarLoad.Location = new System.Drawing.Point(6, 280);
            this.progressBarLoad.Name = "progressBarLoad";
            this.progressBarLoad.Size = new System.Drawing.Size(646, 10);
            this.progressBarLoad.TabIndex = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(221, 253);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(82, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonPull
            // 
            this.buttonPull.Location = new System.Drawing.Point(113, 253);
            this.buttonPull.Name = "buttonPull";
            this.buttonPull.Size = new System.Drawing.Size(102, 23);
            this.buttonPull.TabIndex = 6;
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
            this.dataGridViewContract.TabIndex = 0;
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
            // tabControlOption
            // 
            this.tabControlOption.Controls.Add(this.tabPageContract);
            this.tabControlOption.Controls.Add(this.tabPageDailyContract);
            this.tabControlOption.Controls.Add(this.tabPageOption);
            this.tabControlOption.Controls.Add(this.tabPageOptionData);
            this.tabControlOption.Location = new System.Drawing.Point(-1, 0);
            this.tabControlOption.Name = "tabControlOption";
            this.tabControlOption.SelectedIndex = 0;
            this.tabControlOption.Size = new System.Drawing.Size(662, 251);
            this.tabControlOption.TabIndex = 0;
            // 
            // buttonToCSV
            // 
            this.buttonToCSV.Location = new System.Drawing.Point(570, 253);
            this.buttonToCSV.Name = "buttonToCSV";
            this.buttonToCSV.Size = new System.Drawing.Size(82, 23);
            this.buttonToCSV.TabIndex = 8;
            this.buttonToCSV.Text = "To CSV Form";
            this.buttonToCSV.UseVisualStyleBackColor = true;
            this.buttonToCSV.Click += new System.EventHandler(this.buttonToCSV_Click);
            // 
            // checkBoxUseSP
            // 
            this.checkBoxUseSP.AutoSize = true;
            this.checkBoxUseSP.Checked = true;
            this.checkBoxUseSP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxUseSP.Location = new System.Drawing.Point(386, 257);
            this.checkBoxUseSP.Name = "checkBoxUseSP";
            this.checkBoxUseSP.Size = new System.Drawing.Size(62, 17);
            this.checkBoxUseSP.TabIndex = 9;
            this.checkBoxUseSP.Text = "Use SP";
            this.checkBoxUseSP.UseVisualStyleBackColor = true;
            this.checkBoxUseSP.CheckedChanged += new System.EventHandler(this.checkBoxUseSP_CheckedChanged);
            // 
            // FormDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 386);
            this.Controls.Add(this.checkBoxUseSP);
            this.Controls.Add(this.buttonToCSV);
            this.Controls.Add(this.buttonPull);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBarLoad);
            this.Controls.Add(this.checkBoxLocalDB);
            this.Controls.Add(this.buttonPush);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.tabControlOption);
            this.MinimumSize = new System.Drawing.Size(680, 425);
            this.Name = "FormDB";
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
            this.tabControlOption.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Button buttonPush;
        private System.Windows.Forms.CheckBox checkBoxLocalDB;
        private System.Windows.Forms.ProgressBar progressBarLoad;
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
        private System.Windows.Forms.TabControl tabControlOption;
        private System.Windows.Forms.Button buttonToCSV;
        private System.Windows.Forms.CheckBox checkBoxUseSP;
    }
}