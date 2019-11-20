using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;

namespace ChatRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatDbContext db;
        public UserRepository(ChatDbContext dbContext)
        {
            db = dbContext;
        }

        public void Insert()
        {
            User user = new User();
            user.Name = "zmm";
            user.CreateTime = DateTime.Now;
            user.UpdateTime = user.CreateTime;
            db.Users.Add(user);
            db.SaveChanges();
        }
    }

    public interface IUserRepository
    {
        void Insert();
    }
}
