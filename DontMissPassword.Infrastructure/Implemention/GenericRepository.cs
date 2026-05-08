using AutoMapper;
using AutoMapper.QueryableExtensions;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Infrastructure.DatabaseSettings;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Infrastructure.Implemention
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(AppDbContext appDb)
        {
            _context = appDb;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> Entity => _dbSet;

        public async Task DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<IList<T>> FilterByAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null)
                query = include(query);

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<IList<T>> GetAllAsync(
         Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null)
                query = include(query);

            return await query.ToListAsync();
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<T?> FindByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public Task<T?> GetByIdAsync(object id)
        {
            var entity = _dbSet.FindAsync(id);
            return entity.AsTask();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var id = _context.Entry(entity).Property("Id").CurrentValue;
            var exist = await _dbSet.FindAsync(id);

            if (exist == null)
            {
                throw new KeyNotFoundException($"Entity with Id {id} not found.");
            }
            _context.Entry(exist).CurrentValues.SetValues(entity);
            return exist;
        }

        public async Task<BasePaginatedList<T>> GetPagging(IQueryable<T> query, int index, int pageSize)
        {
            query = query.OrderBy(x => EF.Property<object>(x, "Id"));
            var count = await query.CountAsync();
            var items = await query.Skip((index - 1) * pageSize).Take(pageSize).ToListAsync();
            return new BasePaginatedList<T>(items, count, index, pageSize);
        }
        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }
        public async Task<BasePaginatedList<object>> GetAllWithPaggingSortSelectionFieldAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            Microsoft.Extensions.Configuration.IConfigurationProvider mapperConfig,
            string? searchTerm = null,
            string[]? searchFields = null,
            string? orderBy = null,
            string? fields = null,
            int pageIndex = 1,
            int pageSize = 10)
        {



            // 1. Validation Fields dựa trên TResponse
            // Nói cách khác , chỉ những field nào tồn tại trong TResponse mới được phép chọn và sắp xếp
            // Việc này tránh select data không cần thiết từ database, đồng thời cũng giúp bảo mật khi có những field nhạy cảm tồn tại trong TEntity nhưng không tồn tại trong TResponse
            var validFields = QueryHelper.GetValidFields<TResponse>(fields);
            var validOrderBy = QueryHelper.GetValidOrderBy<TResponse>(orderBy);

            var count = await query.CountAsync();

            // 2. ProjectTo trước để chuyển từ Entity -> DTO
            // Việc này giúp giấu các field nhạy cảm ngay từ đầu
            // Thay vì query hết từ entity rồi mới chọn field, thì bây giờ chỉ query những field cần thiết đã được map sang DTO

            // Tại sao lại cấn projectTo này --> Nếu không có ProjectTo, thì query sẽ trả về TEntity, sau đó mới chọn field động trên TEntity. Điều này sẽ gây ra lỗi nếu có field nào đó tồn tại trong TEntity nhưng không tồn tại trong TResponse.
            var dtoQuery = query.ProjectTo<TResponse>(mapperConfig);

            if (!string.IsNullOrEmpty(searchTerm) && searchFields != null && searchFields.Any())
            {
                // Chỉ tạo filter dựa trên danh sách searchFields bạn đưa vào
                var filterExpression = string.Join(" || ", searchFields.Select(f => $"{f}.ToLower().Contains(@0)"));
                dtoQuery = dtoQuery.Where(filterExpression, searchTerm.ToLower());
            }
            // 3. Sorting trên DTO
            // orderBy sẽ được truyền vào dưới dạng string, ví dụ: "Name desc, Age asc"
            // lib: System.Linq.Dynamic.Core sẽ parse string này và áp dụng sorting động trên DTO
            if (!string.IsNullOrWhiteSpace(validOrderBy))
                dtoQuery = dtoQuery.OrderBy(validOrderBy);

            // 4. Paging
            var pagedQuery = dtoQuery.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            // 5. Select Field động trên DTO
            // Kết quả trả về List<object> (kiểu vô danh chứa các field của TResponse)
            // Cái này cũng có mặt hại là không thẻ strong type được nữa, nhưng bù lại có thể linh hoạt chọn field nào cần thiết để trả về, tránh trả về data không cần thiết
            // Và cũng khó khăn hơn trong việc sử dụng kết quả trả về, vì phải dùng dynamic để truy cập các field
            var items = await pagedQuery
                .Select($"new ({validFields})")
                .ToDynamicListAsync();

            return new BasePaginatedList<object>(items.Cast<object>().ToList(), count, pageIndex, pageSize);
        }

    } 
}
public static class QueryHelper
{
    // Utils 1: Lọc chuỗi Fields để chỉ lấy các field có trong DTO và luôn ép có Id
    public static string GetValidFields<TDto>(string? fields)
    {
        var allowedFields = typeof(TDto).GetProperties()
            .Select(p => p.Name)
            .ToList();

        if (string.IsNullOrWhiteSpace(fields))
            return string.Join(", ", allowedFields); // Trả về tất cả field của DTO nếu ko truyền

        var requestedFields = fields.Split(',')
            .Select(f => f.Trim())
            .Where(f => allowedFields.Any(af => string.Equals(af, f, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Luôn đảm bảo có Id (nếu DTO có Id)
        if (allowedFields.Contains("Id") && !requestedFields.Any(f => f.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            requestedFields.Insert(0, "Id");

        return requestedFields.Any() ? string.Join(", ", requestedFields) : string.Join(", ", allowedFields);
    }

    // Utils 2: Kiểm tra chuỗi OrderBy có hợp lệ không
    public static string? GetValidOrderBy<TDto>(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) return null;

        var allowedFields = typeof(TDto).GetProperties().Select(p => p.Name).ToList();

        // Tách chuỗi "Name DESC, Age ASC" -> "Name", "Age"
        var parts = orderBy.Split(',')
            .Select(p => p.Trim().Split(' ')[0])
            .ToList();

        // Nếu có bất kỳ field nào không nằm trong DTO -> Trả về null hoặc mặc định (để tránh lỗi crash)
        bool isValid = parts.All(p => allowedFields.Any(af => string.Equals(af, p, StringComparison.OrdinalIgnoreCase)));

        return isValid ? orderBy : null;
    }

     
}