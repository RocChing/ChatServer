﻿using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;

namespace ChatModel.Input
{
    public class UserInfo
    {
        public UserInfo() { }

        public UserInfo(User user)
        {
            if (user != null)
            {
                Id = user.Id;
                Name = user.Name;
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}
