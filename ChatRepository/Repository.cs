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
        protected readonly ChatDbContext db;
        public Repository(ChatDbContext chatDbContext)
        {
            db = chatDbContext;
        }

        public virtual bool DeleteById(int id)
        {
            var model = db.Set<T>().Find(id);
            model.Enabled = 0;
            int count = db.SaveChanges();
            return count > 0;
        }

        public virtual T Get(Expression<Func<T, bool>> where)
        {
            return db.Set<T>().FirstOrDefault(where);
        }

        public virtual T GetById(int id)
        {
            return db.Set<T>().FirstOrDefault(m => m.Id == id && m.Enabled == 1);
        }

        public virtual IEnumerable<T> GetList(Expression<Func<T, bool>> where)
        {
            return db.Set<T>().Where(where).ToList();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return db.Set<T>().ToList();
        }

        public virtual bool Exist(Expression<Func<T, bool>> where)
        {
            return this.Get(where) != null;
        }

        public virtual T InsertOrUpdate(T model)
        {
            bool addFlag = model.Id < 1;
            if (addFlag)
            {
                model.Enabled = 1;
                model.CreateTime = DateTime.Now;
                model.UpdateTime = model.CreateTime;
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
