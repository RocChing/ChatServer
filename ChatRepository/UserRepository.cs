using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;
using ChatModel.Input;
using ChatModel.Util;

namespace ChatRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ChatDbContext dbContext) : base(dbContext)
        {

        }

        public User Login(LoginInfo input)
        {
            if (!input.IsValid()) return null;

            string password = StringUtil.GetMd5String(input.Password);

            return this.Get(u => u.Enabled == 1 && u.Password == password && (u.Name == input.Name || u.Phone == input.Name));
        }
    }

    public interface IUserRepository : IRepository<User>
    {
        User Login(LoginInfo input);
    }
}
