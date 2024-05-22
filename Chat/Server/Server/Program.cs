using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketTcpServer
{
    class Program
    {
        static List<myThread> threads = new List<myThread>();
        class myThread
        {
            const string CODE = "#13";
            Thread thread;
            Socket handler;
            public myThread(Socket handler) //Конструктор получает имя функции и номер, до которого ведется счет
            {
                thread = new Thread(this.func);
                //thread.Name = name;
                this.handler = handler;
                thread.Start(handler);//передача параметра в поток
            }

            void func(object handler)//Функция потока, передаем параметр всегда object
            {
                StringBuilder builder = new StringBuilder(); ;
                while (builder.ToString() != CODE)
                {
                    builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов за 1 раз
                    int kol_bytes = 0;//количество полученных байтов
                    byte[] data = new byte[255]; // буфер для получаемых данных
                    do
                    {
                        bytes = ((Socket)handler).Receive(data);  // получаем сообщение
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        kol_bytes += bytes;
                    }
                    while (((Socket)handler).Available > 0);

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    Console.WriteLine(kol_bytes + "bytes\n");
                    // отправляем ответ клиентам, то, что получили от него
                    if (builder.ToString() != CODE)
                        foreach (var thread in threads)
                        {
                            thread.send(builder.ToString());
                        }
                    else
                        this.send(builder.ToString());
                }

                // закрываем сокет
                ((Socket)handler).Shutdown(SocketShutdown.Both);
                ((Socket)handler).Close();
                threads.Remove(this);
                Console.WriteLine("Disconnected");
            }

            void send(string message)
            {
                ((Socket)handler).Send(Encoding.Unicode.GetBytes(message));
            }
        }
        static int port = 8005; // порт для приема входящих запросов
        static void Main(string[] args)
        {
            String Host = Dns.GetHostName();
            Console.WriteLine("Comp name = " + Host);
            IPAddress[] IPs;
            IPs = Dns.GetHostAddresses(Host);
            foreach (IPAddress ip1 in IPs)
                Console.WriteLine(ip1);


            //получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // создаем сокет сервера
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Server launched. Waiting connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();  // сокет для связи с     клиентом
                    threads.Add(new myThread(handler));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
