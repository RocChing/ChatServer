using System;
using System.Collections.Generic;
using System.Text;
using BeetleX;
using BeetleX.EventArgs;
using ChatModel.Input;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ChatServer
{
    public class ChatTcpServer : ServerHandlerBase
    {
        private ServerConfig serverConfig;
        public ChatTcpServer(IOptions<ServerConfig> options)
        {
            serverConfig = options.Value;
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            Console.WriteLine($"有客户端连接ID:{e.Session.ID}");
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            string json = e.Stream.ToPipeStream().ReadLine();
            Console.WriteLine(json);
            MsgInfo info = null;
            try
            {
                info = JsonSerializer.Deserialize<MsgInfo>(json);
                if (!info.IsValid(serverConfig.ValidString))
                {
                    SendError(e, "参数错误-Token");
                    return;
                }
                switch (info.Type)
                {
                    case CmdType.Login:
                        Login(e, info);
                        break;
                    case CmdType.SendMsg:
                        SendMsg(server, e, info);
                        break;
                    default:
                        SendError(e, "参数错误-CmdType");
                        break;
                }
                return;
            }
            catch (Exception ex)
            {
                server.Error(ex, e.Session, ex.Message);
            }
            if (info == null)
            {
                SendError(e, "参数错误-JSON");
            }
        }

        private void Login(SessionReceiveEventArgs e, MsgInfo info)
        {
            LoginInput input = info.As<LoginInput>();
            if (input != null)
            {
                if ("admin".Equals(input.Name, StringComparison.OrdinalIgnoreCase) && "1".Equals(input.Password))
                {
                    Send(e, new MsgInfo(serverConfig.ValidString, CmdType.Login, "登录成功"));
                }
            }
        }

        private void SendMsg(IServer server, SessionReceiveEventArgs e, MsgInfo info)
        {
            MsgInput input = info.As<MsgInput>();
            if (input != null)
            {
                input.From = e.Session.ID;
                switch (input.ToType)
                {
                    case MsgToType.User:
                        int sessionId = Convert.ToInt32(input.To);
                        ISession session = server.GetSession(sessionId);
                        Send(session, info);
                        break;
                    case MsgToType.Group:
                        break;
                    case MsgToType.System:
                        break;
                }
            }
        }

        private void Send(SessionReceiveEventArgs e, MsgInfo info)
        {
            e.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            e.Stream.Flush();
        }

        private void SendError(SessionReceiveEventArgs e, string msg)
        {
            MsgInfo info = new MsgInfo(serverConfig.ValidString, CmdType.Error, msg);
            e.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            e.Stream.Flush();
        }

        private void Send(ISession session, MsgInfo info)
        {
            if (session == null)
            {
                Console.WriteLine("没有找到session");
                return;
            }
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            session.Stream.Flush();
        }

        private void SendError(ISession session, string msg)
        {
            MsgInfo info = new MsgInfo(serverConfig.ValidString, CmdType.Error, msg);
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            session.Stream.Flush();
        }
    }
}
