using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.IO;
using System.Net;

namespace Download
{
    public partial class MainForm : Form
    {    
        /// <summary>
        /// Список элементов, связывающих закачку со строкой в гриде
        /// </summary>
        private static List<RowGrid> downloaders;
        /// <summary>
        /// файл, где хранится список всех закачек
        /// </summary>
        private readonly  string downloadListFile;

        private readonly Logger log;

        private delegate void RefreshDelegate();
        /// <summary>
        /// Адрес сервера
        /// </summary>
        IPEndPoint serverAddress;
        /// <summary>
        /// Клиент
        /// </summary>
        Client client;
        /// <summary>
        /// Флаг установления соединения с сервером
        /// </summary>
       public static bool flagConnect = true;

       public static System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();
       
        /// <summary>
        /// Первоначальная инициализация
        /// </summary>
        public MainForm()
        {            
             client = null;
            serverAddress = null;
            try
            {
                string ipstr = Properties.Settings.Default.DownloadServer;//"127.0.0.1";
               IPAddress ip = null;
               if (!IPAddress.TryParse(ipstr, out ip))
                   MessageBox.Show("Неверный формат IP адреса");
                serverAddress = new IPEndPoint(ip, 11000);
                if (serverAddress == null)
                    throw new FormatException("Не правильный формат ip адреса или порта");
                client = new Client(serverAddress);
                
            }
            catch (FormatException err)
            {
                MessageBox.Show(err.Message);
            }

            InitializeComponent();
            downloadListFile = Properties.Settings.Default.downloadListFile;
            log = new Logger(Directory.GetCurrentDirectory() + "\\" + Properties.Settings.Default.logFile);
            downloaders = new List<RowGrid>();
           
            LoadDownloadList(downloadListFile);
                 
            timer.Tick+=new EventHandler(timer_Tick);
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
    {
    	RowStateChanged();
    }
        /// <summary>
        /// Получить размер загрузки
        /// </summary>
        /// <returns></returns>
        private string GetFormattingSize(long size)
        {
            string result = "Не известно";

            var fsize = (float)size;

            if (fsize > 0)
            {
                if (fsize > 1024)
                {
                    fsize /= 1024;

                    if (fsize > 1024)
                    {
                        fsize /= 1024;

                        if (fsize > 1024)
                        {
                            fsize /= 1024;

                            result = fsize.ToString("F") + " gb.";
                        }
                        else
                        {
                            result = fsize.ToString("F") + " mb.";
                        }
                    }
                    else
                    {
                        result = fsize.ToString("F") + " kb.";
                    }
                }
                else
                {
                    result = fsize.ToString("F") + " b.";
                }
            }

            return result;
        }
        /// <summary>
        /// Загрузить список всех закачек из файла
        /// </summary>
        /// <param name="inputFile"></param>
        private void LoadDownloadList(string inputFile)
        {
            XmlReader reader = null;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.IgnoreWhitespace = true;
                settings.IgnoreComments = true;
                settings.CloseInput = true;

                reader = XmlReader.Create(File.OpenRead(inputFile), settings);

                reader.Read();
                if (reader.NodeType != XmlNodeType.XmlDeclaration) throw new FormatException();
                reader.ReadStartElement("DownloadsArray");

                if (reader.ReadState == ReadState.EndOfFile)
                    return;

                while ((reader.Name != "DownloadsArray") && (reader.NodeType != XmlNodeType.EndElement))
                {
                    RowGrid loadDL = RowGrid.FromXml(reader);
                    downloaders.Add(loadDL);
                    AddDownloadItem(loadDL, false);
                    reader.Read();
                }
            }
            catch (Exception ex)
            {
                log.WriteMessage("Ошибка при первоначальной загрузке списка закачек:  " + ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }
        /// <summary>
        /// Создает загрузку и добавляет его в таблицу
        /// </summary>
        /// <param name="download"></param>
        void AddDownloadItem(RowGrid download, bool selectAddedRow)
        {
            int rowIndex = dataGridView1.Rows.Add(new DataGridViewRow());
            dataGridView1["ColumnURI", rowIndex].Value = download.Uri.AbsoluteUri;
            dataGridView1["ColumnNameFile", rowIndex].Value = download.FileName;
            dataGridView1["ColumnSize", rowIndex].Value =GetFormattingSize(download.Size)+" "+GetFormattingSize(download.BytesDownload);
            dataGridView1["ColumnLocation", rowIndex].Value = download.Location;
            dataGridView1["ColumnLoad", rowIndex].Value = download.NameState(download.State);

        }
      
        /// <summary>
        /// Изменение состояния загрузок
        /// </summary>
        /// <param name="row"></param>
        void RowStateChanged()
        {
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].flagRefresh == true)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        dataGridView1["ColumnURI", i].Value = downloaders[i].Uri.AbsoluteUri;

                        dataGridView1["ColumnNameFile", i].Value = downloaders[i].FileName;
                        if (downloaders[i].time != null)
                        {
                            dataGridView1["ColumnSize", i].Value = GetFormattingSize(downloaders[i].Size) + " " + GetFormattingSize(downloaders[i].BytesDownload) + " " + downloaders[i].time;
                        }
                        else
                        {
                            dataGridView1["ColumnSize", i].Value = GetFormattingSize(downloaders[i].Size) + " " + GetFormattingSize(downloaders[i].BytesDownload);
                        }
                         dataGridView1["ColumnLocation", i].Value = downloaders[i].Location;
                        dataGridView1["ColumnLoad", i].Value = downloaders[i].NameState(downloaders[i].State);
                        downloaders[i].flagRefresh = false;
                        dataGridView1.Refresh();
                    }
                }
            }
        }       
        /// <summary>
        /// Клик кнопки Добавление новой закачки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDownload(object sender, EventArgs e)
        {
            bool flagD=true;
            int countDownload=0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1["ColumnLoad", i].Value.ToString() == "Загружается")
                {
                    countDownload++;
                }
            }
            if (countDownload >= Properties.Settings.Default.numFileDownload)
                flagD = false;
            if (flagD)
            {
                AddDownloadForm form = new AddDownloadForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RowGrid download = new RowGrid(form.Uri, form.FileName);
                    download.State = StateDownload.Created;
                    download.Size = 0;
                    download.BytesDownload = 0;
                    downloaders.Add(download);
                    AddDownloadItem(download, true);

                    client.SignalServerDownload(form.Uri.ToString());                   
               
                }
            }
            else
            {
                MessageBox.Show("Превышено число возможных загрузок на сервер.");
            }
        }
        /// <summary>
        /// Запустить скачивание текущей загрузки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartDownload(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                if (rowIndex != -1)
                {
                    if (downloaders[rowIndex].State == StateDownload.Paused)
                    {
                        dataGridView1["ColumnLoad", rowIndex].Value = downloaders[rowIndex].NameState(downloaders[rowIndex].State);

                        client.SignalServerDownloadLast(dataGridView1["ColumnURI", rowIndex].Value.ToString());
                    }
                    else
                    {
                        MessageBox.Show("Запустить закачку можно только после паузы!","Информация");
                    }
                }
            }
        }
        /// <summary>
        /// Поставить текущую загрузку на паузу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseDownload(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                if (rowIndex != -1)
                {
                    if (dataGridView1["ColumnLoad", rowIndex].Value.ToString() == "Завершена" || dataGridView1["ColumnLoad", rowIndex].Value.ToString() == "Не известно")
                    {
                        MessageBox.Show("Не имеет смысла ставить данную загрузку на паузу","Информация");
                    }
                    else
                    {
                        if (downloaders[rowIndex].State == StateDownload.Downloading)
                        {
                            dataGridView1["ColumnLoad", rowIndex].Value = downloaders[rowIndex].NameState(downloaders[rowIndex].State); 
                    
                            client.SignalServerPause(dataGridView1["ColumnURI", rowIndex].Value.ToString());
                        }
                        else
                            {
                              MessageBox.Show("Запустить закачку можно только во время загрузки!","Информация");
                            }
                    }
                }
            }
        }
        /// <summary>
        /// Остановить текущую закачку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopDownload(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                if (rowIndex != -1)
                {
                    if (dataGridView1["ColumnLoad", rowIndex].Value.ToString() == "Завершена" || dataGridView1["ColumnLoad", rowIndex].Value.ToString() == "Не известно")
                    {
                        MessageBox.Show("Не имеет смысла ставить данную загрузку останавливать!");
                    }
                    else
                    {
                      client.SignalServerStop(dataGridView1["ColumnURI", rowIndex].Value.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Удалить текущую закачку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDownload(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                
                if (rowIndex != -1)
                {
                    string uri = dataGridView1["ColumnURI", rowIndex].Value.ToString();
                    if (MessageBox.Show("Вы действительно хотите удалить закачку из списка?", "Подтверждение",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (MessageBox.Show("Удалить также загруженный файл с диска?", "Подтверждение",
                                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            client.SignalServerDelete(dataGridView1["ColumnURI", rowIndex].Value.ToString());

                        }
                        //dataGridView1.Rows.RemoveAt(rowIndex);
                        //downloaders.RemoveAt(rowIndex);
                        int[] tmp = new int[dataGridView1.Rows.Count];
                        for (int i = 0; i < tmp.Length; i++)
                            tmp[i] = -1;
                        int k=0;
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (uri == dataGridView1["ColumnURI", i].Value.ToString())
                            {
                                tmp[k] = i; k++;
                            }
                        }
                        if (tmp != null)
                        {
                            for (int i = tmp.Length-1; i >=0; i--)
                            {
                                if (tmp[i] != -1)
                                {
                                    dataGridView1.Rows.RemoveAt(tmp[i]);
                                    downloaders.RemoveAt(tmp[i]);
                                }
                            }
                        }
                    }
                }
            }
            RowStateChanged();
        }       
        /// <summary>
        /// Настройка сервера закачек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting(object sender, EventArgs e)
        {
            FormSetting form = new FormSetting(false);
           
            if (form.ShowDialog(this) == DialogResult.OK)
            {
            }
            RowStateChanged();
        }           
        /// <summary>
        /// Сохранить список всех закачек в файл
        /// </summary>
        /// <param name="outputFile"></param>
        private void SaveDownloadList(string outputFile)
        {
            XmlWriter writer = null;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                };

                writer = XmlWriter.Create(File.Create(outputFile), settings);

                writer.WriteStartElement("DownloadsArray");

                for (int i = 0; i < downloaders.Count; i++)
                {
                    RowGrid current = downloaders[i];
                    current.ToXml(writer);
                }

                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                log.WriteMessage("Ошибка при записи списка закачек в файл | " + ex.Message);
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }
        /// <summary>
        /// Если установлено соединение с сервером
        /// </summary>
       public void cool()
        {       
            foreach (var item in Application.OpenForms)
            {
                var mf = item as MainForm;
                if (mf != null)
                {
                    mf.menuStrip1.Enabled = true;
                    foreach (var c in mf.Controls)
                    {
                        var button = c as Button;
                        if (button != null)
                        {
                            button.Enabled = true;
                            button.BackColor = SystemColors.Control;
                        }
                    }
                }
            }
        }
       /// <summary>
       /// Если неустановлено соединение с сервером
       /// </summary>
        public void block()
        {
            foreach (var item in Application.OpenForms)
            {
                var mf = item as MainForm;
                if (mf != null)
                {
                    mf.menuStrip1.Enabled = false;
                    foreach (var c in mf.Controls)
                    {
                        var button = c as Button;
                        if (button != null && button != mf.button_Setting && button != mf.button_Exit)
                        {
                            button.Enabled = false;
                            button.BackColor = Color.Blue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Событие при загрузки формы
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
              FormSetting form = new FormSetting(true);         
              if (form.ShowDialog(this) == DialogResult.OK)
              {
                  block();
                  DateTime dt=DateTime.Now;
                  while (true)
                  {
                      if (!flagConnect)
                      {
                          cool();
                          break;
                      }

                      var sp = DateTime.Now- dt;
                      if (sp.Minutes == Properties.Settings.Default.timeWait)
                      {
                      MessageBox.Show(this, "Сервер не доступен, приложение будет закрыто", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Question);
                     
                          Close();
                          break;
                      }
                  }
              }
              else
              {
                  Close();
              }
        }
        /// <summary>
        /// Событие при нажатие кнопки выход
        /// </summary>
        private void button_Exit_Click(object sender, EventArgs e)
        {
            SaveDownloadList(downloadListFile);
            if (MessageBox.Show(this, "Вы действительно хотите выйти из программы?", "Подтверждение",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (client != null)
                    client.CloseClientCmd();
                Close();
            }          
        }
        /// <summary>
        /// Загружается файл
        /// </summary>
        public static void DOWN(string uri, long size)
        {
            Uri tmp=new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = StateDownload.Downloading;
                    downloaders[i].BytesDownload = size;
                    downloaders[i].Size = size;
                    downloaders[i].flagRefresh=true;                     
                }
            }
        }
        //Невозможно загрузить
        public static void NODOWN(string uri)
        {
            Uri tmp = new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = StateDownload.Nofile;
                    downloaders[i].flagRefresh = true;
                }
            }
        }      
            //загружено
        public static void COMPLETED(string uri,long size)
        {
            Uri tmp=new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = StateDownload.Completed;
                    downloaders[i].BytesDownload = size;
                    downloaders[i].Size = size;
                    downloaders[i].flagRefresh=true;
                }
            }
        }
        public static void Pause(string uri,long size)
        {
            Uri tmp = new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = StateDownload.Paused;
                    downloaders[i].Size = size;
                    downloaders[i].flagRefresh = true;
                }
            }
        }
        public static void Stop(string uri, long size)
        {
            Uri tmp = new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = StateDownload.Stopped;
                    downloaders[i].Size = size;
                    downloaders[i].flagRefresh = true;
                }
            }
        }
       
         public static void  Information(StateDownload st, string uri, long size,long downsize,TimeSpan time)
        {
            Uri tmp = new Uri(uri);
            for (int i = 0; i < downloaders.Count; i++)
            {
                if (downloaders[i].Uri == tmp)
                {
                    downloaders[i].State = st;
                    downloaders[i].BytesDownload = downsize;
                    downloaders[i].Size = size;
                    downloaders[i].time = time;
                    downloaders[i].flagRefresh = true; 
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RowStateChanged();
        }  

    }
}
