using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;

namespace ChatRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ChatDbContext dbContext) : base(dbContext)
        {

        }
    }

    public interface IUserRepository : IRepository<User>
    {

    }
}
