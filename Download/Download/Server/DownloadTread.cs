using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using DownloadLibrary;

namespace Server
{       
     class DownloadTread : IDownloader
    {   /// <summary>
        /// Используется, когда создает или записывает в файл.
        /// </summary>
        static object fileLocker = new object();
        /// <summary>
        /// Имя скачиваемого файла
        /// </summary>
        public string FileName { get; set; }
        object statusLocker = new object();
         /// <summary>
        /// Идентификатор ресурса
        /// </summary>
        public Uri Url { get; private set; }
        /// <summary>
        /// Локальный путь для сохранения файлов
        /// </summary>
        public string DownloadPath { get; set; }
        /// <summary>
        /// Общий размер
        /// </summary>
        public long TotalSize { get; set; }
        /// <summary>
        /// Сетевые учетные данные, используемые для проверки подлинности запроса
        /// </summary>
        public ICredentials Credentials { get; set; }
         /// <summary>
        /// Прокси 
        /// </summary>
        public IWebProxy Proxy { get; set; }
        /// <summary>
        /// Укажите, следует ли удаленный сервер поддерживает "Accept-Ranges" заголовка.
        /// Используйте CheckUrl метод для инициализации этого свойства внутренне.
        /// </summary>
        public bool IsRangeSupported { get; set; }
        ///<summary>
        /// Свойство начальная точка может быть использована в многопоточной загрузки сценария, и
        /// Каждый поток начинает загружать конкретный блок файла
        ///</summary>
        public long StartPoint { get; set; }
        ///<summary>
        /// Конечная точка
        ///</summary>
        public long EndPoint { get; set; }
        ///<summary>
        /// Размер загруженных данных, который был прописан в локальном файле
        ///</summary>
        public long DownloadedSize { get; private set; }
        ///<summary>
        /// Размер кэша
        ///</summary>
        public int CachedSize { get; private set; }
        ///<summary>
        /// Флаг возможности загрузки
        ///</summary>
        public bool HasChecked { get; set; }       
        StateDownload status;
        /// <summary>
        /// Состояние загрузки.Если состояние изменилось, вызываем событие StatusChanged
        /// </summary>
        public StateDownload Status
        {
            get
            {
                return status;
            }
            set
            {
                lock (statusLocker)
                {
                    if (status != value)
                    {
                        status = value;
                        this.OnStatusChanged(EventArgs.Empty);
                    }
                }
            }
        }
        /// <summary>
        /// Сохраняем использованное время, проведенное в загрузке данных. Стоимость не включает
        /// паузу времени, и он будет обновляться только когда загрузка приостановлена или завершена
        /// </summary>
        private TimeSpan usedTime = new TimeSpan();
        /// <summary>
        /// Время начала работы
        /// </summary>
        private DateTime lastStartTime;
        /// <summary>
        /// Общее время посчитанное
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
        /// <summary>
        /// Время DownloadProgressChanged, используется для расчета скорости загрузки
        /// </summary>        
        private DateTime lastNotificationTime;
        /// <summary>
        /// Размер DownloadProgressChanged, используется для расчета скорости загрузки
        /// </summary>
        private Int64 lastNotificationDownloadedSize;
        /// <summary>
        /// Если получим число буферов, затем событие DownloadProgressChanged 
        /// </summary>
        public int BufferCountPerNotification { get; set; }
        /// <summary>
        /// Установить BufferSize при чтении данных в поток ответа
        /// </summary>        
        public int BufferSize { get; set; }
        /// <summary>
        /// Размер кэша в памяти
        /// </summary>
        public int MaxCacheSize { get; set; }

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        public event EventHandler StatusChanged;
        /// <summary>
        /// Скачать весь файл
        /// </summary>
        public DownloadTread(string url) : this(url, 0)
        {        }
        /// <summary>
        /// Скачать файл с начальной точки до конца
        /// </summary>
        public DownloadTread(string url, long startPoint): this(url, startPoint, int.MaxValue)
        {        }
        /// <summary>
        /// Скачать блок файла. Размер буфера по умолчанию равен 1 Кб, кэш-память является
        /// 1 Мб, а количество буферов в уведомлении 64.
        /// </summary>
        public DownloadTread(string url, long startPoint, long endPoint) : this(url, startPoint, endPoint, 1024, 1048576, 64)
        {        }
        public DownloadTread(string url, long startPoint, long endPoint, int bufferSize, int cacheSize, int bufferCountPerNotification)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.BufferSize = bufferSize;
            this.MaxCacheSize = cacheSize;
            this.BufferCountPerNotification = bufferCountPerNotification;

            this.Url = new Uri(url, UriKind.Absolute);
            // Установить значения по умолчанию IsRangeSupported.
            this.IsRangeSupported = true;

            // Установить Created статус.
            this.status = StateDownload.Created;
        }
       
        /// <summary>
        /// Проверяем Uri, чтобы найти его размер, и поддерживает ли он "Пауза".
        /// </summary>
        public void CheckUrl(out string fileName)
        {
            fileName = SaveFile.CheckUrl(this);            
        }
        /// <summary>
        /// Проверить, существует ли файл назначения. Если нет, то создайте файл с тем же размер файла для загрузки.
        /// </summary>
        void CheckFileOrCreateFile()
        {
           SaveFile.CheckFileOrCreateFile(this, fileLocker);
        }
        /// <summary>
        /// Проверка URL
        /// </summary>
        void CheckUrlAndFile(out string fileName)
        {
            CheckUrl(out fileName);
            CheckFileOrCreateFile();

            this.HasChecked = true;
        }
        /// <summary>
        /// Проверка свойств
        /// </summary>
        void EnsurePropertyValid()
        {
            if (this.StartPoint < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "StartPoint не может быть меньше 0");
            }

            if (this.EndPoint < this.StartPoint)
            {
                throw new ArgumentOutOfRangeException(
                    "EndPoint не может быть меньше StartPoint ");
            }

            if (this.BufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "BufferSize не может быть меньше 0 ");
            }

            if (this.MaxCacheSize < this.BufferSize)
            {
                throw new ArgumentOutOfRangeException(
                    "MaxCacheSize не может быть меньше, чем BufferSize ");
            }

            if (this.BufferCountPerNotification <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "BufferCountPerNotification не может быть меньше 0. ");
            }
        }
        /// <summary>
        /// Стартовать загрузку
        /// </summary>
        public void Download()
        {
            if (this.Status != StateDownload.Created)
            {
                throw new ApplicationException("Закачка не создана.");
            }

            this.Status = StateDownload.Downloading;
            //Скачивать в том же потоке
            DownloadInternal(null);
        }
        /// <summary>
        /// Начало загрузить с помощью ThreadPool.
        /// </summary>
        public void BeginDownload()
        {
            if (this.Status != StateDownload.Created)
            {
                throw new ApplicationException("Закачка не создана");
            }
            this.Status = StateDownload.Downloading;
            ThreadPool.QueueUserWorkItem(DownloadInternal);//помещает в очередь на выполнение
        }
        /// <summary>
        /// Пауза загрузки
        /// </summary>
        public void Pause()
        {
            switch (this.Status)
            {
                case StateDownload.Downloading:
                    this.Status = StateDownload.Paused;
                    break;
                default:
                    throw new ApplicationException("Загрузка не может быть приостановлена");
            }
        }

        /// <summary>
        /// Возобновить скачивание
        /// </summary>
        public void Resume()
        {
            if (this.Status != StateDownload.Paused)
            {
                throw new ApplicationException("Возобновить только после паузы");
            }
            this.Status = StateDownload.Downloading;
            DownloadInternal(null);
        }

        /// <summary>
        /// Возобновить скачивание ThreadPool.
        /// </summary>
        public void BeginResume()
        {
            if (this.Status != StateDownload.Paused)
            {
                throw new ApplicationException("Возобновить только после паузы");
            }
            this.Status = StateDownload.Downloading;
            ThreadPool.QueueUserWorkItem(DownloadInternal);
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
        }
        /// <summary>
        ///  Скачать данные с помощью HttpWebRequest. Он будет читать буфер байт из
        /// ответа потоком и сохранить буфер в MemoryStream кэша первого.
        /// Если кэш заполнен или загрузка приостановлена, отменена или завершены, пишит данные в кэше файла.
        /// </summary>
        void DownloadInternal(object obj)
        {
            if (this.Status != StateDownload.Downloading)
            {  return;
            }         
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            Stream responseStream = null;
            MemoryStream downloadCache = null;
            this.lastStartTime = DateTime.Now;
            try
            {                
                if (!HasChecked)
                {
                    string filename = string.Empty;
                    CheckUrlAndFile(out filename);
                }

                this.EnsurePropertyValid();
                //this.Status = StateDownload.Downloading;
                // Создаем запрос к файлу для загрузки.
                webRequest = SaveFile.InitializeHttpWebRequest(this);

                // Блок для загрузки
                if (EndPoint != int.MaxValue)
                {
                    webRequest.AddRange(StartPoint + DownloadedSize, EndPoint);
                }
                else
                {
                    webRequest.AddRange(StartPoint + DownloadedSize);
                }

                // Получаем ответ от сервера и получить response поток.
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
                // Кэш данных в памяти.
                downloadCache = new MemoryStream(this.MaxCacheSize);

                byte[] downloadBuffer = new byte[this.BufferSize];

                int bytesSize = 0;
                CachedSize = 0;
                int receivedBufferCount = 0;
                while (true)
                {
                    // Читать буфер данных из потока.
                    bytesSize = responseStream.Read(downloadBuffer, 0, downloadBuffer.Length);
                    // Если кэш заполнен или загрузка приостановлена, отменена или
                    // Завершения записи данных в кэше локального файла.
                    if (this.Status != StateDownload.Downloading
                        || bytesSize == 0 || this.MaxCacheSize < CachedSize + bytesSize)
                    {
                        try
                        {
                            // Запись данных в кэше локального файла.
                            WriteCacheToFile(downloadCache, CachedSize);
                            this.DownloadedSize += CachedSize;
                           if (this.Status != StateDownload.Downloading || bytesSize == 0)
                            {  break;  }
                           // Сброс кэша
                            downloadCache.Seek(0, SeekOrigin.Begin);
                            CachedSize = 0;
                        }
                        catch (Exception ex)
                        {
                            this.OnDownloadCompleted( new DownloadCompletedEventArgs(null, this.DownloadedSize, this.TotalSize, this.TotalUsedTime,ex));
                            return;
                        }
                    }
                    // Запись данных из буфера в кэш в памяти
                    downloadCache.Write(downloadBuffer, 0, bytesSize);
                    CachedSize += bytesSize;
                    receivedBufferCount++;
                    if (receivedBufferCount == this.BufferCountPerNotification)
                    {
                        InternalDownloadProgressChanged(CachedSize);
                        receivedBufferCount = 0;
                    }
                }
                // Обновление использованного времени
                usedTime = usedTime.Add(DateTime.Now - lastStartTime);                
                    this.Status = StateDownload.Completed;              
            }
            catch (Exception ex)
            {
                this.OnDownloadCompleted(new DownloadCompletedEventArgs(null,this.DownloadedSize, this.TotalSize,this.TotalUsedTime,ex));
                return;
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                }
                if (downloadCache != null)
                {
                    downloadCache.Close();
                }
            }
        }
        /// <summary>
        /// Запись данных в кэше локального файла.
        /// </summary>
        void WriteCacheToFile(MemoryStream downloadCache, int cachedSize)
        {
            // Блокировки других потоков или процессов, для предотвращения записи данных в файл
            lock (fileLocker)
            {
                using (FileStream fileStream = new FileStream(DownloadPath, FileMode.Open))
                {
                    byte[] cacheContent = new byte[cachedSize];
                    downloadCache.Seek(0, SeekOrigin.Begin);
                    downloadCache.Read(cacheContent, 0, cachedSize);
                    fileStream.Seek(DownloadedSize + StartPoint, SeekOrigin.Begin);
                    fileStream.Write(cacheContent, 0, cachedSize);
                }
            }
        }
        /// <summary>
        /// Если загрузка завершена успешно
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDownloadCompleted(DownloadCompletedEventArgs e)
        {
            if (e.Error != null && this.status != StateDownload.Stopped)
            {
                this.Status = StateDownload.Completed;
            }
            if (DownloadCompleted != null)
            {
                DownloadCompleted(this, e);
            }
        }
        /// <summary>
        /// Рассчитать скорость загрузки 
        /// </summary>
        /// <param name="cachedSize"></param>
        private void InternalDownloadProgressChanged(int cachedSize)
        {
            int speed = 0;
            DateTime current = DateTime.Now;
            TimeSpan interval = current - lastNotificationTime;

            if (interval.TotalSeconds < 60)
            {
                speed = (int)Math.Floor((this.DownloadedSize + cachedSize - this.lastNotificationDownloadedSize) / interval.TotalSeconds);
            }
            lastNotificationTime = current;
            lastNotificationDownloadedSize = this.DownloadedSize + cachedSize;
           // this.OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(this.DownloadedSize + cachedSize, this.TotalSize, speed));
        }
        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, e);
            }
        }
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

            if (this.status == StateDownload.Stopped)
            {
                Exception ex = new Exception("Загрузка отменена пользователем");
                this.OnDownloadCompleted(new DownloadCompletedEventArgs( null,this.DownloadedSize,this.TotalSize,this.TotalUsedTime,ex));
            }
            if (this.Status == StateDownload.Completed)
            {
                this.OnDownloadCompleted(new DownloadCompletedEventArgs(new FileInfo(this.DownloadPath),this.DownloadedSize,this.TotalSize,this.TotalUsedTime,null));
            }
        }
    }
}

