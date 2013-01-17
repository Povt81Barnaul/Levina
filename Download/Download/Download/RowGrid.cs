using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Download
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
        public class RowGrid
        {
            /// <summary>
            /// Идентификатор ресурса
            /// </summary>
            public Uri Uri { get; private set; }
            /// <summary>
            /// Имя скачиваемого файла
            /// </summary>
            public string FileName { get; set; }
            /// <summary>
            /// Состояние загрузки
            /// </summary>
            public StateDownload State;
            /// <summary>
            /// Размер скачиваемого файла
            /// </summary>
            public long Size { get; set; }
            /// <summary>
            /// Раcположение(на какой сервер идет закачка)
            /// </summary>
            public string Location = Properties.Settings.Default.DownloadServer;
            /// <summary>
            /// Скачано байт
            /// </summary>
            public long BytesDownload { get; set; }
            /// <summary>
            /// Флаг обновления
            /// </summary>
            public bool flagRefresh { get; set; }
            /// <summary>
            /// Время затрачиваемое на загрузку
            /// </summary>
            public TimeSpan time { get; set; }
            private RowGrid() { flagRefresh=false; }
            public RowGrid(Uri _uri, string _filename)
            {
                Uri = _uri;
                FileName = _filename;
                flagRefresh=false;
            }
            public void ToXml(XmlWriter writer)
            {
                writer.WriteStartElement("download");

                writer.WriteElementString("uri", Uri.ToString());
                writer.WriteElementString("namefile", FileName);
                writer.WriteElementString("filePath", Location);
                writer.WriteElementString("size", Size.ToString());
                writer.WriteElementString("bytesDownload", BytesDownload.ToString());
                writer.WriteElementString("downloadState", ((int)State).ToString());
 
                writer.WriteEndElement();
            }
            public static RowGrid FromXml(XmlReader reader)
            {
                RowGrid result = new RowGrid();

                reader.ReadStartElement("download");

                if (reader.Name != "uri") throw new FormatException();
                result.Uri = new Uri(reader.ReadString());
                reader.Read();
                if (reader.Name != "namefile") throw new FormatException();
                result.FileName = reader.ReadString();
                reader.Read();
                if (reader.Name != "filePath") throw new FormatException();
                result.Location = reader.ReadString();
                reader.Read();
                if (reader.Name != "size") throw new FormatException();
                result.Size = long.Parse(reader.ReadString());
                reader.Read();
                if (reader.Name != "bytesDownload") throw new FormatException();
                result.BytesDownload = Int64.Parse(reader.ReadString());
                reader.Read();
                if (reader.Name != "downloadState") throw new FormatException();
                result.State = (StateDownload)(Convert.ToInt32(reader.ReadString()));
                
                reader.ReadEndElement();
                return result;
            }
            public string NameState(StateDownload stateDownload)
            {
                if (stateDownload == StateDownload.Completed)
                    return "Завершена";
                else
                {
                    if (stateDownload == StateDownload.Downloading)
                        return "Загружается";
                    else
                    {
                        if (stateDownload == StateDownload.Created)
                            return "Создана";
                        else
                        {
                            if (stateDownload == StateDownload.Stopped)
                                return "Остановлена";
                            else
                            {
                                if (stateDownload == StateDownload.Paused)
                                    return "Пауза";
                                else
                                    return "Не известно";
                            }
                        }
                    }
                }
            }
           
        }
}
