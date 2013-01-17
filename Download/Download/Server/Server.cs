using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using DownloadLibrary;
using System.Xml;
namespace Server
{
        public class Server
        {
            private Thread _serverThread;
            private IPEndPoint _serverAddress;
            private TcpListener _serverListener;
           static private List<ConnectionInfo> _connections;
            private int _maxConnectCount;
            public List<Download> sf = new List<Download>();
            WebProxy proxy = null;
            /// <summary>
            /// Информация о соединении
            /// </summary>
            public class ConnectionInfo : IDisposable
            {
                public Socket Socket;
                public Thread Thread;
                public String HostName;
                public NetworkStream Stream;
                public BinaryWriter Writer;
                public BinaryReader Reader;
                public int id;
                //public StateConputer state;
                public bool isPlay;

                /// <summary>
                /// Новое соединение
                /// </summary>
                /// <param name="socket">Сокет</param>
                /// <param name="id">ID</param>
                public ConnectionInfo(Socket socket, int id)
                {
                    this.Socket = socket;
                    this.Stream = new NetworkStream(this.Socket);
                    this.Reader = new BinaryReader(this.Stream);
                    this.Writer = new BinaryWriter(this.Stream);
                    this.id = id;
                    this.isPlay = false;
                }

                /// <summary>
                /// Строковое представление
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    if (!string.IsNullOrWhiteSpace(HostName))
                        return String.Format("{0}", HostName);
                    return "Определяется имя хоста...";
                }

                /// <summary>
                /// Уничтожение объекта
                /// </summary>
                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                //Освобожение ресурсов
                protected virtual void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        if (Socket != null)
                        {
                            Socket.Dispose();
                            Socket = null;
                        }
                        if (Stream != null)
                        {
                            Stream.Dispose();
                            Stream = null;
                        }
                        if (Writer != null)
                        {
                            Writer.Dispose();
                            Writer = null;
                        }
                        if (Reader != null)
                        {
                            Reader.Dispose();
                            Reader = null;
                        }
                    }
                }

                //Деструктор
                ~ConnectionInfo()
                {
                    Dispose(false);
                }
            }

            /// <summary>
            /// Новый сервер
            /// </summary>
            /// <param name="ipEndPoint">Структура: ip адрес и порт</param>
            public Server(IPEndPoint serverAddress)
            {
                _serverAddress = serverAddress;
                _connections = new List<ConnectionInfo>();
                _serverListener = new TcpListener(_serverAddress);
                
                _serverThread = new Thread(AcceptConnections) { IsBackground = true };
                _serverThread.Start();
            }

            //Принимаем новые соединения (обработку каждого помещаем в отдельный поток)
            private void AcceptConnections()
            {
                _serverListener.Start();
                while (true)
                {
                    Socket socket = _serverListener.AcceptSocket();
                    _maxConnectCount++;

                    ConnectionInfo connection = new ConnectionInfo(socket, _maxConnectCount)
                    {
                        Thread = new Thread(ProcessConnection) { IsBackground = true }
                    };
                    connection.Thread.Start(connection);

                    lock (_connections)
                        _connections.Add(connection);
                }
            }

            //Обработка отдельного соединения
            private void ProcessConnection(object state)
            {
                ConnectionInfo connection = (ConnectionInfo)state;
                try
                {

                    bool flag = true;
                    do
                    {
                        string cmd = connection.Reader.ReadString();

                        if (cmd.Contains(MessageClientServer.CONNECT))
                        {
                            try
                            {
                                string[] str = cmd.Split(' ');
                                connection.HostName = str[1];
                                Console.WriteLine("Соединение с {0} \n", connection.HostName);
                                SignalCool(connection);
                            }catch(Exception er)
                            {
                                Console.WriteLine(er.ToString());
                                SignalNoCool(connection);
                            }
                        }
                        else
                        {
                            if (cmd.Contains(MessageClientServer.START)) //Cигнал на загрузку файла
                            {
                                string[] str = cmd.Split(' ');
                                string uri = str[1];
                                int maxThread = Convert.ToInt32(str[2]);
                                Download d = new Download(uri,maxThread);
                                d.Proxy = this.proxy;
                                string filename = string.Empty;
                                d.FileName=MessageClientServer.GenerateFileNameFromUri(uri);
                                d.HostName = connection.HostName;
                                d.id = connection.id;
                                d.DownloadPath = d.DownloadPath + @"\"+d.FileName;
                                d.DownloadCompleted += DownloadCompleted;
                                d.DownloadProgressChanged += DownloadProgressChanged;
                                d.StatusChanged += StatusChanging;
                                d.BeginDownload();
                                sf.Add(d);
                                
                                Console.WriteLine("{0}:Стартовать загрузку {1} \n", connection.HostName,uri);
                                StatusChanged(d);
                            }
                            else
                            {
                                if (cmd.Contains(MessageClientServer.LAST)) //Cигнал на загрузку имеющегося файла
                                {
                                    string[] str = cmd.Split(' ');
                                    string uri = str[1];
                                    for (int i = 0; i < sf.Count; i++)
                                    {
                                        if (sf[i].Url.AbsoluteUri == uri)
                                        {
                                            sf[i].BeginResume();
                                            Console.WriteLine("{0}:Стартовать имеющуюся загрузку {1} \n", connection.HostName, uri);
                                            StatusChanged(sf[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    if (cmd.Contains(MessageClientServer.STOP)) //Стоп загрузки
                                    {
                                        string[] str = cmd.Split(' ');
                                        string uri = str[1];
                                        for (int i = 0; i < sf.Count; i++)
                                        {
                                            if (sf[i].Url.AbsoluteUri == uri)
                                            {
                                                sf[i].Cancel();
                                                Console.WriteLine("{0}:Остановить загрузку {1} \n", connection.HostName, uri);
                                               StatusChanged(sf[i]);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (cmd.Contains(MessageClientServer.PAUSE)) //Пауза загрузки
                                        {
                                            string[] str = cmd.Split(' ');
                                            string uri = str[1];
                                            for (int i = 0; i < sf.Count; i++)
                                            {
                                                if (sf[i].Url.AbsoluteUri == uri)
                                                {
                                                    sf[i].Pause();
                                                    Console.WriteLine("{0}:Приостановить загрузку {1} \n", connection.HostName, uri);
                                                    StatusChanged(sf[i]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (cmd.Contains(MessageClientServer.DELETE)) //Удаление
                                            {
                                                string[] str = cmd.Split(' ');
                                                string uri = str[1];
                                                int[] tmp = new int[sf.Count];
                                                int k = 0;
                                                for (int i = 0; i < tmp.Length; i++)
                                                    tmp[i] = -1;
                                                for (int i = 0; i < sf.Count; i++)
                                                {
                                                    if (sf[i].Url.AbsoluteUri == uri && sf[i].HostName == connection.HostName)
                                                    {
                                                        try
                                                        {
                                                            if (File.Exists(sf[i].DownloadPath))
                                                                File.Delete(sf[i].DownloadPath);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Console.WriteLine("Невозможно удалить файл | " + ex.Message);
                                                        }
                                                        // sf[i].Cancel();
                                                        Console.WriteLine("{0}:Удалить загрузку {1} \n", connection.HostName, uri);
                                                        tmp[k] = i; k++;
                                                    }
                                                }
                                                if (tmp != null)
                                                {
                                                    for (int i = tmp.Length - 1; i >= 0; i--)
                                                    {
                                                        if (tmp[i] != -1)
                                                        {
                                                                sf.RemoveAt(tmp[i]);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                switch (cmd)
                                                {
                                                    //Хост разрывает соединение
                                                    case MessageClientServer.CLOSE_CONNECT:
                                                        {
                                                            Console.WriteLine("Клиент {0} отключился.", connection.HostName);
                                                            flag = false;
                                                            break;
                                                        }

                                                }
                                            }
                                        }                                    
                                    }
                                }
                            }
                        }
                        
                    } while (flag && connection.Socket.Connected);
                }
                catch (SocketException exp)
                {
                    Console.WriteLine(string.Format("SocketException на хосте: {0} - {1}", connection.HostName, exp.Message));
                }
                catch (Exception exp)
                {
                    Console.WriteLine(string.Format("Exception на хосте: {0} - {1}", connection.HostName, exp.Message));
                }
                finally
                {
                    connection.Writer.Close();
                    connection.Reader.Close();
                    connection.Stream.Close();
                    connection.Socket.Close();
                    lock (_connections)
                        _connections.Remove(connection);
                    SaveDownloadList("download.xml");
                }
            }

            //Поиск соединения по id и номеру хоста
            private static ConnectionInfo SearchConnection(string hostName, int id)
            {
                lock (_connections)
                {
                    foreach (ConnectionInfo c in _connections)
                        if (c.id == id && c.HostName == hostName)
                            return c;
                    return null;
                }
            }
            /// <summary>
            /// Отправить сигнал - соединение установлено
            /// </summary>
            /// <param name="con">Соедиение</param>
            public void SignalCool(ConnectionInfo con)
            {
                con.Writer.Write(MessageClientServer.COOL);
                Console.WriteLine("Подтверждение сервера об удачном соединении!!");
            }
            /// <summary>
            /// Отправить сигнал - соединение неустановлено
            /// </summary>
            /// <param name="con">Соедиение</param>
            public void SignalNoCool(ConnectionInfo con)
            {
                con.Writer.Write(MessageClientServer.NOCOOL);
                Console.WriteLine("Подтверждение сервера об неудачном соединении!!");
            }
            /// <summary>
            /// Отправить сигнал - завершена загрузка
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            public static void SignalCompleted(string hostName, int id, string uri, long size)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.COMPLETED + " " + uri+" "+size);
            }

            /// <summary>
            /// Отправить сигнал - загружается загрузка
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            public static void SignalFileDownload(string hostName, int id, string uri, long size)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.DOWNLOADING+" "+uri+" "+size);
            }
            /// <summary>
            /// Отправить сигнал - стоп загрузки
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            public static void SignalStop(string hostName, int id, string uri, long size)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.STOP+" "+uri+" "+size);
            }
            /// <summary>
            /// Отправить сигнал - стоп загрузки
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            /// <param name="uri">URI</param>
            public static void SignalPaused(string hostName, int id, string uri, long size)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.PAUSE+" "+uri+" "+size);
            }
            /// <summary>
            /// Отправить сигнал - невозможно загружить загрузку
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            public static void SignalNOFileDownload(string hostName, int id, string uri)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.NODOWNLOADING + " " + uri);
            }
            /// <summary>
            /// Отправить сигнал - невозможно загружить загрузку
            /// </summary>
            /// <param name="hostName">Имя хоста</param>
            /// <param name="id">ID</param>
            public static void SignalCompleting(string hostName, int id, StateDownload Status, string uri, long size, long downsize,TimeSpan time)
            {
                ConnectionInfo connection = SearchConnection(hostName, id);
                //Не нашли на выход
                if (connection == null)
                    return;
                //Если нашли, отправим соответствующую команду серверу
                connection.Writer.Write(MessageClientServer.INFO + " " + Status + " " + uri + " " + size + " " + downsize + " " + time.ToString());
            }      
            /// <summary>
            /// Команда всем хостам, о том что сервер выключается
            /// </summary>
            public void ExitServerCmd()
            {
                lock (_connections)
                {
                    foreach (ConnectionInfo c in _connections)
                        c.Writer.Write(MessageClientServer.EXIT_SERVER);
                    _connections.Clear();
                    SaveDownloadList("download.xml");
                }
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

                    for (int i = 0; i < sf.Count; i++)
                    {
                        Download current = sf[i];
                        current.ToXml(writer);
                    }

                    writer.WriteEndElement();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при записи списка закачек в файл | " + ex.Message);
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }
           public static void StatusChanged(Download downloader)
            {               
                switch (downloader.Status)
                {
                    case StateDownload.Downloading:
                        SignalFileDownload(downloader.HostName, downloader.id, downloader.Url.AbsoluteUri, downloader.TotalSize);
                        break;
                    case StateDownload.Stopped:
                        SignalStop(downloader.HostName, downloader.id, downloader.Url.AbsoluteUri,downloader.TotalSize);
                        break;
                    case StateDownload.Completed:
                        SignalCompleted(downloader.HostName, downloader.id, downloader.Url.AbsoluteUri, downloader.TotalSize);
                        break;
                    case StateDownload.Paused:
                        SignalPaused(downloader.HostName, downloader.id, downloader.Url.AbsoluteUri, downloader.TotalSize);                        
                        break;
                    case StateDownload.Nofile:
                        SignalNOFileDownload(downloader.HostName, downloader.id, downloader.Url.AbsoluteUri);
                        break;
                }

                if (downloader.Status == StateDownload.Paused)
                {
                    string tmp=
                       String.Format("Отправлено: {0}KB, Общий размер: {1}KB, Время: {2}:{3}:{4}",
                       downloader.DownloadedSize / 1024, downloader.TotalSize / 1024,
                       downloader.TotalUsedTime.Hours, downloader.TotalUsedTime.Minutes,
                       downloader.TotalUsedTime.Seconds);
                    Console.WriteLine(tmp+"\n");
                }
            }
        

         /// <summary>
        /// Загрузить список всех закачек из файла
        /// </summary>
        /// <param name="inputFile"></param>
           public void LoadDownloadList(string inputFile)
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
                       Download d = Download.FromXml(reader);
                       sf.Add(d);
                       reader.Read();
                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine("Ошибка при первоначальной загрузке списка закачек:  " + ex.Message);
               }
               finally
               {
                   if (reader != null)
                   {
                       reader.Close();
                   }
               }
           }
            void DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            if (sf.Count >= 1)
            SignalCompleting(sf[sf.Count - 1].HostName, sf[sf.Count - 1].id, sf[sf.Count-1].Status, sf[sf.Count - 1].Url.AbsoluteUri,sf[sf.Count - 1].TotalSize, e.DownloadedSize,e.TotalTime);
        }

            void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (sf.Count >= 1)
            SignalCompleting(sf[sf.Count - 1].HostName, sf[sf.Count - 1].id, sf[sf.Count - 1].Status, sf[sf.Count - 1].Url.AbsoluteUri, sf[sf.Count - 1].TotalSize, sf[sf.Count - 1].DownloadedSize, sf[sf.Count - 1].TotalUsedTime);
        }

            void StatusChanging(object sender, EventArgs e)
            {
                if (sf.Count >= 1)
                    StatusChanged(sf[sf.Count - 1]);
               
            }
        
            }
}

