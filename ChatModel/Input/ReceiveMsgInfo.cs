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

        /// <summary>
        /// 发给谁的 User = UserId , Group=GroupId
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// 发给谁的类型
        /// </summary>
        public MsgToType ToType { get; set; }

        public string ReceiveTime { get; set; }

        public byte[] MsgOfBytes { get; set; }

        public ReceiveMsgInfo() { }

        public ReceiveMsgInfo(User from, MsgInfo info)
        {
            From = new UserInfo(from);
            Msg = info.Msg;
            Type = info.Type;
            To = info.To;
            ToType = info.ToType;
            ReceiveTime = DateTime.Now.ToString(Constant.DateTimeFormat);
            MsgOfBytes = info.MsgOfBytes;
        }
    }
}
