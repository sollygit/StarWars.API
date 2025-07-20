using Microsoft.EntityFrameworkCore;
using StarWars.Interface;
using System.Linq.Expressions;

namespace StarWars.Repository
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _entities;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public virtual void Add(T entity)
        {
            _entities.Add(entity);
        }
        public virtual void AddRange(IEnumerable<T> entities)
        {
            _entities.AddRange(entities);
        }
        public virtual void Update(T entity)
        {
            _entities.Update(entity);
        }
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _entities.UpdateRange(entities);
        }
        public virtual void Remove(T entity)
        {
            _entities.Remove(entity);
        }
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _entities.RemoveRange(entities);
        }
        public virtual int Count()
        {
            return _entities.Count();
        }
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _entities.Where(predicate);
        }
        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _entities.SingleOrDefault(predicate);
        }
        public virtual T Get(string id)
        {
            return _entities.Find(id);
        }
        public virtual IEnumerable<T> GetAll()
        {
            return _entities.ToList();
        }
    }
}
