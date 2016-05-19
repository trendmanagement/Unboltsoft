namespace ICE_Import
{
    partial class FormCSV
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
            this.backgroundWorker_ParsingOptions = new System.ComponentModel.BackgroundWorker();
            this.buttonToDB = new System.Windows.Forms.Button();
            this.button_CancelFuture = new System.Windows.Forms.Button();
            this.progressBar_ParsingFuture = new System.Windows.Forms.ProgressBar();
            this.label_ParsedFuture = new System.Windows.Forms.Label();
            this.button_ParseFuture = new System.Windows.Forms.Button();
            this.label_InputFuture = new System.Windows.Forms.Label();
            this.button_InputFuture = new System.Windows.Forms.Button();
            this.backgroundWorker_ParsingFutures = new System.ComponentModel.BackgroundWorker();
            this.button_CancelOption = new System.Windows.Forms.Button();
            this.progressBar_ParsingOption = new System.Windows.Forms.ProgressBar();
            this.label_ParsedOption = new System.Windows.Forms.Label();
            this.button_ParseOption = new System.Windows.Forms.Button();
            this.label_InputOption = new System.Windows.Forms.Label();
            this.button_InputOption = new System.Windows.Forms.Button();
            this.checkBoxFuturesOnly = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // backgroundWorker_ParsingOptions
            // 
            this.backgroundWorker_ParsingOptions.WorkerReportsProgress = true;
            this.backgroundWorker_ParsingOptions.WorkerSupportsCancellation = true;
            this.backgroundWorker_ParsingOptions.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_ParsingOptions_DoWork);
            this.backgroundWorker_ParsingOptions.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ParsingOptions_ProgressChanged);
            this.backgroundWorker_ParsingOptions.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_ParsingOptions_RunWorkerCompleted);
            // 
            // buttonToDB
            // 
            this.buttonToDB.Location = new System.Drawing.Point(12, 226);
            this.buttonToDB.Name = "buttonToDB";
            this.buttonToDB.Size = new System.Drawing.Size(115, 23);
            this.buttonToDB.TabIndex = 7;
            this.buttonToDB.Text = "To DB Form";
            this.buttonToDB.UseVisualStyleBackColor = true;
            this.buttonToDB.Click += new System.EventHandler(this.buttonDB_Click);
            // 
            // button_CancelFuture
            // 
            this.button_CancelFuture.Enabled = false;
            this.button_CancelFuture.Location = new System.Drawing.Point(69, 41);
            this.button_CancelFuture.Name = "button_CancelFuture";
            this.button_CancelFuture.Size = new System.Drawing.Size(58, 23);
            this.button_CancelFuture.TabIndex = 13;
            this.button_CancelFuture.Text = "Cancel";
            this.button_CancelFuture.UseVisualStyleBackColor = true;
            this.button_CancelFuture.Click += new System.EventHandler(this.button_CancelFuture_Click);
            // 
            // progressBar_ParsingFuture
            // 
            this.progressBar_ParsingFuture.Location = new System.Drawing.Point(12, 70);
            this.progressBar_ParsingFuture.Name = "progressBar_ParsingFuture";
            this.progressBar_ParsingFuture.Size = new System.Drawing.Size(560, 23);
            this.progressBar_ParsingFuture.TabIndex = 12;
            // 
            // label_ParsedFuture
            // 
            this.label_ParsedFuture.AutoSize = true;
            this.label_ParsedFuture.Location = new System.Drawing.Point(134, 46);
            this.label_ParsedFuture.Name = "label_ParsedFuture";
            this.label_ParsedFuture.Size = new System.Drawing.Size(0, 13);
            this.label_ParsedFuture.TabIndex = 11;
            // 
            // button_ParseFuture
            // 
            this.button_ParseFuture.Enabled = false;
            this.button_ParseFuture.Location = new System.Drawing.Point(12, 41);
            this.button_ParseFuture.Name = "button_ParseFuture";
            this.button_ParseFuture.Size = new System.Drawing.Size(51, 23);
            this.button_ParseFuture.TabIndex = 10;
            this.button_ParseFuture.Text = "Parse";
            this.button_ParseFuture.UseVisualStyleBackColor = true;
            this.button_ParseFuture.Click += new System.EventHandler(this.button_ParseFuture_Click);
            // 
            // label_InputFuture
            // 
            this.label_InputFuture.AutoSize = true;
            this.label_InputFuture.Location = new System.Drawing.Point(134, 17);
            this.label_InputFuture.Name = "label_InputFuture";
            this.label_InputFuture.Size = new System.Drawing.Size(71, 13);
            this.label_InputFuture.TabIndex = 9;
            this.label_InputFuture.Text = "(not selected)";
            // 
            // button_InputFuture
            // 
            this.button_InputFuture.Location = new System.Drawing.Point(12, 12);
            this.button_InputFuture.Name = "button_InputFuture";
            this.button_InputFuture.Size = new System.Drawing.Size(115, 23);
            this.button_InputFuture.TabIndex = 8;
            this.button_InputFuture.Text = "Future CSV File(s)";
            this.button_InputFuture.UseVisualStyleBackColor = true;
            this.button_InputFuture.Click += new System.EventHandler(this.button_InputFuture_Click);
            // 
            // backgroundWorker_ParsingFutures
            // 
            this.backgroundWorker_ParsingFutures.WorkerReportsProgress = true;
            this.backgroundWorker_ParsingFutures.WorkerSupportsCancellation = true;
            this.backgroundWorker_ParsingFutures.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_ParsingFutures_DoWork);
            this.backgroundWorker_ParsingFutures.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ParsingFutures_ProgressChanged);
            this.backgroundWorker_ParsingFutures.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_ParsingFutures_RunWorkerCompleted);
            // 
            // button_CancelOption
            // 
            this.button_CancelOption.Enabled = false;
            this.button_CancelOption.Location = new System.Drawing.Point(69, 151);
            this.button_CancelOption.Name = "button_CancelOption";
            this.button_CancelOption.Size = new System.Drawing.Size(58, 23);
            this.button_CancelOption.TabIndex = 19;
            this.button_CancelOption.Text = "Cancel";
            this.button_CancelOption.UseVisualStyleBackColor = true;
            this.button_CancelOption.Click += new System.EventHandler(this.button_CancelOption_Click);
            // 
            // progressBar_ParsingOption
            // 
            this.progressBar_ParsingOption.Location = new System.Drawing.Point(12, 180);
            this.progressBar_ParsingOption.Name = "progressBar_ParsingOption";
            this.progressBar_ParsingOption.Size = new System.Drawing.Size(560, 23);
            this.progressBar_ParsingOption.TabIndex = 18;
            // 
            // label_ParsedOption
            // 
            this.label_ParsedOption.AutoSize = true;
            this.label_ParsedOption.Location = new System.Drawing.Point(133, 139);
            this.label_ParsedOption.Name = "label_ParsedOption";
            this.label_ParsedOption.Size = new System.Drawing.Size(0, 13);
            this.label_ParsedOption.TabIndex = 17;
            // 
            // button_ParseOption
            // 
            this.button_ParseOption.Enabled = false;
            this.button_ParseOption.Location = new System.Drawing.Point(12, 151);
            this.button_ParseOption.Name = "button_ParseOption";
            this.button_ParseOption.Size = new System.Drawing.Size(51, 23);
            this.button_ParseOption.TabIndex = 16;
            this.button_ParseOption.Text = "Parse";
            this.button_ParseOption.UseVisualStyleBackColor = true;
            this.button_ParseOption.Click += new System.EventHandler(this.button_ParseOptions_Click);
            // 
            // label_InputOption
            // 
            this.label_InputOption.AutoSize = true;
            this.label_InputOption.Location = new System.Drawing.Point(133, 127);
            this.label_InputOption.Name = "label_InputOption";
            this.label_InputOption.Size = new System.Drawing.Size(71, 13);
            this.label_InputOption.TabIndex = 15;
            this.label_InputOption.Text = "(not selected)";
            // 
            // button_InputOption
            // 
            this.button_InputOption.Location = new System.Drawing.Point(12, 122);
            this.button_InputOption.Name = "button_InputOption";
            this.button_InputOption.Size = new System.Drawing.Size(115, 23);
            this.button_InputOption.TabIndex = 14;
            this.button_InputOption.Text = "Option CSV File(s)";
            this.button_InputOption.UseVisualStyleBackColor = true;
            this.button_InputOption.Click += new System.EventHandler(this.button_InputOption_Click);
            // 
            // checkBoxFuturesOnly
            // 
            this.checkBoxFuturesOnly.AutoSize = true;
            this.checkBoxFuturesOnly.Location = new System.Drawing.Point(12, 99);
            this.checkBoxFuturesOnly.Name = "checkBoxFuturesOnly";
            this.checkBoxFuturesOnly.Size = new System.Drawing.Size(85, 17);
            this.checkBoxFuturesOnly.TabIndex = 20;
            this.checkBoxFuturesOnly.Text = "Futures Only";
            this.checkBoxFuturesOnly.UseVisualStyleBackColor = true;
            this.checkBoxFuturesOnly.CheckedChanged += new System.EventHandler(this.checkBoxFuturesOnly_CheckedChanged);
            // 
            // FormCSV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 261);
            this.Controls.Add(this.checkBoxFuturesOnly);
            this.Controls.Add(this.button_CancelOption);
            this.Controls.Add(this.progressBar_ParsingOption);
            this.Controls.Add(this.label_ParsedOption);
            this.Controls.Add(this.button_ParseOption);
            this.Controls.Add(this.label_InputOption);
            this.Controls.Add(this.button_InputOption);
            this.Controls.Add(this.button_CancelFuture);
            this.Controls.Add(this.progressBar_ParsingFuture);
            this.Controls.Add(this.label_ParsedFuture);
            this.Controls.Add(this.button_ParseFuture);
            this.Controls.Add(this.label_InputFuture);
            this.Controls.Add(this.button_InputFuture);
            this.Controls.Add(this.buttonToDB);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "FormCSV";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ICE Import (CSV Form)";
            this.SizeChanged += new System.EventHandler(this.FormCSV_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.ComponentModel.BackgroundWorker backgroundWorker_ParsingOptions;
        private System.Windows.Forms.Button buttonToDB;
        private System.Windows.Forms.Button button_CancelFuture;
        private System.Windows.Forms.ProgressBar progressBar_ParsingFuture;
        private System.Windows.Forms.Label label_ParsedFuture;
        private System.Windows.Forms.Button button_ParseFuture;
        private System.Windows.Forms.Label label_InputFuture;
        private System.Windows.Forms.Button button_InputFuture;
        private System.ComponentModel.BackgroundWorker backgroundWorker_ParsingFutures;
        private System.Windows.Forms.Button button_CancelOption;
        private System.Windows.Forms.ProgressBar progressBar_ParsingOption;
        private System.Windows.Forms.Label label_ParsedOption;
        private System.Windows.Forms.Button button_ParseOption;
        private System.Windows.Forms.Label label_InputOption;
        private System.Windows.Forms.Button button_InputOption;
        private System.Windows.Forms.CheckBox checkBoxFuturesOnly;
    }
}

