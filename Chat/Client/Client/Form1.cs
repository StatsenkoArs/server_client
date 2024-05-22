using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Client
{

    public partial class Form1 : Form
    {
        static string CODE = "#13";
        static bool connected;
        class myThread
        {
            public Thread thread;
            Label label;
            public myThread(Socket handler, Label output) //Конструктор получает имя функции и номер, до которого ведется счет
            {
                label = output;
                thread = new Thread(this.func);
                //thread.Name = name;
                thread.Start(handler);//передача параметра в поток
            }

            void func(object handler)//Функция потока, передаем параметр всегда object
            {
                if (connected)
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
                        label.Invoke(new Action(() => label.Text += DateTime.Now.ToShortTimeString() + ": " + builder.ToString() + "\n"));
                    }

                    // закрываем сокет
                    ((Socket)handler).Close();
                }
            }
        }
        int port = 8005; // порт сервера
        string address = "127.0.0.1"; // адрес сервера
        Socket socket;
        myThread listener;
        public Form1()
        {
            InitializeComponent();
            this.label1.Text = "Server Answers\n";

            try
            {
                //создаем конечную точку
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                //создаем сокет
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);

                connected = true;

                listener = new myThread(socket, this.label1);

            }
            catch (Exception ex)
            {
                this.label1.Text += ex.Message + "\n";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!connected)
                {
                    //создаем конечную точку
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                    //создаем сокет
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    // подключаемся к удаленному хосту
                    socket.Connect(ipPoint);

                    connected = true;

                    listener = new myThread(socket, this.label1);
                }
                StringBuilder builder = new StringBuilder();

                string message = this.textBox1.Text;
                byte[] data = Encoding.Unicode.GetBytes(message);
                //посылаем сообщение
                socket.Send(data);
                if (message == CODE)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    connected = false;
                }
            }
            catch (Exception ex)
            {
                this.label1.Text += ex.Message + "\n";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (connected)
                {
                    connected = false;

                    StringBuilder builder = new StringBuilder();

                    string message = CODE;
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    //посылаем сообщение
                    socket.Send(data);
                    // закрываем сокет
                    if (message == CODE)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                this.label1.Text += ex.Message + "\n";
            }
        }
    }
}
