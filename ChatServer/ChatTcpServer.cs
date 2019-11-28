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
using ChatModel.Util;
using ChatModel;

namespace ChatServer
{
    public class ChatTcpServer : ServerHandlerBase
    {
        private readonly ServerConfig serverConfig;
        private readonly ILogger<ChatTcpServer> logger;
        private readonly IUserRepository userRepository;
        private readonly string currentUserKey = "UserInfoKey";
        private readonly JsonSerializerOptions jsonOpt;
        private readonly MessageManager msgMgr;

        public ChatTcpServer(ILogger<ChatTcpServer> log, IOptions<ServerConfig> options, MessageManager messageManager, IUserRepository userRepository)
        {
            serverConfig = options.Value;
            logger = log;
            this.userRepository = userRepository;
            msgMgr = messageManager;

            jsonOpt = new JsonSerializerOptions();
            jsonOpt.Converters.Add(new DatetimeJsonConverter());
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            logger.LogInformation($"有客户端连接ID:{e.Session.ID}");
            object obj = e.Session[currentUserKey];
            logger.LogInformation($"the session is null? {obj == null}");
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            ISession session = e.Session;
            string json = e.Stream.ToPipeStream().ReadLine();
            if (json.IsNullOrEmpty())
            {
                int len = (int)e.Stream.Length;
                json = e.Stream.ToPipeStream().ReadString(len);
            }
            Console.WriteLine(json);
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
                    case CmdType.SearchUser:
                        SearchUser(server, session, info);
                        break;
                    case CmdType.AddUser:
                        AddUser(session, info);
                        break;
                    case CmdType.LoginById:
                        LoginById(session, info);
                        break;
                    case CmdType.Check:
                        Check(session, info);
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

        private void Check(ISession session, CmdInfo info)
        {
            SendInfo(session, info.Clone(Constant.Healthy));
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

        private void LoginById(ISession session, CmdInfo info)
        {
            if (info.Data == null) return;
            string str = info.Data.ToString();
            int userId = Convert.ToInt32(str);
            var user = userRepository.GetById(userId);
            if (user == null)
            {
                SendError(session, "登录失败-请检查用户ID");
                return;
            }
            session[currentUserKey] = user;
            var queue = msgMgr.GetQueue(userId);
            if (queue != null && queue.Count > 0)
            {
                while (!queue.IsEmpty)
                {
                    if (queue.TryDequeue(out CmdInfo cloneInfo))
                    {
                        logger.LogInformation("发送离线消息");
                        SendInfo(session, cloneInfo);
                    }
                }
            }
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

        private void SearchUser(IServer server, ISession session, CmdInfo info)
        {
            string key = info.Data.ToString();
            var list = userRepository.GetListByKey(key);
            SendInfo(session, info.Clone(list));
        }

        private void AddUser(ISession session, CmdInfo info)
        {
            UserExtInfo userExt = info.As<UserExtInfo>();
            if (userExt == null)
            {
                SendError(session, "参数错误-UserExtInfo");
            }

            try
            {
                User user = new User(userExt)
                {
                    Password = StringUtil.GetMd5String(User.PASSWORD)
                };
                userRepository.InsertOrUpdate(user);
                SendInfo(session, info.Clone("添加成功"));
            }
            catch (Exception e)
            {
                SendError(session, e.Message);
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

        private void SendInfo(ISession session, CmdInfo info, ReceiveMsgInfo data)
        {
            CmdInfo cloneInfo = info.Clone(data);
            if (session == null)
            {
                logger.LogWarning($"没有找到session--进入待发送队列--to is [{data.To}]");
                msgMgr.Add(data.To, cloneInfo);
                return;
            }
            Console.WriteLine("找到用户");
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(cloneInfo, options: jsonOpt));
            session.Stream.Flush();
        }

        private void SendInfo(ISession session, CmdInfo info)
        {
            if (session == null)
            {
                logger.LogWarning("没有找到session");
                return;
            }
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info, options: jsonOpt));
            session.Stream.Flush();
        }

        private void SendError(ISession session, string msg)
        {
            CmdInfo info = new CmdInfo(serverConfig.ValidString, CmdType.Error, msg);
            session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info, options: jsonOpt));
            session.Stream.Flush();
        }
    }
}
