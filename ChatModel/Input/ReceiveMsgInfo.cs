using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;

namespace ChatModel.Input
{
    public class ReceiveMsgInfo
    {
        /// <summary>
        /// 发送者信息
        /// </summary>
         public UserInfo From { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 信息类型
        /// </summary>
        public MsgType Type { get; set; }

        public ReceiveMsgInfo() { }

        public ReceiveMsgInfo(User from, MsgInfo info)
        {
            From = new UserInfo(from);
            Msg = info.Msg;
            Type = info.Type;
        }
    }
}
