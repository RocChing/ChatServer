using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ChatModel.Entity;
using System.Linq;

namespace ChatRepository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ChatDbContext db;
        public Repository(ChatDbContext chatDbContext)
        {
            db = chatDbContext;
        }

        public bool DeleteById(int id)
        {
            var model = db.Set<T>().Find(id);
            model.Enabled = 0;
            int count = db.SaveChanges();
            return count > 0;
        }

        public T Get(Expression<Func<T, bool>> where)
        {
            return db.Set<T>().FirstOrDefault(where);
        }

        public T GetById(int id)
        {
            return db.Set<T>().Find(id);
        }

        public IEnumerable<T> GetList(Expression<Func<T, bool>> where)
        {
            return db.Set<T>().Where(where).ToList();
        }

        public T InsertOrUpdate(T model)
        {
            bool addFlag = model.Id < 1;
            if (addFlag)
            {
                model.Enabled = 1;
                model.CreateTime = DateTime.Now;
                db.Set<T>().Add(model);
            }
            else
            {
                model.UpdateTime = DateTime.Now;
                db.Attach(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            db.SaveChanges();
            return model;
        }
    }
}
