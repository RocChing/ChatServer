using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
    }
}
