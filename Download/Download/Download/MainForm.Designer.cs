namespace Download
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColumnNameFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLoad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnURI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.загрузкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.стартоватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.добавитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.приостановитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.остановитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.удалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button_Exit = new System.Windows.Forms.Button();
            this.button_Setting = new System.Windows.Forms.Button();
            this.button_Delete = new System.Windows.Forms.Button();
            this.button_Stop = new System.Windows.Forms.Button();
            this.button_Pause = new System.Windows.Forms.Button();
            this.button_Start = new System.Windows.Forms.Button();
            this.button_Add = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnNameFile,
            this.ColumnLoad,
            this.ColumnSize,
            this.ColumnURI,
            this.ColumnLocation});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGridView1.Location = new System.Drawing.Point(0, 73);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(984, 599);
            this.dataGridView1.TabIndex = 0;
            // 
            // ColumnNameFile
            // 
            this.ColumnNameFile.HeaderText = "Имя файла";
            this.ColumnNameFile.Name = "ColumnNameFile";
            this.ColumnNameFile.ReadOnly = true;
            // 
            // ColumnLoad
            // 
            this.ColumnLoad.HeaderText = "Загружено";
            this.ColumnLoad.Name = "ColumnLoad";
            this.ColumnLoad.ReadOnly = true;
            // 
            // ColumnSize
            // 
            this.ColumnSize.HeaderText = "Размер";
            this.ColumnSize.Name = "ColumnSize";
            this.ColumnSize.ReadOnly = true;
            // 
            // ColumnURI
            // 
            this.ColumnURI.HeaderText = "URI";
            this.ColumnURI.Name = "ColumnURI";
            this.ColumnURI.ReadOnly = true;
            // 
            // ColumnLocation
            // 
            this.ColumnLocation.HeaderText = "Расположение";
            this.ColumnLocation.Name = "ColumnLocation";
            this.ColumnLocation.ReadOnly = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.загрузкаToolStripMenuItem,
            this.настройкаToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(984, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // загрузкаToolStripMenuItem
            // 
            this.загрузкаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.стартоватьToolStripMenuItem,
            this.добавитьToolStripMenuItem,
            this.приостановитьToolStripMenuItem,
            this.остановитьToolStripMenuItem,
            this.удалитьToolStripMenuItem});
            this.загрузкаToolStripMenuItem.Name = "загрузкаToolStripMenuItem";
            this.загрузкаToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.загрузкаToolStripMenuItem.Text = "Загрузка";
            // 
            // стартоватьToolStripMenuItem
            // 
            this.стартоватьToolStripMenuItem.Name = "стартоватьToolStripMenuItem";
            this.стартоватьToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.стартоватьToolStripMenuItem.Text = "Стартовать";
            this.стартоватьToolStripMenuItem.Click += new System.EventHandler(this.StartDownload);
            // 
            // добавитьToolStripMenuItem
            // 
            this.добавитьToolStripMenuItem.Name = "добавитьToolStripMenuItem";
            this.добавитьToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.добавитьToolStripMenuItem.Text = "Добавить";
            this.добавитьToolStripMenuItem.Click += new System.EventHandler(this.AddDownload);
            // 
            // приостановитьToolStripMenuItem
            // 
            this.приостановитьToolStripMenuItem.Name = "приостановитьToolStripMenuItem";
            this.приостановитьToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.приостановитьToolStripMenuItem.Text = "Приостановить";
            this.приостановитьToolStripMenuItem.Click += new System.EventHandler(this.PauseDownload);
            // 
            // остановитьToolStripMenuItem
            // 
            this.остановитьToolStripMenuItem.Name = "остановитьToolStripMenuItem";
            this.остановитьToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.остановитьToolStripMenuItem.Text = "Остановить";
            this.остановитьToolStripMenuItem.Click += new System.EventHandler(this.StopDownload);
            // 
            // удалитьToolStripMenuItem
            // 
            this.удалитьToolStripMenuItem.Name = "удалитьToolStripMenuItem";
            this.удалитьToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.удалитьToolStripMenuItem.Text = "Удалить";
            this.удалитьToolStripMenuItem.Click += new System.EventHandler(this.DeleteDownload);
            // 
            // настройкаToolStripMenuItem
            // 
            this.настройкаToolStripMenuItem.Name = "настройкаToolStripMenuItem";
            this.настройкаToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.настройкаToolStripMenuItem.Text = "Настройка";
            this.настройкаToolStripMenuItem.Click += new System.EventHandler(this.Setting);
            // 
            // button_Exit
            // 
            this.button_Exit.BackgroundImage = global::Download.Properties.Resources.exit;
            this.button_Exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Exit.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Exit.Location = new System.Drawing.Point(892, 24);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(51, 49);
            this.button_Exit.TabIndex = 8;
            this.toolTip1.SetToolTip(this.button_Exit, "Выход");
            this.button_Exit.UseVisualStyleBackColor = true;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // button_Setting
            // 
            this.button_Setting.BackgroundImage = global::Download.Properties.Resources._6;
            this.button_Setting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Setting.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_Setting.Location = new System.Drawing.Point(943, 24);
            this.button_Setting.Name = "button_Setting";
            this.button_Setting.Size = new System.Drawing.Size(41, 49);
            this.button_Setting.TabIndex = 9;
            this.toolTip1.SetToolTip(this.button_Setting, "Настройка");
            this.button_Setting.UseVisualStyleBackColor = true;
            this.button_Setting.Click += new System.EventHandler(this.Setting);
            // 
            // button_Delete
            // 
            this.button_Delete.BackgroundImage = global::Download.Properties.Resources._1;
            this.button_Delete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Delete.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Delete.Location = new System.Drawing.Point(168, 24);
            this.button_Delete.Name = "button_Delete";
            this.button_Delete.Size = new System.Drawing.Size(41, 49);
            this.button_Delete.TabIndex = 5;
            this.toolTip1.SetToolTip(this.button_Delete, "Удалить");
            this.button_Delete.UseVisualStyleBackColor = true;
            this.button_Delete.Click += new System.EventHandler(this.DeleteDownload);
            // 
            // button_Stop
            // 
            this.button_Stop.BackgroundImage = global::Download.Properties.Resources._8;
            this.button_Stop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Stop.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Stop.Location = new System.Drawing.Point(126, 24);
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.Size = new System.Drawing.Size(42, 49);
            this.button_Stop.TabIndex = 4;
            this.toolTip1.SetToolTip(this.button_Stop, "Остановить");
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.StopDownload);
            // 
            // button_Pause
            // 
            this.button_Pause.BackgroundImage = global::Download.Properties.Resources._7;
            this.button_Pause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Pause.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Pause.Location = new System.Drawing.Point(84, 24);
            this.button_Pause.Name = "button_Pause";
            this.button_Pause.Size = new System.Drawing.Size(42, 49);
            this.button_Pause.TabIndex = 3;
            this.toolTip1.SetToolTip(this.button_Pause, "Приостановить");
            this.button_Pause.UseVisualStyleBackColor = true;
            this.button_Pause.Click += new System.EventHandler(this.PauseDownload);
            // 
            // button_Start
            // 
            this.button_Start.BackgroundImage = global::Download.Properties.Resources._2;
            this.button_Start.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Start.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Start.Location = new System.Drawing.Point(42, 24);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(42, 49);
            this.button_Start.TabIndex = 2;
            this.toolTip1.SetToolTip(this.button_Start, "Старт");
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.StartDownload);
            // 
            // button_Add
            // 
            this.button_Add.BackgroundImage = global::Download.Properties.Resources._5;
            this.button_Add.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Add.Dock = System.Windows.Forms.DockStyle.Left;
            this.button_Add.Location = new System.Drawing.Point(0, 24);
            this.button_Add.Name = "button_Add";
            this.button_Add.Size = new System.Drawing.Size(42, 49);
            this.button_Add.TabIndex = 1;
            this.toolTip1.SetToolTip(this.button_Add, "Добавить");
            this.button_Add.UseVisualStyleBackColor = true;
            this.button_Add.Click += new System.EventHandler(this.AddDownload);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(251, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Обновить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 672);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.button_Setting);
            this.Controls.Add(this.button_Delete);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Pause);
            this.Controls.Add(this.button_Start);
            this.Controls.Add(this.button_Add);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Клиент";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button_Add;
        private System.Windows.Forms.Button button_Start;
        private System.Windows.Forms.Button button_Pause;
        private System.Windows.Forms.Button button_Stop;
        private System.Windows.Forms.Button button_Delete;
        private System.Windows.Forms.Button button_Setting;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem загрузкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem стартоватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem добавитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem приостановитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem остановитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem удалитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкаToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNameFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLoad;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnURI;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLocation;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Button button1;
    }
}

