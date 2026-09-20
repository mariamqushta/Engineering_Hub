using Engineering_Hub.models.context;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Engineering_Hub.Repository
{
    public class GenericRepository<T> where T : class
    {
        private readonly EngineeringHubContext _context;

        public GenericRepository(EngineeringHubContext context)
        {
            _context = context;
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }
        public List<T> GetByCondition(
            Expression<Func<T, bool>> condition)
        {
            return _context.Set<T>()
                .Where(condition)
                .ToList();
        }
        public List<T> GetByConditionWithInclude(
        Expression<Func<T, bool>> condition,
        params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query
                .Where(condition)
                .ToList();
        }
        public void add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void Edit(T entity)
        {
            _context.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public void Delete(int id)
        {
            T entity = _context.Set<T>().Find(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
