using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetAsync(Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IQueryable<T>>? includes = null);
        Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageSize = 0,
            int pageNumber = 0
        );
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task RemoveAsync(T entity);
    }
}
