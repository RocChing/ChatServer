using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public enum CmdType
    {
        Login = 1,
        SendMsg = 2,
        SearchUser = 3,
        Error = 0,
        Info = 100
    }
}
