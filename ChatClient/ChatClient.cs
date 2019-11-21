using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using BeetleX.Clients;
using BeetleX;
using ChatModel.Input;
using ChatModel.Entity;

namespace ChatClient
{
    public class ChatClient
    {
        private AsyncTcpClient client;
        private User user;
        private readonly string host = "localhost";
        public ChatClient()
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

                CmdInfo info = JsonSerializer.Deserialize<CmdInfo>(line);
                switch (info.Type)
                {
                    case CmdType.Error:
                        Console.WriteLine(info.GetDataRowText());
                        break;
                    case CmdType.Login:
                        user = info.As<User>();
                        Console.WriteLine($"欢迎[{user.Name}]登录系统");
                        break;
                    case CmdType.SendMsg:
                        ReceiveMsgInfo receiveMsgInfo = info.As<ReceiveMsgInfo>();
                        if (receiveMsgInfo != null)
                        {
                            Console.WriteLine($"[{receiveMsgInfo.From.Name}]:[{receiveMsgInfo.Msg}]");
                        }
                        break;
                }
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

        public int GetUserId()
        {
            if (user != null) return user.Id;
            return 0;
        }

        public void Close()
        {
            client.DisConnect();
        }
    }
}
