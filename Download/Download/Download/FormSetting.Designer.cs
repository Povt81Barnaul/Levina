namespace Download
{
    partial class FormSetting
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
            this.label2 = new System.Windows.Forms.Label();
            this.button_Ok = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numerFileDownload = new System.Windows.Forms.NumericUpDown();
            this.numerThreadFile = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_DownloadServer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.timeWait = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numerFileDownload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerThreadFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeWait)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(331, 26);
            this.label2.TabIndex = 3;
            this.label2.Text = "Если настройки произведены, нажмите \"Ок\",\r\n и если сервер доступен, откроется окн" +
                "о работы с загрузками.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // button_Ok
            // 
            this.button_Ok.Location = new System.Drawing.Point(256, 170);
            this.button_Ok.Name = "button_Ok";
            this.button_Ok.Size = new System.Drawing.Size(75, 32);
            this.button_Ok.TabIndex = 7;
            this.button_Ok.Text = "Ок";
            this.button_Ok.UseVisualStyleBackColor = true;
            this.button_Ok.Click += new System.EventHandler(this.SettingSave);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(337, 170);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 32);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "Отмена";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.Cancel);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(234, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Число одновременно закачиваемых файлов";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Количество потоков на файл";
            // 
            // numerFileDownload
            // 
            this.numerFileDownload.Location = new System.Drawing.Point(268, 57);
            this.numerFileDownload.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerFileDownload.Name = "numerFileDownload";
            this.numerFileDownload.Size = new System.Drawing.Size(47, 20);
            this.numerFileDownload.TabIndex = 11;
            this.numerFileDownload.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numerThreadFile
            // 
            this.numerThreadFile.Location = new System.Drawing.Point(268, 91);
            this.numerThreadFile.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numerThreadFile.Name = "numerThreadFile";
            this.numerThreadFile.Size = new System.Drawing.Size(47, 20);
            this.numerThreadFile.TabIndex = 12;
            this.numerThreadFile.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(272, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Адрес сервера, куда будет производиться загрузка";
            // 
            // tb_DownloadServer
            // 
            this.tb_DownloadServer.Location = new System.Drawing.Point(15, 25);
            this.tb_DownloadServer.Name = "tb_DownloadServer";
            this.tb_DownloadServer.Size = new System.Drawing.Size(325, 20);
            this.tb_DownloadServer.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(247, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Время ожидания подключения сервера (минут)";
            // 
            // timeWait
            // 
            this.timeWait.Location = new System.Drawing.Point(268, 117);
            this.timeWait.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.timeWait.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.timeWait.Name = "timeWait";
            this.timeWait.Size = new System.Drawing.Size(47, 20);
            this.timeWait.TabIndex = 16;
            this.timeWait.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // FormSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 204);
            this.Controls.Add(this.timeWait);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tb_DownloadServer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numerThreadFile);
            this.Controls.Add(this.numerFileDownload);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Ok);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.label2);
            this.Name = "FormSetting";
            this.Text = "Настройка сервера-закачек";
            ((System.ComponentModel.ISupportInitialize)(this.numerFileDownload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numerThreadFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeWait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_Ok;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numerFileDownload;
        private System.Windows.Forms.NumericUpDown numerThreadFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_DownloadServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown timeWait;
    }
}