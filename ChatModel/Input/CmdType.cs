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
        AddUser = 4,
        LoginById = 5,
        Check = 6,
        Error = 0,
        Info = 100
    }
}
