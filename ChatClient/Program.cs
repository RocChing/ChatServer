using System;
using ChatModel.Input;

namespace ChatClient
{
    class Program
    {
        static WebSocketClient client = new WebSocketClient();
        static void Main(string[] args)
        {
            Console.WriteLine("------------------------------Web Socket Client-------------------------------");
            Console.WriteLine("命令如下");
            Console.WriteLine("1-Login-登录");
            Console.WriteLine("2-SendMsg-发送消息");
            Console.WriteLine("E-退出客户端");
            bool exit = false;
            while (true)
            {
                Console.Write("请输入命令:");
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        SendMsg();
                        break;
                    case "c":
                    case "C":
                        Console.Clear();
                        break;
                    case "e":
                    case "E":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("命令有误");
                        break;
                }
                if (exit)
                {
                    break;
                }
            }
        }

        static void Login()
        {
            Console.Write("请输入账号:");
            string name = Console.ReadLine();
            Console.Write("请输入密码:");
            string password = Console.ReadLine();
            client.Send(new MsgInfo(CmdType.Login, new LoginInput()
            {
                Name = name,
                Password = password
            }));
        }

        static void SendMsg()
        {
            Console.Write("请输入对方ID:");
            string to = Console.ReadLine();
            Console.Write("请输入消息:");
            string msg = Console.ReadLine();
            client.Send(new MsgInfo(CmdType.SendMsg, new MsgInput()
            {
                Msg = msg,
                To = to,
                Type = MsgType.Text,
                ToType = MsgToType.User
            }));
        }
    }
}
