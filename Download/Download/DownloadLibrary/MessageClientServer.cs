using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DownloadLibrary
{

    public enum StateDownload
    {
        Completed = 0,
        Downloading = 1,
        Paused = 2,
        Created = 3,
        Stopped = 4,
        Nofile = 5
    }
    /// <summary>
    /// Сообщения для обмена между сервером и клиентом
    /// </summary>
    public class MessageClientServer
    {
            /// <summary>
            /// Соединение
            /// </summary>
            public const String CONNECT = "CONNECT";
            /// <summary>
            /// Соединение успешно
            /// </summary>
            public const String COOL = "COOL";
            /// <summary>
            /// Соединение неуспешно
            /// </summary>
            public const String NOCOOL = "NOCOOL";
            /// <summary>
            /// Закрытие соединения
            /// </summary>
            public const String CLOSE_CONNECT = "CLOSE";
            /// <summary>
            /// Старт загрузки
            /// </summary>
            public const String START = "START";
            /// <summary>
            /// Старт имеющуюся загрузки
            /// </summary>
            public const String LAST = "LAST";
            /// <summary>
            /// Стоп загрузки
            /// </summary>
            public const String STOP = "STOP";
            /// <summary>
            /// Пауза загрузки
            /// </summary>
            public const String PAUSE = "PAUSE";
            /// <summary>
            /// Загружается
            /// </summary>
            public const String DOWNLOADING = "DOWNLOADING";
            /// <summary>
            /// Завершена загрузка
            /// </summary>
            public const String COMPLETED = "COMPLETED";
            /// <summary>
            /// Удалить загрузку
            /// </summary>
            public const String DELETE = "DELETE";
            /// <summary>
            /// Не загружается
            /// </summary>
            public const String NODOWNLOADING = "NODOWNLOADING";
            /// <summary>
            /// Информационное сообщение
            /// </summary>
            public const String INFO = "INFO";
            /// <summary>
            /// Выключение сервера
            /// </summary>
            public const String EXIT_SERVER = "EXIT_SERVER";

            /// <summary>
            /// Генерирует имя файла из uri
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            public static string GenerateFileNameFromUri(string url)
            {
                Regex r = new Regex(@"^http://[\w/\.\-:|]+/(?<file_name>[\w\.\s|\-]+)", RegexOptions.Compiled);

                string result;

                try
                {
                    result = r.Match(url).Result("${file_name}");
                }
                catch (Exception)
                {
                    try
                    {
                        result = Regex.IsMatch(url, @"^http://[\w/\.\-:|/]+") ? "index.html" : string.Empty;
                    }
                    catch (Exception)
                    {
                        result = string.Empty;
                    }
                }
                return result;
            }
        }
}

