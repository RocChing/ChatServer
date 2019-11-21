using System;
using System.Collections.Generic;
using System.Text;
using ChatModel.Entity;
using System.Linq.Expressions;
using System.Linq;

namespace ChatRepository
{
    public interface IRepository<T> where T : BaseEntity
    {
        T InsertOrUpdate(T model);

        T Get(Expression<Func<T, bool>> where);

        T GetById(int id);

        IEnumerable<T> GetList(Expression<Func<T, bool>> where);

        bool DeleteById(int id);
    }
}
