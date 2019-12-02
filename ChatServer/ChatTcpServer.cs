using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using BeetleX;
using BeetleX.EventArgs;
using ChatModel.Input;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ChatRepository;
using ChatModel.Entity;
using ChatModel.Util;
using ChatModel;
using Microsoft.Extensions.Caching.Memory;

namespace ChatServer
{
    public class ChatTcpServer : ServerHandlerBase
    {
        private readonly ServerConfig serverConfig;
        private readonly ILogger<ChatTcpServer> logger;
        private readonly IUserRepository userRepository;
        private readonly string currentUserKey = "UserInfoKey";
        private readonly string cacheUserListKey = "UserListKey";
        private readonly JsonSerializerOptions jsonOpt;
        private readonly MessageManager msgMgr;
        private readonly MemoryCache cache;

        private readonly int minMsgLength = 5;
        private readonly string beginFlag = "N";

        private long msgAllLength = 0;
        private bool newMsg = false;

        private object lockObj = new object();

        private readonly ConcurrentDictionary<long, MsgFullInfo> msgFullInfos;

        public ChatTcpServer(ILogger<ChatTcpServer> log, IOptions<ServerConfig> options, MessageManager messageManager, IUserRepository userRepository)
        {
            serverConfig = options.Value;
            logger = log;
            this.userRepository = userRepository;
            msgMgr = messageManager;

            jsonOpt = new JsonSerializerOptions();
            jsonOpt.Converters.Add(new DatetimeJsonConverter());
            cache = new MemoryCache(new MemoryCacheOptions());

            msgFullInfos = new ConcurrentDictionary<long, MsgFullInfo>();
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            logger.LogInformation($"有客户端连接ID:{e.Session.ID}");
            object obj = e.Session[currentUserKey];
            logger.LogInformation($"the session is null? {obj == null}");
        }
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            if (e.Stream.Length < minMsgLength) return;
            MsgFullInfo msgFullInfo = msgFullInfos.GetOrAdd(e.Session.ID, key =>
             {
                 return new MsgFullInfo(key);
             });

            string json = string.Empty;
            lock (lockObj)
            {
                if (!msgFullInfo.NewMsg)
                {
                    byte firstChar = (byte)e.Stream.ToPipeStream().ReadByte();
                    string beginMsg = Encoding.UTF8.GetString(new byte[] { firstChar });
                    if (beginFlag.Equals(beginMsg))
                    {
                        msgFullInfo.MsgAllLength = e.Stream.ToPipeStream().ReadInt32();
                        msgFullInfo.NewMsg = true;
                    }
                }

                Console.WriteLine($"the msg currentMsgLength is: {e.Stream.Length}");
                Console.WriteLine($"the msgAllLength value is: {msgAllLength}");

                if (msgFullInfo.MsgAllLength > e.Stream.Length)
                {
                    return;
                }

                msgFullInfo.Reset();
                json = e.Stream.ToPipeStream().ReadToEnd();
            }

            ISession session = e.Session;

            Console.WriteLine($"the json length is:{json.Length}, value is: {json}");
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
                        Check(server, session, info);
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

        private void Check(IServer server, ISession session, CmdInfo info)
        {
            object obj = info.Data;
            int userId = 0;
            if (obj != null)
            {
                int.TryParse(obj.ToString(), out userId);
            }

            if (userId > 0)
            {
                logger.LogInformation($"the session name is: {session.Name}");
                User user = GetUserInfo(server, userId);
                if (user == null)
                {
                    logger.LogInformation("没有找到session");
                    user = GetUserInfo(userId);
                    session[currentUserKey] = user;
                }
                else
                {
                    logger.LogInformation("找到session");
                }
                SendOffLineMsg(session, userId);
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
            user.Password = string.Empty;
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
            user.Password = string.Empty;
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

        private void SendOffLineMsg(ISession session, int userId)
        {
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
                User fromUser = GetUserInfo(server, input.From);
                if (fromUser == null)
                {
                    logger.LogInformation($"您还没有登录系统 the session name is: {session.Name}");
                    SendError(session, "您还没有登录系统");
                    return;
                }
                if (input.MsgOfBytes != null)
                {
                    logger.LogInformation($"the first byte is: {input.MsgOfBytes[0]}");
                    logger.LogInformation($"the last byte is: {input.MsgOfBytes[input.MsgOfBytes.Length - 1]}");
                }
                switch (input.ToType)
                {
                    case MsgToType.User:
                        ISession toSession = GetSession(server, input.To);
                        SendInfo(toSession, info, new ReceiveMsgInfo(fromUser, input));
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

        private User GetUserInfo(int userId)
        {

            var list = cache.GetOrCreate(cacheUserListKey, ce =>
             {
                 logger.LogInformation("从数据库读取。。。");
                 return userRepository.GetList(m => m.Enabled == 1);
             });
            User user = null;
            if (list != null && list.Count() > 0)
            {
                user = list.FirstOrDefault(m => m.Id == userId);
            }
            if (user == null)
            {
                logger.LogError($"没有找到对应的用户--[{userId}]");
            }
            return user;
        }

        private void SendInfo(ISession session, CmdInfo info, ReceiveMsgInfo data)
        {
            CmdInfo cloneInfo = info.Clone(data);
            if (session == null)
            {
                int to = data.To;
                if (to < 1)
                {
                    logger.LogWarning($"参数错误--To:[{to}]");
                    return;
                }
                logger.LogWarning($"没有找到session--进入待发送队列--to is [{to}]");
                msgMgr.Add(to, cloneInfo);
                return;
            }
            Console.WriteLine("找到用户");
            //session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(cloneInfo, options: jsonOpt));
            //session.Stream.Flush();
            SendByteMsg(session, JsonSerializer.Serialize(cloneInfo, jsonOpt));
        }

        private void SendInfo(ISession session, CmdInfo info)
        {
            if (session == null)
            {
                logger.LogWarning("没有找到session");
                return;
            }
            SendByteMsg(session, JsonSerializer.Serialize(info, jsonOpt));
            //session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info, options: jsonOpt));
            //session.Stream.Flush();
        }

        private void SendError(ISession session, string msg)
        {
            CmdInfo info = new CmdInfo(serverConfig.ValidString, CmdType.Error, msg);
            //session.Stream.ToPipeStream().WriteLine(JsonSerializer.Serialize(info, options: jsonOpt));
            //session.Stream.Flush();
            SendByteMsg(session, JsonSerializer.Serialize(info, options: jsonOpt));
        }

        private void SendByteMsg(ISession session, string msg)
        {
            byte[] bytes1 = Encoding.UTF8.GetBytes(beginFlag);
            byte[] bytes2 = Encoding.UTF8.GetBytes(msg);
            byte[] bytes3 = BitConverter.GetBytes(bytes2.Length);

            Console.WriteLine($"the [msg] length is:{bytes2.Length}");
            foreach (var item in bytes3)
            {
                Console.WriteLine($"the byte value is:{item}");
            }
            session.Stream.ToPipeStream().Write(bytes1, 0, bytes1.Length);
            session.Stream.ToPipeStream().Write(bytes3, 0, bytes3.Length);
            session.Stream.ToPipeStream().Write(bytes2, 0, bytes2.Length);
            session.Stream.Flush();
            //session.Stream.ToPipeStream().Write(msg);
            //session.Stream.Flush();
        }
    }
}
