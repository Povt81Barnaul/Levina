using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DownloadLibrary;
using System.IO;

namespace Server
{
    public interface IDownloader
    {

        #region Данные загрузки
        Uri Url { get; }
        string DownloadPath { get; set; }
        long TotalSize { get; set; }

        ICredentials Credentials { get; set; }
        IWebProxy Proxy { get; set; }
        #endregion

        bool IsRangeSupported { get; set; }
        long StartPoint { get; set; }
        long EndPoint { get; set; }
        string FileName { get; set; }
        #region Данные загрузки и статус
        long DownloadedSize { get; }
        int CachedSize { get; }

        bool HasChecked { get; set; }
        StateDownload Status { get; set; }
        TimeSpan TotalUsedTime { get; }
        #endregion

        #region Установленные данные загрузки
        int BufferSize { get; set; }
        int BufferCountPerNotification { get; set; }
        int MaxCacheSize { get; set; }
        #endregion
        event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        event EventHandler StatusChanged;

        void CheckUrl(out string fileName);

        void BeginDownload();

        void Pause();

        void BeginResume();

        void Cancel();
    }
    public class DownloadProgressChangedEventArgs : EventArgs
    {
        public Int64 ReceivedSize { get; private set; }
        public Int64 TotalSize { get; private set; }
        public int DownloadSpeed { get; private set; }

        public DownloadProgressChangedEventArgs(Int64 receivedSize,
            Int64 totalSize, int downloadSpeed)
        {
            this.ReceivedSize = receivedSize;
            this.TotalSize = totalSize;
            this.DownloadSpeed = downloadSpeed;
        }
    }
    public class DownloadCompletedEventArgs : EventArgs
    {
        public Int64 DownloadedSize { get; private set; }
        public Int64 TotalSize { get; private set; }
        public Exception Error { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public FileInfo DownloadedFile { get; private set; }

        public DownloadCompletedEventArgs(
            FileInfo downloadedFile, Int64 downloadedSize,
            Int64 totalSize, TimeSpan totalTime, Exception ex)
        {
            this.DownloadedFile = downloadedFile;
            this.DownloadedSize = downloadedSize;
            this.TotalSize = totalSize;
            this.TotalTime = totalTime;
            this.Error = ex;
        }
    }
    
}
