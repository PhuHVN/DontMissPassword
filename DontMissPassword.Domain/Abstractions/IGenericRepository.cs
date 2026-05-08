using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Domain.Abstractions
{
    public interface IGenericRepository <T> where T : class
    {
        IQueryable<T> Entity { get; }
        Task<T?> FindByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<T?> GetByIdAsync(Object id);
        Task AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Object id);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<IList<T>> FilterByAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<BasePaginatedList<T>> GetPagging(IQueryable<T> query, int index, int pageSize);
        IQueryable<T> GetQueryable();
        Task DeleteRangeAsync(IEnumerable<T> entities);

        Task<BasePaginatedList<object>> GetAllWithPaggingSortSelectionFieldAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            IConfigurationProvider mapperConfig,
            string? searchTerm = null,
            string[]? searchFields = null,
            string? orderBy = null,
            string? fields = null,
            int pageIndex = 1,
            int pageSize = 10);
    }
}
