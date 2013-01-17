using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using DownloadLibrary;
using System.Xml;

namespace Server
{
    
    public class Download : IDownloader
    {
        public int id;
        public String HostName;
        public string DownloadPath { get; set; }
        /// <summary>
        /// Имя скачиваемого файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Размер скачиваемого файла
        /// </summary>
        public long TotalSize { get; set; }       
        /// <summary>
        /// Идентификатор ресурса
        /// </summary>
        public Uri Url { get; private set; }
        StateDownload status;
        public StateDownload Status
        {
            get
            { return status; }

            set
            {
                if (status != value)
                {
                    status = value;
                    this.OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        public int MaxThreads { get; set; }
        List<DownloadTread> downloadTread = null;
        /// <summary>
        /// Если получим число буферов, затем возникает событие DownloadProgressChanged
        /// </summary>
        public int BufferCountPerNotification { get; set; }
        /// <summary>
        /// Установить BufferSize при чтении данных в поток ответа.
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// Размер кэша в памяти.
        /// </summary>
        public int MaxCacheSize { get; set; }
        /// <summary>
        /// Начальная точка
        /// </summary>
        public long StartPoint { get; set; }
        /// <summary>
        /// Конечная точка
        /// </summary>
        public long EndPoint { get; set; }
        /// <summary>
        /// Флаг возможности загрузки
        /// </summary>
        public bool HasChecked { get; set; }
        /// <summary>
        /// Используется при расчете скорости загрузки.
        /// </summary>
        static object locker = new object();
        /// <summary>
        /// Укажите, следует ли удаленный сервер поддерживает "Accept-Ranges" заголовка.
        /// Используйте CheckUrl метод для инициализации этого свойства внутренне.
        /// </summary>
        public bool IsRangeSupported { get; set; }
        /// <summary>
        /// Сетевые учетные данные, используемые для проверки подлинности запроса
        /// </summary>
        public ICredentials Credentials { get; set; }
        ///<summary>
        /// Размер кэша
        ///</summary>
        public int CachedSize { get; private set; }
        /// <summary>
        /// Время начала
        /// </summary>
        private DateTime lastStartTime;
        /// <summary>
        /// Время для события DownloadProgressChanged
        /// </summary>
        private DateTime lastNotificationTime;
        /// <summary>
        /// Размер для события DownloadProgressChanged
        /// </summary>
        private long lastNotificationDownloadedSize;
        /// <summary>
        /// Прокси 
        /// </summary>
        public IWebProxy Proxy { get; set; }
        /// <summary>
        /// размер загруженной чати файла
        /// </summary>
        public long DownloadedSize
        {
            get
            {
                return this.downloadTread.Sum(client => client.DownloadedSize);
            }
        }
        /// <summary>
        /// Сохраняем использованное время, проведенное в загрузке данных. Стоимость не включает
        /// паузу времени, и он будет обновляться только когда загрузка приостановлена или завершена
        /// </summary>
        private TimeSpan usedTime = new TimeSpan();
        /// <summary>
        /// Использованное время
        /// </summary>
        public TimeSpan TotalUsedTime
        {
            get
            {
                if (this.Status != StateDownload.Downloading)
                {
                    return usedTime;
                }
                else
                {
                    return usedTime.Add(DateTime.Now - lastStartTime);
                }
            }
        }
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        public event EventHandler StatusChanged;

        public Download()
        {
        }
        public Download(string uri)
            : this(uri, Environment.ProcessorCount * 2)
        {
        }

        public Download(string uri, int maxThreadCount)
        {
            this.DownloadPath = Directory.GetCurrentDirectory() + @"\Download";  
            this.Url = new Uri(uri);
            this.StartPoint = 0;
            this.EndPoint = long.MaxValue;
            this.BufferSize = 1024;
            this.MaxCacheSize = 1048576;
            this.BufferCountPerNotification = 64;
            this.MaxThreads = maxThreadCount;
           
            // Установить максимальное число одновременных подключений разрешено ServicePoint
            ServicePointManager.DefaultConnectionLimit = maxThreadCount;
            downloadTread = new List<DownloadTread>();
            this.Status = StateDownload.Created;
        }
        /// <summary>
        /// проверить Uri и файл
        /// </summary>
        public void CheckUrlAndFile(out string fileName)
        {
            CheckUrl(out fileName);
            CheckFileOrCreateFile();

            this.HasChecked = true;
        }
        /// <summary>
        ///Проверить uri
        /// </summary>
        public void CheckUrl(out string fileName)
        {
            fileName = SaveFile.CheckUrl(this);
        }
        /// <summary>
        ///  Проверить, существует ли файл назначения. Если нет, то создайте файл с тем же
        /// размер файла для загрузки.
        /// </summary>
        void CheckFileOrCreateFile()
        {
            SaveFile.CheckFileOrCreateFile(this, locker);
        }
        /// <summary>
        /// Начать загрузку
        /// </summary>
       public void BeginDownload()
       {
           if (this.Status != StateDownload.Created)
           {
               throw new ApplicationException("Если не создана загрузка, нельзя начать её скачивать.");
           }

           this.Status = StateDownload.Downloading;
           ThreadPool.QueueUserWorkItem(DownloadInternal);
       }

       void DownloadInternal(object obj)
       {
           if (this.Status != StateDownload.Downloading)
           {
               return;
           }
           try
           {
               if (this.StartPoint < 0)
               {     throw new ArgumentOutOfRangeException("StartPoint не может быть меньше 0");
               }
               if (this.EndPoint < this.StartPoint)
               {
                   throw new ArgumentOutOfRangeException("EndPoint не может быть меньше StartPoint");
               }           
               if (this.MaxThreads < 1)
               {
                   throw new ArgumentOutOfRangeException( "Максимальное количество потоков не может быть меньше 1");
               }
               this.Status = StateDownload.Downloading;

               if (!this.HasChecked)
               {
                   string filename = null;
                   this.CheckUrlAndFile(out filename);
               }               
               // Если файл не поддерживает "Accept-Ranges" в заголовке, а затем создать один
                // DownloadTread, чтобы загрузить файл в один поток, в противном случае создания
               // Несколько DownloadTread, чтобы загрузить файл.

               if (!IsRangeSupported)
               {
                   DownloadTread down = new DownloadTread(
                       this.Url.AbsoluteUri,
                       0,
                       long.MaxValue,
                       this.BufferSize,
                       this.BufferCountPerNotification * this.BufferSize,
                       this.BufferCountPerNotification);

                   down.TotalSize = this.TotalSize;
                   down.DownloadPath = this.DownloadPath;
                   down.HasChecked = true;
                   down.Credentials = this.Credentials;
                   down.Proxy = this.Proxy;
                   down.DownloadProgressChanged += client_DownloadProgressChanged;//?
                   down.StatusChanged += client_StatusChanged;
                   down.DownloadCompleted += client_DownloadCompleted;
                   this.downloadTread.Add(down);
               }
               else
               {
                   // Вычислить размер блока для каждого клиента, чтобы скачать.
                   int maxSizePerThread =(int)Math.Ceiling((double)this.TotalSize / this.MaxThreads);
                   if (maxSizePerThread < this.MaxCacheSize)
                   {
                       maxSizePerThread = this.MaxCacheSize;
                   }

                   long leftSizeToDownload = this.TotalSize;

                   // Реальное количество потоков номер минимальное значение MaxThreads и TotalSize / MaxCacheSize.              
                   int threadsCount = (int)Math.Ceiling((double)this.TotalSize / maxSizePerThread);

                   for (int i = 0; i < threadsCount; i++)
                   {
                       long endPoint = maxSizePerThread * (i + 1) - 1;
                       long sizeToDownload = maxSizePerThread;

                       if (endPoint > this.TotalSize)
                       {
                           endPoint = this.TotalSize - 1;
                           sizeToDownload = endPoint - maxSizePerThread * i;
                       }
                       // Скачать блок файла
                       DownloadTread down = new DownloadTread(this.Url.AbsoluteUri, maxSizePerThread * i,endPoint);

                       // Установить флаг HasChecked, так что клиент не будет проверять URL снова.
                       down.DownloadPath = this.DownloadPath;
                       down.HasChecked = true;
                       down.TotalSize = sizeToDownload;
                       down.Credentials = this.Credentials;
                       down.Proxy = this.Proxy;

                       this.downloadTread.Add(down);
                   }
               }
               // Установить lastStartTime для расчета использовались времени.
               lastStartTime = DateTime.Now;
               // Выполнение  DownloadTread
               foreach (var down in this.downloadTread)
               {
                   if (this.Proxy != null)
                   {
                       down.Proxy = this.Proxy;
                   }
                   // Регистрация событий DownloadTread
                   down.StatusChanged += client_StatusChanged;
                   down.BeginDownload();
               }
           }
           catch (Exception ex)
           {
               this.Status = StateDownload.Nofile;
               Console.WriteLine("Ошибка: " + ex.ToString());
               this.Cancel();
               this.OnDownloadCompleted(new DownloadCompletedEventArgs( null,  this.DownloadedSize, this.TotalSize, this.TotalUsedTime, ex));
          }
       }
       /// <summary>
       /// Пауза
       /// </summary>
       public void Pause()
       {
           if (this.Status != StateDownload.Downloading)
           {
               throw new ApplicationException("Загрузка не может быть приостановлена.");
           }
           this.Status = StateDownload.Paused;
           // Приостановить все DownloadTread. Если все клиенты паузу,
           // cтатус загрузчик будет изменен на приостановлен.
           if (downloadTread != null)
           {
               foreach (var down in this.downloadTread)
               {
                   if (down.Status == StateDownload.Downloading)
                   {
                       down.Pause();
                   }
               }
           }
       }
        /// <summary>
       /// Возобновить скачивание
       /// </summary>
       public void BeginResume()
       {
           if (this.Status != StateDownload.Paused)
           {
               throw new ApplicationException("Возобновить только после паузы");
           }

           lastStartTime = DateTime.Now;
           this.Status = StateDownload.Downloading;
           if (downloadTread != null)
           {
               foreach (var client in this.downloadTread)
               {
                   if (client.Status != StateDownload.Completed)
                   {
                       client.BeginResume();
                   }
               }
           }
       }
       /// <summary>
       /// Отмена загрузки
       /// </summary>
       public void Cancel()
       {
           if (this.Status == StateDownload.Created
              || this.Status == StateDownload.Downloading
              || this.Status == StateDownload.Completed
              || this.Status == StateDownload.Paused
              || this.Status == StateDownload.Stopped)
           {
               this.Status = StateDownload.Stopped;
           }
           if (downloadTread != null)
           {
               foreach (var client in this.downloadTread)
               {
                   client.Cancel();
               }
           }
       }
       /// <summary>
       /// Обработка StatusChanged случае все DownloadTread
       /// </summary>
       void client_StatusChanged(object sender, EventArgs e)
       {   // Если все клиенты будут Completed, то статус этого загрузчик завершен.
           if (this.downloadTread.All( client => client.Status == StateDownload.Completed))
           {
               this.Status = StateDownload.Completed;
             Server.StatusChanged(this);
           }
           // все клиенты будут Stopped, то статус этого загрузчик Stopped
            else if (this.downloadTread.All( client => client.Status == StateDownload.Stopped))
           {
               this.Status = StateDownload.Stopped;
               Server.StatusChanged(this);
           }
           else
           {  // Завершившие клиенты не будут приниматься во внимание
               var nonCompletedClients = this.downloadTread.Where(client => client.Status != StateDownload.Completed);

               // Если все nonCompletedClients - Downloading, то загрузчик - Downloading
               if (nonCompletedClients.All( client => client.Status == StateDownload.Downloading))
               {
                   this.Status = StateDownload.Downloading;
                   Server.StatusChanged(this);
               }

               // Если все nonCompletedClients - Paused, то загрузчик - Paused
               else if (nonCompletedClients.All(client => client.Status == StateDownload.Paused))
               {
                   this.Status = StateDownload.Paused;
                   Server.StatusChanged(this);
               }
               else if (nonCompletedClients.All(client => client.Status == StateDownload.Nofile))
               {
                   this.Status = StateDownload.Nofile;
                   Server.StatusChanged(this);
               }
           }
       }
       /// <summary>
       /// Событие DownloadProgressChanged для всех
       /// </summary>
       void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
       {
           lock (locker)
           {
               if (DownloadProgressChanged != null)
               {
                   int speed = 0;
                   DateTime current = DateTime.Now;
                   TimeSpan interval = current - lastNotificationTime;

                   if (interval.TotalSeconds < 60)
                   {
                       speed = (int)Math.Floor((this.DownloadedSize + this.CachedSize -
                           this.lastNotificationDownloadedSize) / interval.TotalSeconds);
                   }

                   lastNotificationTime = current;
                   lastNotificationDownloadedSize = this.DownloadedSize + this.CachedSize;

                   var downloadProgressChangedEventArgs =
                       new DownloadProgressChangedEventArgs(
                           DownloadedSize, TotalSize, speed);
                   this.OnDownloadProgressChanged(downloadProgressChangedEventArgs);
                   Server.SignalCompleting(this.HostName, this.id, this.Status, this.Url.AbsoluteUri, this.TotalSize, this.DownloadedSize, this.TotalUsedTime);
               }

           }
       }
       /// <summary>
       /// Событие DownloadCompleted для всех
       /// </summary>
       /// <param name="e"></param>
       /// <param name="sender"></param>
        void client_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
       {
           if (e.Error != null && this.Status != StateDownload.Stopped)
           {
               this.Cancel();
               this.OnDownloadCompleted(new DownloadCompletedEventArgs( null, this.DownloadedSize, this.TotalSize, this.TotalUsedTime,e.Error));
               Server.SignalCompleting(this.HostName, this.id, this.Status, this.Url.AbsoluteUri, this.TotalSize, this.DownloadedSize,this.TotalUsedTime);
           }
       }
       /// <summary>
       /// Событие StatusChanged
       /// </summary>
       /// <param name="e"></param>
       protected virtual void OnStatusChanged(EventArgs e)
       {

           switch (this.Status)
           {
               case StateDownload.Created:
               case StateDownload.Downloading:
               case StateDownload.Paused:
               case StateDownload.Stopped:
               case StateDownload.Completed:
                   if (this.StatusChanged != null)
                   {
                       this.StatusChanged(this, e);
                   }
                   break;
               default:
                   break;
           }

           if (this.Status == StateDownload.Paused
               || this.Status == StateDownload.Stopped
               || this.Status == StateDownload.Completed)
           {
               this.usedTime += DateTime.Now - lastStartTime;
           }

           if (this.Status == StateDownload.Stopped)
           {
               Exception ex = new Exception("Загрузка отменена пользователем. ");
               this.OnDownloadCompleted(new DownloadCompletedEventArgs( null,this.DownloadedSize,  this.TotalSize,  this.TotalUsedTime,ex));
           }

           if (this.Status == StateDownload.Completed)
           {
               this.OnDownloadCompleted( new DownloadCompletedEventArgs(new FileInfo(this.DownloadPath), this.DownloadedSize,  this.TotalSize, this.TotalUsedTime,null));
           }
       }
       /// <summary>
       /// Событие DownloadCompleted 
       /// </summary>
       protected virtual void OnDownloadCompleted(DownloadCompletedEventArgs e)
       {
           if (DownloadCompleted != null)
           {
               DownloadCompleted(this, e);
           }
       }
       /// <summary>
       /// Событие DownloadProgressChanged event
       /// </summary>
       /// <param name="e"></param>
       protected virtual void OnDownloadProgressChanged( DownloadProgressChangedEventArgs e)
       {
           if (DownloadProgressChanged != null)
           {
               DownloadProgressChanged(this, e);
           }
       }
       /// <summary>
       /// Загужает данные о закачке из xml файла
       /// </summary>
       /// <param name="reader"></param>
       /// <returns></returns>   
       public static Download FromXml(XmlReader reader)
       {
           Download result = new Download();

           reader.ReadStartElement("download");

           if (reader.Name != "uri") throw new FormatException();
           result.Url = new Uri(reader.ReadString());
           reader.Read();
           if (reader.Name != "filePath") throw new FormatException();

           string filePath = reader.ReadString();
           result.DownloadPath = Path.GetDirectoryName(filePath);
           result.FileName = Path.GetFileName(filePath);
           reader.Read();

           if (reader.Name != "size") throw new FormatException();
           result.TotalSize = Int64.Parse(reader.ReadString());
           reader.Read();
           if (reader.Name != "startpoint") throw new FormatException();
           result.StartPoint = long.Parse(reader.ReadString());
           reader.Read();
           if (reader.Name != "endpoint") throw new FormatException();
           result.EndPoint= long.Parse(reader.ReadString());
           reader.Read();
           if (reader.Name != "downloadState") throw new FormatException();
           result.Status = (StateDownload)(Convert.ToInt32(reader.ReadString()));
           //reader.Read();
           
           reader.ReadEndElement();
           return result;
       }
       /// <summary>
       /// Записывает данные о закачке из xml файла
       /// </summary>
       /// <param name="reader"></param>
       /// <returns></returns>   
        public void ToXml(XmlWriter writer)
       {
           writer.WriteStartElement("download");
           writer.WriteElementString("uri", Url.ToString());
           writer.WriteElementString("filePath", FileName);
           writer.WriteElementString("size", TotalSize.ToString());
           writer.WriteElementString("startpoint", StartPoint.ToString());
           writer.WriteElementString("endpoint", EndPoint.ToString());
           writer.WriteElementString("downloadState", ((int)Status).ToString());
           writer.WriteEndElement();
       }
    }
}
