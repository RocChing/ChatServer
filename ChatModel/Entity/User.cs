using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ChatModel.Input;

namespace ChatModel.Entity
{
    [Table("T_User")]
    public class User : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        public string Password { get; set; }

        public int Gender { get; set; }

        public string NickName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Avatar { get; set; }

        public User()
        {

        }

        public User(UserExtInfo info)
        {
            Name = info.Name;
            NickName = info.NickName;
            Gender = info.Gender;
            Avatar = info.Avatar;
            Phone = info.Phone;
            Email = string.Empty;
            Enabled = 1;
        }

        public const string PASSWORD = "111111";
    }
}
