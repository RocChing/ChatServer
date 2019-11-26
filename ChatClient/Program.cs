using System;
using ChatModel.Input;

namespace ChatClient
{
    class Program
    {
        static ChatClient client = new ChatClient();
        static string validString = "ef0a6a74fc2343d18edf56cf4eb07211";
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
            client.Send(new CmdInfo(validString, CmdType.Login, new LoginInfo()
            {
                Name = name,
                Password = password
            }));
        }

        static void SendMsg()
        {
            //Console.WriteLine("请输入From ID:");
            //string from = Console.ReadLine();
            Console.Write("请输入To ID:");
            string to = Console.ReadLine();
            Console.Write("请输入消息:");
            string msg = Console.ReadLine();
            client.Send(new CmdInfo(validString, CmdType.SendMsg, new MsgInfo()
            {
                From = client.GetUserId(),
                Msg = msg,
                To = Convert.ToInt32(to),
                Type = MsgType.Text,
                ToType = MsgToType.User
            }));
        }
    }
}
