using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OS_LAB_2_SERVER
{
    class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        TcpClient client;
        ServerObject server;    // Объект сервера

        int currentID = 0;
        //Dictionary<string, Record> records = new Dictionary<string, Record>();

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }
        
        public void SetId()
        {
            try
            {
                Stream = client.GetStream();
                string message = "Новое подключение: " + Id;
                Console.WriteLine(message);
                for (int i = 0; i < server.records.Count; i++)
                {
                    server.BroadcastMessage(JsonConvert.SerializeObject(server.records[i]), Id);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public void Process()
        {
            try
            {
                string message;
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        Record tmp = new Record();
                        message = GetMessage();

                        if (message != "")
                        {
                            if (message == "MERGE")
                            {
                                if (server.records.Count > 0)
                                {
                                    for (int i = 0; i < server.records.Count; i++)
                                    {
                                        server.BroadcastMessage(JsonConvert.SerializeObject(server.records[i]), Id);
                                    }
                                }
                                else
                                {
                                    server.BroadcastMessage("Empty", Id);
                                }
                            }
                            else
                            {
                                if (message == "UPDATE")
                                {
                                    // function update....
                                }
                                else
                                {
                                    if (message == "DELETE")
                                    {
                                        // function delete
                                    }
                                    else
                                    {
                                        var deserMessage = JsonConvert.DeserializeObject<Record>(message);

                                        if (deserMessage == null)
                                        {
                                            server.RemoveConnection(this.Id);
                                            Close();
                                            return;
                                        }
                                        int indexRow = Convert.ToInt32(deserMessage.id) - 1;

                                        tmp.name = deserMessage.name;
                                        tmp.marka = deserMessage.marka;
                                        tmp.serNum = deserMessage.serNum;
                                        tmp.tech = deserMessage.tech;
                                        tmp.date = deserMessage.date;
                                        tmp.faultClient = deserMessage.faultClient;
                                        tmp.id = deserMessage.id;

                                        try
                                        {
                                            if (indexRow < server.records.Count)
                                            {
                                                server.records[indexRow] = tmp;
                                            }
                                            else
                                            {
                                                server.records.Add(tmp);
                                                currentID++;
                                            }

                                            //Console.WriteLine("JSON: " + tmp.name + " " + tmp.marka + " " + tmp.serNum + " " +
                                            //    tmp.tech + " " + tmp.date + " " + tmp.faultClient + " " + tmp.id + " ");
                                        }
                                        catch { }

                                        for (int i = 0; i < server.records.Count; i++)
                                        {
                                            server.BroadcastMessage(JsonConvert.SerializeObject(server.records[i]), Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        message = String.Format("Покинул чат");
                        Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        public string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
