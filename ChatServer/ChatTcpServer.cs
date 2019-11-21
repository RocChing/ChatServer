using System;
using System.Collections.Generic;
using System.Text;
using BeetleX;
using BeetleX.EventArgs;
using ChatModel.Input;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ChatRepository;
using ChatModel.Entity;

namespace ChatServer
{
    public class ChatTcpServer : ServerHandlerBase
    {
        private ServerConfig serverConfig;
        private ILogger<ChatTcpServer> logger;
        private IUserRepository userRepository;
        private readonly string currentUserKey = "UserInfoKey";
        public ChatTcpServer(ILogger<ChatTcpServer> log, IOptions<ServerConfig> options, IUserRepository userRepository)
        {
            serverConfig = options.Value;
            logger = log;
            this.userRepository = userRepository;
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            logger.LogInformation($"有客户端连接ID:{e.Session.ID}");
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            ISession session = e.Session;
            string json = e.Stream.ToPipeStream().ReadLine();
            CmdInfo info = null;
            try
            {
                info = JsonSerializer.Deserialize<CmdInfo>(json);

                if (info == null)
                {
                    SendError(session, "参数错误-JSON");
                }

                if (!info.IsValid(serverConfig.ValidString))
                {
                    SendError(session, "参数错误-Token");
                    return;
                }

                switch (info.Type)
                {
                    case CmdType.Login:
                        Login(session, info);
                        break;
                    case CmdType.SendMsg:
                        SendMsg(server, session, info);
                        break;
                    default:
                        SendError(session, "参数错误-CmdType");
                        break;
                }
            }
            catch (Exception ex)
            {
                server.Error(ex, e.Session, ex.Message);
            }
        }

        private void Login(ISession session, CmdInfo info)
        {
            LoginInfo input = info.As<LoginInfo>();
            if (input == null)
            {
                SendError(session, "参数错误-LoginInpu");
                return;
            }
            var user = userRepository.Login(input);
            if (user == null)
            {
                SendError(session, "登录失败-请检查用户名或密码");
                return;
            }
            session[currentUserKey] = user;
            SendInfo(session, info.Clone(user));
        }

        private void SendMsg(IServer server, ISession session, CmdInfo info)
        {
            MsgInfo input = info.As<MsgInfo>();
            if (input != null)
            {
                ISession from = GetSession(server, input.From);
                if (from == null)
                {
                    SendError(session, "您还没有登录系统");
                    return;
                }
                switch (input.ToType)
                {
                    case MsgToType.User:
                        ISession toSession = GetSession(server, input.To);
                        SendInfo(toSession, info, new ReceiveMsgInfo(GetUserInfo(server, input.From), input));
                        break;
                    case MsgToType.Group:
                        break;
                    case MsgToType.System:
                        break;
                }
            }
        }

        private ISession GetSession(IServer server, int userId)
        {
            var sessions = server.GetOnlines();
            foreach (var item in sessions)
            {
                object obj = item[currentUserKey];
                if (obj == null) continue;
                if (obj is User user)
                {
                    if (user.Id == userId)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private User GetUserInfo(IServer server, int userId)
        {
            var sessions = server.GetOnlines();
            foreach (var item in sessions)
            {
                object obj = item[currentUserKey];
                if (obj == null) continue;
                if (obj is User user)
                {
                    if (user.Id == userId)
                    {
                        return user;
                    }
                }
            }
            return null;
        }

        private void SendInfo(ISession session, CmdInfo info, object data)
        {
            if (session == null)
            {
                logger.LogWarning("没有找到session");
                return;
            }
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info.Clone(data)));
            session.Stream.Flush();
        }

        private void SendInfo(ISession session, CmdInfo info)
        {
            if (session == null)
            {
                logger.LogWarning("没有找到session");
                return;
            }
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            session.Stream.Flush();
        }

        private void SendError(ISession session, string msg)
        {
            CmdInfo info = new CmdInfo(serverConfig.ValidString, CmdType.Error, msg);
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info));
            session.Stream.Flush();
        }
    }
}
