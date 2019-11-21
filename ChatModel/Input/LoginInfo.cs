using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public class LoginInfo
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public bool IsValid()
        {
            if (Name.IsNullOrEmpty())
            {
                return false;
            }

            if (Password.IsNullOrEmpty())
            {
                return false;
            }
            return true;
        }
    }
}
