namespace ICE_Import
{
    partial class Form1
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
            this.button_InputFiles = new System.Windows.Forms.Button();
            this.label_InputFile = new System.Windows.Forms.Label();
            this.button_Parse = new System.Windows.Forms.Button();
            this.label_ParsedFile = new System.Windows.Forms.Label();
            this.comboBox_Symbol = new System.Windows.Forms.ComboBox();
            this.progressBar_Parsing = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker_Parsing = new System.ComponentModel.BackgroundWorker();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.buttonDB = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_InputFiles
            // 
            this.button_InputFiles.Location = new System.Drawing.Point(12, 12);
            this.button_InputFiles.Name = "button_InputFiles";
            this.button_InputFiles.Size = new System.Drawing.Size(75, 23);
            this.button_InputFiles.TabIndex = 0;
            this.button_InputFiles.Text = "Input File(s)";
            this.button_InputFiles.UseVisualStyleBackColor = true;
            this.button_InputFiles.Click += new System.EventHandler(this.button_InputFile_Click);
            // 
            // label_InputFile
            // 
            this.label_InputFile.AutoSize = true;
            this.label_InputFile.Location = new System.Drawing.Point(93, 17);
            this.label_InputFile.Name = "label_InputFile";
            this.label_InputFile.Size = new System.Drawing.Size(71, 13);
            this.label_InputFile.TabIndex = 1;
            this.label_InputFile.Text = "(not selected)";
            // 
            // button_Parse
            // 
            this.button_Parse.Enabled = false;
            this.button_Parse.Location = new System.Drawing.Point(12, 68);
            this.button_Parse.Name = "button_Parse";
            this.button_Parse.Size = new System.Drawing.Size(75, 23);
            this.button_Parse.TabIndex = 3;
            this.button_Parse.Text = "Parse";
            this.button_Parse.UseVisualStyleBackColor = true;
            this.button_Parse.Click += new System.EventHandler(this.button_Parse_Click);
            // 
            // label_ParsedFile
            // 
            this.label_ParsedFile.AutoSize = true;
            this.label_ParsedFile.Location = new System.Drawing.Point(93, 73);
            this.label_ParsedFile.Name = "label_ParsedFile";
            this.label_ParsedFile.Size = new System.Drawing.Size(0, 13);
            this.label_ParsedFile.TabIndex = 4;
            // 
            // comboBox_Symbol
            // 
            this.comboBox_Symbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Symbol.FormattingEnabled = true;
            this.comboBox_Symbol.Location = new System.Drawing.Point(12, 41);
            this.comboBox_Symbol.Name = "comboBox_Symbol";
            this.comboBox_Symbol.Size = new System.Drawing.Size(121, 21);
            this.comboBox_Symbol.TabIndex = 2;
            // 
            // progressBar_Parsing
            // 
            this.progressBar_Parsing.Location = new System.Drawing.Point(12, 97);
            this.progressBar_Parsing.Name = "progressBar_Parsing";
            this.progressBar_Parsing.Size = new System.Drawing.Size(560, 23);
            this.progressBar_Parsing.TabIndex = 5;
            // 
            // backgroundWorker_Parsing
            // 
            this.backgroundWorker_Parsing.WorkerReportsProgress = true;
            this.backgroundWorker_Parsing.WorkerSupportsCancellation = true;
            this.backgroundWorker_Parsing.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_Parsing_DoWork);
            this.backgroundWorker_Parsing.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_Parsing_ProgressChanged);
            this.backgroundWorker_Parsing.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_Parsing_RunWorkerCompleted);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Enabled = false;
            this.button_Cancel.Location = new System.Drawing.Point(12, 127);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // buttonDB
            // 
            this.buttonDB.Location = new System.Drawing.Point(12, 226);
            this.buttonDB.Name = "buttonDB";
            this.buttonDB.Size = new System.Drawing.Size(139, 23);
            this.buttonDB.TabIndex = 7;
            this.buttonDB.Text = "Open DataBase window";
            this.buttonDB.UseVisualStyleBackColor = true;
            this.buttonDB.Click += new System.EventHandler(this.buttonDB_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 261);
            this.Controls.Add(this.buttonDB);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.progressBar_Parsing);
            this.Controls.Add(this.comboBox_Symbol);
            this.Controls.Add(this.label_ParsedFile);
            this.Controls.Add(this.button_Parse);
            this.Controls.Add(this.label_InputFile);
            this.Controls.Add(this.button_InputFiles);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ICE Import";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_InputFiles;
        private System.Windows.Forms.Label label_InputFile;
        private System.Windows.Forms.ComboBox comboBox_Symbol;
        private System.Windows.Forms.Button button_Parse;
        private System.Windows.Forms.Label label_ParsedFile;
        private System.Windows.Forms.ProgressBar progressBar_Parsing;
        private System.Windows.Forms.Button button_Cancel;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Parsing;
        private System.Windows.Forms.Button buttonDB;
    }
}

