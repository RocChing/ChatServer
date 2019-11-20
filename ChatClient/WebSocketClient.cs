using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BeetleX.Clients;
using BeetleX;
using ChatModel;

namespace ChatClient
{
    public class WebSocketClient
    {
        private AsyncTcpClient client;

        private readonly string url = "ws://localhost:8800/";
        private readonly string host = "localhost";
        public WebSocketClient()
        {
            Init();
        }

        private void Init()
        {
            client = SocketFactory.CreateClient<AsyncTcpClient>(host, 8800);
            client.DataReceive = (o, args) =>
            {
                string line = args.Stream.ToPipeStream().ReadLine();
                Console.WriteLine(line);
            };
            client.Connected = c =>
            {
                Console.WriteLine("连接到服务器: " + c.IsConnected);
            };
            client.PacketReceive = (c, msg) =>
            {
                Console.WriteLine("PacketReceive");
                Console.WriteLine(msg.GetType());
            };
        }

        public void Send(string msg)
        {
            client.Connect();
            client.Stream.ToPipeStream().WriteLine(msg);
            client.Stream.Flush();
            //client.Send();
        }

        public void Send<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);
            Send(json);
            //client.Connect();
            //client.Send(data);
        }

        public void Send(object data)
        {
            string json = JsonSerializer.Serialize(data);
            Send(json);
        }

        public void Close()
        {
            client.DisConnect();
        }
    }
}
