using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OS_LAB_2_SERVER
{
    class ServerObject
    {
        public List<Record> records = new List<Record>();              // Главный словарь
        static TcpListener tcpListener;                         // Сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>();  // Все подключения

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }

        // Прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 7770);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    clientObject.SetId();
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    Thread.Sleep(20);

                    //BroadcastMessage(JsonConvert.SerializeObject(records), clients[clients.Count].Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // Трансляция сообщения всем подключенным клиентам
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            if (data.Length == 0) { return; }
            for (int i = 0; i < clients.Count; i++)
            {
                //if (clients[i].Id != id) // если id клиента не равно id отправляющего
                //{
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                //}
            }
        }

        // Отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}
