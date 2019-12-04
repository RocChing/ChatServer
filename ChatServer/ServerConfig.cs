using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    public  class ServerConfig
    {
        /// <summary>
        /// 端口号 默认8800
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 验证字符串 服务端和客户端必须设置为相同
        /// </summary>
        public string ValidString { get; set; }
    }
}
