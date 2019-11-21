using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public class MsgInfo
    {
        /// <summary>
        /// 信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 信息类型
        /// </summary>
        public MsgType Type { get; set; }

        /// <summary>
        /// 谁发的
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// 发给谁的 User = UserId , Group=GroupId
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// 发给谁的类型
        /// </summary>
        public MsgToType ToType { get; set; }
    }
}
