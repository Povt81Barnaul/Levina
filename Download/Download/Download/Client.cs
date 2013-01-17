using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using DownloadLibrary;
using System.Windows.Forms;

namespace Download
{
    /// <summary>
    /// Клиент
    /// </summary>
    public class Client : IDisposable
    {
        private const int INTERVAL_CONNECT = 5000;

        private TcpClient _client;
        private Thread _clientThreaad;
        private IPEndPoint _serverAddress;
        private String _hostName;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        private System.Timers.Timer _timerConnect;

        /// <summary>
        /// Клиент
        /// </summary>
        /// <param name="serverAddress">Адрес сервера</param>
        public Client(IPEndPoint serverAddress)
        {
            _serverAddress = serverAddress;
            _hostName = Dns.GetHostName();
            _timerConnect = new System.Timers.Timer(Convert.ToDouble(INTERVAL_CONNECT));
            _timerConnect.Elapsed += _timerConnect_Elapsed;

            _clientThreaad = new Thread(RunClient) { IsBackground = true };
            _clientThreaad.Start();
        }

        //Повтор соединения по таймеру
        void _timerConnect_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_client.Connected)
                return;

            try
            {
                _client.Connect(_serverAddress);
            }
            catch (SocketException)
            {

            }
        }

        //Запуск работы клиента (в отдельном потоке принемаем данные)
        private void RunClient(object obj)
        {
        start:
            try
            {
                _client = new TcpClient();
                _timerConnect.Start();

                while (true)
                    if (_client.Connected)
                    {
                        _timerConnect.Stop();
                        break;
                    }

                _stream = _client.GetStream();
                _writer = new BinaryWriter(_stream);
                _reader = new BinaryReader(_stream);

                string helloStr = string.Format("{0} {1}", MessageClientServer.CONNECT, _hostName);
                _writer.Write(helloStr);

                try
                {
                    bool flag = true;
                    do
                    {
                        //Считываем команды из буфера
                        string cmd = _reader.ReadString();
                        if (cmd.Contains(MessageClientServer.NODOWNLOADING)) //Cигнал-невозможно загрузить 
                        {
                            string[] str = cmd.Split(' ');
                            string uri = str[1];
                            MainForm.NODOWN(uri);
                        }
                        else{
                            if (cmd.Contains(MessageClientServer.DOWNLOADING)) //Cигнал-загружается
                            {
                                string[] str = cmd.Split(' ');
                                string uri = str[1];
                                long size = long.Parse(str[2]);
                                MainForm.DOWN(uri, size);
                            }
                            else
                            {
                                if (cmd.Contains(MessageClientServer.COMPLETED)) //Cигнал-загрузка завершена
                            {
                                string[] str = cmd.Split(' ');
                                string uri = str[1];
                                long size = long.Parse(str[2]);
                                MainForm.COMPLETED(uri,size);
                            }
                            else
                            {
                                if (cmd.Contains(MessageClientServer.PAUSE)) //Cигнал-загрузка на паузе
                                {
                                    string[] str = cmd.Split(' ');
                                    string uri = str[1];
                                    long size = long.Parse(str[2]);
                                    MainForm.Pause(uri,size);
                                }
                                else
                                {
                                    if (cmd.Contains(MessageClientServer.STOP)) //Cигнал-загрузка на стоп
                                    {
                                        string[] str = cmd.Split(' ');
                                        string uri = str[1];
                                        long size = long.Parse(str[2]);
                                        MainForm.Stop(uri, size);
                                    }
                                    else
                                    {
                                        if (cmd.Contains(MessageClientServer.INFO)) //инфрмационные сигнал
                                        {
                                            string[] str = cmd.Split(' ');
                                            string tmp = str[1];
                                            StateDownload st=StateDownload.Nofile;
                                            if (tmp == "Downloading")
                                                st = StateDownload.Downloading;
                                            else
                                            {
                                                if (tmp == "Completed")
                                                    st = StateDownload.Completed;
                                                else
                                                    if (tmp == "Paused")
                                                        st = StateDownload.Paused;
                                                    else
                                                        if (tmp == "Stopped")
                                                            st = StateDownload.Stopped;
                                                        else
                                                            if (tmp == "Created")
                                                                st = StateDownload.Created;
                                            }
                                            string uri = str[2];
                                            long size = long.Parse(str[3]);
                                            long downsize = long.Parse(str[4]);
                                            TimeSpan time = TimeSpan.Parse(str[5]);
                                            MainForm.Information(st,uri, size,downsize,time);
                                        }
                                        else
                                        {
                                            switch (cmd)
                                            {
                                                //соединение не установлено
                                                case MessageClientServer.NOCOOL:
                                                    break;
                                                //соединение установлено
                                                case MessageClientServer.COOL:
                                                    MainForm.flagConnect = false;
                                                    break;

                                                //Выключение сервера
                                                case MessageClientServer.EXIT_SERVER:
                                                    {
                                                        MessageBox.Show("Сервер отключился.");
                                                        CloseResourse();
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
                    }while (flag);
                    
                }
                catch (SocketException)
                {
                    MessageBox.Show("Соединение было не ожиданно разорванно!");
                    CloseResourse();
                    goto start;
                }
            }
            catch (Exception exp)
            {
                CloseResourse();
                MessageBox.Show(exp.Message);
            }
            finally
            {
                CloseResourse();
            }
        }

        /// <summary>
        /// Отправка на сервер сигнала на загрузку файла
        /// </summary>
        public void SignalServerDownload(string strUri)
        {
            _writer.Write(MessageClientServer.START+" "+strUri+" "+Properties.Settings.Default.numTread.ToString());
        }
        /// <summary>
        /// Отправка на сервер сигнала на загрузку имеющегося файла
        /// </summary>
        public void SignalServerDownloadLast(string strUri)
        {
            _writer.Write(MessageClientServer.LAST + " " + strUri);
        }
        /// <summary>
        /// Отправка на сервер сигнала на установку на паузу загрузку файла
        /// </summary>
        public void SignalServerPause(string strUri)
        {
            _writer.Write(MessageClientServer.PAUSE + " " + strUri);
        }
        /// <summary>
        /// Отправка на сервер сигнала остановки загрузки файла
        /// </summary>
        public void SignalServerStop(string strUri)
        {
            _writer.Write(MessageClientServer.STOP + " " + strUri);
        }
        
        /// <summary>
        /// Отправка на сервер сигнала удаление загрузки файла
        /// </summary>
        public void SignalServerDelete(string strUri)
        {
            _writer.Write(MessageClientServer.DELETE + " " + strUri);
        }

        /// <summary>
        /// Отправка на сервер команды об отключении клиента
        /// </summary>
        public void CloseClientCmd()
        {
            if (_client != null && _client.Connected)
                _writer.Write(MessageClientServer.CLOSE_CONNECT);             
        }

        //Закрытие ресурсов
        private void CloseResourse()
        {
            //Закрытие соединения
            _writer.Close();
            _reader.Close();
            _stream.Close();
            _client.Close();            
        }

        //Уничтожение объекта
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //Освобождение памяти
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                if (_writer != null)
                {
                    _writer.Dispose();
                    _writer = null;
                }
                if (_timerConnect != null)
                {
                    _timerConnect.Dispose();
                    _timerConnect = null;
                }
            }
        }

        //Деструтор
        ~Client()
        {
            Dispose(false);
        }
    }
}
 

