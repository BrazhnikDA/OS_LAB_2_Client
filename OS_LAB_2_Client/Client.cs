using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OS_LAB_2_Client
{ 
    class Client
    {
        private const string host = "127.0.0.1";    // IP
        private const int port = 7770;              // Порт
        TcpClient client;                           // Клиент
        NetworkStream stream;                       // Поток от клиента до сервера
        
        public bool IsConnected { get; set; }

        public Client()
        {

        }

        public void serverConnect()
        {
            // Создаём клиент
            client = new TcpClient();
            try
            {
                client.Connect(host, port);     // Подключение клиента
                stream = client.GetStream();    // Получаем поток
                IsConnected = true;
                // Запускаем новый поток для получения данных от сервера
                Thread receiveThread = new Thread(new ThreadStart(receiveMessage));
                receiveThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                // Вывод ошибки в консоль, скорее всего сервер не запущен, или случилась хрень
                Console.WriteLine(ex.Message);
                //MessageBox.Show("Сервер не отвечает", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool checkConnection()
        {
            if (client == null) return false;

            if (!client.Connected)
            {
                return false;
            }

            return true;
        }

        private void receiveMessage()
        {
            // Бесконечно слушаем
            while (true)
            {
                try
                {
                    byte[] data = new byte[128]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable); 
                    Console.WriteLine("Error: " + builder.ToString());      // Вывод полученного сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); // Соединение было прервано
                    //MessageBox.Show("Подключение разрвано, приложение закроется через 5 секунд", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //Thread.Sleep(5000);
                    disconnect();
                }
            }
        }

        private void disconnect()
        {
            if (stream != null)
                stream.Close();     // Отключение потока
            if (client != null)
                client.Close();     // Отключение клиента
            Environment.Exit(0);    // Закртиые приложения
        }

        public void sendMessage(string message)
        {
            if ((message != ""))
            {
                byte[] buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes(message);   // Делаем байт код в формате UTF-8(Это важно) и отправляем его на сервер
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
