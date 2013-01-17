using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using DownloadLibrary;
using System.Net;
namespace Server
{
    public class SaveFile
    {
        public static HttpWebRequest InitializeHttpWebRequest(IDownloader downloader)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(downloader.Url);

            if (downloader.Credentials != null)
            {
                webRequest.Credentials = downloader.Credentials;
            }
            else
            {
                webRequest.Credentials = CredentialCache.DefaultCredentials;
            }

            if (downloader.Proxy != null)
            {
                webRequest.Proxy = downloader.Proxy;
            }
            else
            {
                webRequest.Proxy = WebRequest.DefaultWebProxy;
            }

            return webRequest;
        }
        /// <summary>
        /// Проверить URL 
        /// </summary>
        /// <param name="downloader"></param>
        /// <returns></returns>
        public static string CheckUrl(IDownloader downloader)
        {
            string fileName = string.Empty;
            var webRequest = InitializeHttpWebRequest(downloader);

            using (var response = webRequest.GetResponse())
            {
                foreach (var header in response.Headers.AllKeys)
                {
                    if (header.Equals("Accept-Ranges", StringComparison.OrdinalIgnoreCase))
                    {
                        downloader.IsRangeSupported = true;
                    }

                    if (header.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase))
                    {
                        string contentDisposition = response.Headers[header];

                        string pattern = ".[^;]*;\\s+filename=\"(?<file>.*)\"";
                        Regex r = new Regex(pattern);
                        Match m = r.Match(contentDisposition);
                        if (m.Success)
                        {
                            fileName = m.Groups["file"].Value;
                        }
                    }
                }
                //--------
                // downloader.IsRangeSupported = true;

                fileName = MessageClientServer.GenerateFileNameFromUri(downloader.Url.AbsoluteUri);

                //-----------
                downloader.TotalSize = response.ContentLength;

                if (downloader.TotalSize <= 0)
                {
                    downloader.Status = StateDownload.Nofile;
                    throw new ApplicationException("Файл не найден");                   
                }

                if (!downloader.IsRangeSupported)
                {
                    downloader.StartPoint = 0;
                    downloader.EndPoint = int.MaxValue;
                }
            }

            if (downloader.IsRangeSupported &&
                (downloader.StartPoint != 0 || downloader.EndPoint != long.MaxValue))
            {
                webRequest = InitializeHttpWebRequest(downloader);

                if (downloader.EndPoint != int.MaxValue)
                {
                    webRequest.AddRange(downloader.StartPoint, downloader.EndPoint);
                }
                else
                {
                    webRequest.AddRange(downloader.StartPoint);
                }
                using (var response = webRequest.GetResponse())
                {
                    downloader.TotalSize = response.ContentLength;
                }
            }

            return fileName;
        }

        /// <summary>
        /// Проверить, существует ли файл назначения. Если нет, то создайте файл с тем же размер файла для загрузки.
        /// </summary>
        public static void CheckFileOrCreateFile(IDownloader downloader, object fileLocker)
        {
            lock (fileLocker)
            {
                FileInfo fileToDownload = new FileInfo(downloader.DownloadPath);
                if (fileToDownload.Exists)
                {
                    if (fileToDownload.Length != downloader.TotalSize)
                    {
                        downloader.Status = StateDownload.Nofile;
                        throw new ApplicationException("Загрузка невозможна");

                    }
                }
                // Создать файл
                else
                {
                    if (downloader.TotalSize == 0)
                    {
                        downloader.Status = StateDownload.Nofile;
                        throw new ApplicationException("Файл невозможно создать");
                    }

                    using (FileStream fileStream = File.Create(downloader.DownloadPath))
                    {
                        long createdSize = 0;
                        byte[] buffer = new byte[4096];
                        while (createdSize < downloader.TotalSize)
                        {
                            int bufferSize = (downloader.TotalSize - createdSize) < 4096
                                ? (int)(downloader.TotalSize - createdSize) : 4096;
                            fileStream.Write(buffer, 0, bufferSize);
                            createdSize += bufferSize;
                        }
                    }
                }
            }
        }

    }
}
