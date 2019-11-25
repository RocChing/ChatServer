using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;
using ChatModel.Input;
using ChatModel.Util;
using System.Linq;

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

        public List<UserExtInfo> GetListByKey(string key)
        {
            if (key.IsNullOrEmpty())
            {
                return new List<UserExtInfo>();
            }

            return db.Users.Where(u => (u.Name == key || u.Phone == key) && u.Enabled == 1).Select(u => new UserExtInfo()
            {
                Id = u.Id,
                Avatar = u.Avatar,
                Gender = u.Gender,
                NickName = u.NickName,
                Name = u.Name,
                Phone = u.Phone
            }).ToList();
        }

        public override User InsertOrUpdate(User model)
        {
            string existUserMsg = "已经存在该账户";
            bool addFlag = model.Id < 1;
            if (addFlag)
            {
                if (Exist(m => m.Name == model.Name && m.Enabled == 1))
                {
                    throw new Exception(existUserMsg);
                }
            }
            else
            {
                if (Exist(m => m.Name == model.Name && m.Enabled == 1 && m.Id != model.Id))
                {
                    throw new Exception(existUserMsg);
                }
            }
            return base.InsertOrUpdate(model);
        }
    }

    public interface IUserRepository : IRepository<User>
    {
        User Login(LoginInfo input);

        List<UserExtInfo> GetListByKey(string key);
    }
}
