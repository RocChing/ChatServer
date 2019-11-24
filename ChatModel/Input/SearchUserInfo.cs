using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public class SearchUserInfo : UserInfo
    {
        public string Avatar { get; set; }

        public int Gender { get; set; }

        public string NickName { get; set; }

        public string Phone { get; set; }
    }
}
