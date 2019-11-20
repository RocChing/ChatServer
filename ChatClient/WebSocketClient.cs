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
                line = ChatModel.Util.StringUtil.GetGBString(line);
                Console.WriteLine(line);
            };
            client.Connected = c =>
            {
                Console.WriteLine("连接到服务器: " + c.IsConnected);
            };
        }

        public void Send(string msg)
        {
            client.Connect();
            client.Stream.ToPipeStream().WriteLine(msg);
            client.Stream.Flush();
        }

        public void Send<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);
            Send(json);
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
