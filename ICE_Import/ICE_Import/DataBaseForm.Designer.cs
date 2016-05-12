namespace ICE_Import
{
    partial class DataBaseForm
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
            this.checkBoxCheckDB = new System.Windows.Forms.CheckBox();
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
            this.tabControlOption = new System.Windows.Forms.TabControl();
            this.dataGridViewOption = new System.Windows.Forms.DataGridView();
            this.tabPageDailyContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).BeginInit();
            this.tabPageContract.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).BeginInit();
            this.tabPageOptionData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).BeginInit();
            this.tabPageOption.SuspendLayout();
            this.tabControlOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.BackColor = System.Drawing.SystemColors.InfoText;
            this.richTextBoxLog.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLog.Location = new System.Drawing.Point(3, 294);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(654, 90);
            this.richTextBoxLog.TabIndex = 1;
            this.richTextBoxLog.Text = "";
            // 
            // buttonPush
            // 
            this.buttonPush.Location = new System.Drawing.Point(6, 253);
            this.buttonPush.Name = "buttonPush";
            this.buttonPush.Size = new System.Drawing.Size(109, 23);
            this.buttonPush.TabIndex = 2;
            this.buttonPush.Text = "Push data to DB";
            this.buttonPush.UseVisualStyleBackColor = true;
            this.buttonPush.Click += new System.EventHandler(this.buttonPush_Click);
            // 
            // checkBoxCheckDB
            // 
            this.checkBoxCheckDB.AutoSize = true;
            this.checkBoxCheckDB.Checked = true;
            this.checkBoxCheckDB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCheckDB.Location = new System.Drawing.Point(209, 255);
            this.checkBoxCheckDB.Name = "checkBoxCheckDB";
            this.checkBoxCheckDB.Size = new System.Drawing.Size(102, 17);
            this.checkBoxCheckDB.TabIndex = 3;
            this.checkBoxCheckDB.Text = "Local DataBase";
            this.checkBoxCheckDB.UseVisualStyleBackColor = true;
            this.checkBoxCheckDB.CheckedChanged += new System.EventHandler(this.checkBoxCheckDB_CheckedChanged);
            // 
            // progressBarLoad
            // 
            this.progressBarLoad.Location = new System.Drawing.Point(6, 280);
            this.progressBarLoad.Name = "progressBarLoad";
            this.progressBarLoad.Size = new System.Drawing.Size(648, 10);
            this.progressBarLoad.TabIndex = 4;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(121, 252);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(82, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonPull
            // 
            this.buttonPull.Location = new System.Drawing.Point(550, 251);
            this.buttonPull.Name = "buttonPull";
            this.buttonPull.Size = new System.Drawing.Size(102, 23);
            this.buttonPull.TabIndex = 6;
            this.buttonPull.Text = "Pull data from DB";
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
            this.tabPageContract.Text = "tblcontract";
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
            // dataGridViewOption
            // 
            this.dataGridViewOption.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOption.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewOption.Name = "dataGridViewOption";
            this.dataGridViewOption.Size = new System.Drawing.Size(648, 219);
            this.dataGridViewOption.TabIndex = 0;
            // 
            // DataBaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 386);
            this.Controls.Add(this.buttonPull);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBarLoad);
            this.Controls.Add(this.checkBoxCheckDB);
            this.Controls.Add(this.buttonPush);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.tabControlOption);
            this.MinimumSize = new System.Drawing.Size(680, 425);
            this.Name = "DataBaseForm";
            this.Text = "DataBaseForm";
            this.Load += new System.EventHandler(this.DataBaseForm_Load);
            this.tabPageDailyContract.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDailyContract)).EndInit();
            this.tabPageContract.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewContract)).EndInit();
            this.tabPageOptionData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOptionData)).EndInit();
            this.tabPageOption.ResumeLayout(false);
            this.tabControlOption.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOption)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Button buttonPush;
        private System.Windows.Forms.CheckBox checkBoxCheckDB;
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
    }
}