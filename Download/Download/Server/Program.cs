using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace Server
{
    class Program
    {

        //Сервер
      public static Server server;        

        static void StartServer()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Download"))
            {
                Directory.CreateDirectory(@"Download");
            }
            //Адресс сервера
            IPEndPoint serverAddress;
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ip = ipHostInfo.AddressList[0];

            serverAddress = new IPEndPoint(ip, 11000);
            if (serverAddress == null)
                Console.WriteLine("Не правильный формат ip адреса или порта");
            server = new Server(serverAddress);
            server.LoadDownloadList("download.xml");
        }

       static void Main(string[] args)
        {
            StartServer();            
           
            Console.WriteLine("\nСервер ожидает...");
            Console.Read();
        }

}
} 

    