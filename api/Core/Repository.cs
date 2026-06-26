using Core.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core
{
    public interface IRepository
    {
        /// <summary>
        /// Bắt đầu transaction ở mức DbContext.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Thêm một thực thể mới vào cơ sở dữ liệu và lưu thay đổi.
        /// </summary>
        Task<TEntity> AddAsync<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : EntityBase;

        /// <summary>
        /// Thêm nhiều thực thể mới vào cơ sở dữ liệu và lưu thay đổi.
        /// </summary>
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool saveChanges = true) where TEntity : EntityBase;

        /// <summary>
        /// Xóa cứng một thực thể khỏi cơ sở dữ liệu.
        /// </summary>
        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : EntityBase;

        /// <summary>
        /// Xóa cứng một thực thể khỏi cơ sở dữ liệu theo Id.
        /// </summary>
        Task DeleteAsync<TEntity>(Guid id) where TEntity : EntityBase;

        /// <summary>
        /// Xóa cứng nhiều thực thể theo danh sách Id.
        /// </summary>
        Task DeleteRangeAsync<TEntity>(IEnumerable<Guid> ids) where TEntity : EntityBase;

        /// <summary>
        /// Xóa cứng nhiều thực thể theo điều kiện (filter expression).
        /// </summary>
        Task DeleteRangeAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase;

        /// <summary>
        /// Xóa mềm: đánh dấu một thực thể là đã xóa (`IsDeleted = true`).
        /// </summary>
        Task<bool> DeleteSoftAsync<TEntity>(TEntity entity) where TEntity : EntityBase;

        /// <summary>
        /// Xóa mềm: đánh dấu một thực thể là đã xóa theo Id (`IsDeleted = true`).
        /// </summary>
        Task<bool> DeleteSoftAsync<TEntity>(Guid id) where TEntity : EntityBase;

        /// <summary>
        /// Xóa mềm nhiều thực thể theo danh sách Id (`IsDeleted = true`).
        /// </summary>
        Task DeleteRangeSoftAsync<TEntity>(IEnumerable<Guid> ids) where TEntity : EntityBase;

        /// <summary>
        /// Xóa mềm nhiều thực thể theo điều kiện (filter expression) (`IsDeleted = true`).
        /// </summary>
        Task DeleteRangeSoftAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase;

        /// <summary>
        /// Lấy danh sách (có chọn lọc và projection) theo điều kiện và có thể include các bảng liên quan.
        /// </summary>
        Task<IEnumerable<TModel>> FindAllAsync<TEntity, TModel>(
            Expression<Func<TEntity, bool>> where = null,
            Expression<Func<TEntity, TModel>> selector = null,
            IEnumerable<string> includes = null) where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Lấy danh sách từ một IQueryable đã được build sẵn và áp dụng projection (selector).
        /// </summary>
        Task<IEnumerable<TModel>> FindAllAsync<TEntity, TModel>(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, TModel>> selector = null) where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Lấy danh sách tất cả thực thể (lọc theo điều kiện nếu có) và không tracking.
        /// </summary>
        Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>>? where = null) where TEntity : EntityBase;

        /// <summary>
        /// Lấy danh sách thực thể theo điều kiện và có thể include các bảng liên quan.
        /// </summary>
        Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            Expression<Func<TEntity, bool>> where = null,
            IEnumerable<string>? includes = null) where TEntity : EntityBase;

        /// <summary>
        /// Lấy danh sách theo projection (selector) giữa TModel và TEntity.
        /// </summary>
        Task<IEnumerable<TModel>> FindAllAsync<TModel, TEntity>(Expression<Func<TEntity, TModel>> selector)
            where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Tìm một thực thể theo điều kiện (filter) và có thể include các bảng liên quan.
        /// </summary>
        Task<TEntity> FindAsync<TEntity>(
            Expression<Func<TEntity, bool>> where = null,
            IEnumerable<string> includes = null) where TEntity : EntityBase;

        /// <summary>
        /// Lấy thực thể đầu tiên trong bảng (không filter).
        /// </summary>
        Task<TEntity> FirstAsync<TEntity>() where TEntity : EntityBase;

        /// <summary>
        /// Lấy thực thể đầu tiên theo điều kiện.
        /// </summary>
        Task<TEntity> FirstAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : EntityBase;

        /// <summary>
        /// Tìm một thực thể theo Id và có thể include các bảng liên quan.
        /// </summary>
        Task<TEntity> FindAsync<TEntity>(Guid id, IEnumerable<string> includes = null) where TEntity : EntityBase;

        /// <summary>
        /// Tìm một thực thể theo Id.
        /// </summary>
        Task<TEntity> FindAsync<TEntity>(Guid id) where TEntity : EntityBase;

        /// <summary>
        /// Tìm theo Id và projection sang TModel, có thể include các bảng liên quan.
        /// </summary>
        Task<TModel> FindAsync<TEntity, TModel>(
            Guid id,
            Expression<Func<TEntity, TModel>> selector,
            IEnumerable<string> includes = null) where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Tìm thực thể đầu tiên hoặc giá trị mặc định (trả về null nếu không có dữ liệu).
        /// </summary>
        Task<TEntity> FirstOrDefautAynsc<TEntity>() where TEntity : EntityBase;

        /// <summary>
        /// Lấy thực thể mới nhất (theo CreatedAt) và có thể include các bảng liên quan.
        /// </summary>
        Task<TEntity> FindNewestAsync<TEntity>(IEnumerable<string> includes) where TEntity : EntityBase;

        /// <summary>
        /// Lấy IQueryable của TEntity (không filter theo include), dùng cho các truy vấn nâng cao.
        /// </summary>
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : EntityBase;

        /// <summary>
        /// Cập nhật một thực thể và lưu thay đổi.
        /// </summary>
        Task<TEntity> UpdateAsync<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : EntityBase;

        /// <summary>
        /// Cập nhật nhiều thực thể và lưu thay đổi.
        /// </summary>
        Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entites, bool saveChanges = true) where TEntity : EntityBase;

        /// <summary>
        /// Lưu tất cả thay đổi đang có trên DbContext.
        /// </summary>
        Task SaveChangeAsync();

        /// <summary>
        /// Lấy IQueryable của TEntity và include các bảng liên quan.
        /// </summary>
        IQueryable<TEntity> GetQueryable<TEntity>(IEnumerable<string> includes) where TEntity : EntityBase;

        /// <summary>
        /// Đếm số lượng thực thể theo điều kiện (nếu có).
        /// </summary>
        Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase;

        /// <summary>
        /// Tìm kiếm có phân trang và projection sang TModel, từ một IQueryable đầu vào.
        /// </summary>
        Task<PagedResult<TModel>> FindPagedAsync<TEntity, TModel>(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, TModel>> selector = null,
            int? page = 1,
            int? pageSize = 20)
            where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Tìm kiếm có phân trang trên IQueryable đã là kiểu TModel.
        /// </summary>
        Task<PagedResult<TModel>> FindPagedAsync<TModel>(
            IQueryable<TModel> query,
            int? page = 1,
            int? pageSize = 15) where TModel : class;

        /// <summary>
        /// Tìm kiếm có phân trang theo điều kiện (where) và projection sang TModel.
        /// </summary>
        Task<PagedResult<TModel>> FindPagedAsync<TEntity, TModel>(
            Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, TModel>> selector = null,
            int? page = 1,
            int? pageSize = 20)
            where TEntity : EntityBase where TModel : class;

        /// <summary>
        /// Tìm theo Id và projection sang TModel, có thể include các bảng liên quan.
        /// </summary>
        Task<TModel> FindAsync<TModel, TEntity>(
            Guid id,
            IEnumerable<string> includes = null,
            Expression<Func<TEntity, TModel>> selector = null)
            where TModel : class
            where TEntity : EntityBase;

        /// <summary>
        /// Kiểm tra tồn tại của thực thể theo điều kiện (return true/false).
        /// </summary>
        Task<bool> IsExistsAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : EntityBase;
    }

    /// <summary>
    /// Lớp Repository cung cấp các CRUD cho các thực thể trong cơ sở dữ liệu
    /// </summary>
    public class Repository : IRepository
    {
        private readonly AppDbContext _context;
        private readonly IAppContextAccessor _accessor;

        // Hàm khởi tạo Repository với tham chiếu đến DbContext và IAppContextAccessor
        public Repository(AppDbContext context, IAppContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Thêm một thực thể vào cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity">Thực thể</typeparam>
        /// <param name="entity">Giá trị thực thể</param>
        /// <returns></returns>
        public async Task<TEntity> AddAsync<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : EntityBase
        {
            var userId = _accessor.GetCurrentUserId();
            if (userId != null)
            {
                entity.CreatedBy = userId;
                entity.ModifiedBy = userId;
            }

            entity.IsDeleted = false;
            entity.CreatedAt = DateTime.Now;
            entity.ModifiedAt = DateTime.Now;

            _context.Set<TEntity>().Add(entity);
            if (saveChanges)
            {
                await SaveChangeAsync();
            }
            return entity;
        }

        /// <summary>
        /// Thêm nhiều thực thể vào cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity">Thực thể</typeparam>
        /// <param name="entities">Giá trị thực thể</param>
        /// <returns></returns>
        public async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool saveChanges = true) where TEntity : EntityBase
        {
            var entityTemps = entities.ToList();
            entityTemps.ForEach(x =>
            {
                x.CreatedBy = _accessor.GetCurrentUserId();
                x.ModifiedBy = _accessor.GetCurrentUserId();
                x.CreatedAt = DateTime.Now;
                x.ModifiedAt = DateTime.Now;
            });

            _context.Set<TEntity>().AddRange(entityTemps);
            if (saveChanges)
            {
                await SaveChangeAsync();
            }
        }

        /// <summary>
        /// Xóa cứng - xóa một thực thể khỏi cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            _context.Set<TEntity>().Remove(entity);
            await SaveChangeAsync();
            return;
        }

        /// <summary>
        /// Xóa cứng - xóa một thực thể khỏi cơ sở dữ liệu bằng Id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync<TEntity>(Guid id) where TEntity : EntityBase
        {
            var entity = await AsQueryable<TEntity>().FirstOrDefaultAsync(c => c.Id.Equals(id));
            if (entity != null)
            {
                _context.Set<TEntity>().Remove(entity);
                await SaveChangeAsync();
                return;
            }
        }

        /// <summary>
        /// Xóa cứng - xóa nhiều thực thể khỏi cơ sở dữ liệu bằng danh sách Id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteRangeAsync<TEntity>(IEnumerable<Guid> ids) where TEntity : EntityBase
        {
            var entities = AsQueryable<TEntity>().Where(c => ids.Contains(c.Id));
            _context.RemoveRange(entities);
            await SaveChangeAsync();
        }

        /// <summary>
        /// Xóa cứng - xóa nhiều thực thể khỏi cơ sở dữ liệu bằng điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task DeleteRangeAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase
        {
            ArgumentNullException.ThrowIfNull(where);
            var entities = _context.Set<TEntity>().Where(where);
            _context.RemoveRange(entities);
            await SaveChangeAsync();
        }

        /// <summary>
        /// Xóa mềm - một thực thể khỏi cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteSoftAsync<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            entity.IsDeleted = true;
            entity.ModifiedAt = DateTime.Now;
            _context.Set<TEntity>().Update(entity);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Xóa mềm - xóa một thực thể khỏi cơ sở dữ liệu bằng Id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteSoftAsync<TEntity>(Guid id) where TEntity : EntityBase
        {
            var entity = await AsQueryable<TEntity>().FirstOrDefaultAsync(c => c.Id.Equals(id));
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.ModifiedAt = DateTime.Now;
                _context.Set<TEntity>().Update(entity);

                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        /// <summary>
        /// Xóa mềm - xóa nhiều thực thể khỏi cơ sở dữ liệu bằng danh sách Id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="ids">Danh sách id</param>
        /// <returns></returns>
        public async Task DeleteRangeSoftAsync<TEntity>(IEnumerable<Guid> ids) where TEntity : EntityBase
        {
            var entities = await AsQueryable<TEntity>().Where(c => ids.Contains(c.Id))
                .ToListAsync();
            entities.ForEach(element =>
            {
                element.IsDeleted = true;
            });

            _context.Set<TEntity>().UpdateRange(entities);
            await SaveChangeAsync();
        }

        /// <summary>
        /// Xóa mềm - xóa nhiều thực thể khỏi cơ sở dữ liệu bằng điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task DeleteRangeSoftAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase
        {
            ArgumentNullException.ThrowIfNull(where);
            var entities = await AsQueryable<TEntity>().Where(where)
                .ToListAsync();
            entities.ForEach(element => element.IsDeleted = true);
            _context.Set<TEntity>().UpdateRange(entities);
            await SaveChangeAsync();
        }

        /// <summary>
        /// Danh sách tất cả thực thể
        /// </summary>
        /// <typeparam name="TEntity">Entity</typeparam>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="where">Điều kiện lọc</param>
        /// <param name="selector">Hàm chuyển đổi</param>
        /// <param name="includes">Các bảng cần join</param>
        /// <returns></returns>
        public async Task<IEnumerable<TModel>> FindAllAsync<TEntity, TModel>(Expression<Func<TEntity, bool>> where = null, Expression<Func<TEntity, TModel>> selector = null, IEnumerable<string> includes = null) where TEntity : EntityBase where TModel : class
        {
            ArgumentNullException.ThrowIfNull(selector);
            var query = AsQueryable<TEntity>(includes).AsNoTracking();
            if (where != null)
            {
                query = query.Where(where);
            }

            return await query.Select(selector).ToListAsync();
        }

        /// <summary>
        /// Danh thực thể từ một truy vấn cụ thể và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TEntity">Entity</typeparam>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="selector">Selector</param>
        /// <returns></returns>
        public async Task<IEnumerable<TModel>> FindAllAsync<TEntity, TModel>(IQueryable<TEntity> query, Expression<Func<TEntity, TModel>> selector = null) where TEntity : EntityBase where TModel : class
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(selector);
            query = query.OrderByDescending(x => x.CreatedAt).AsQueryable();
            return await query.Select(selector).ToListAsync();
        }

        /// <summary>
        /// Danh sách thực thể theo điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>>? where = null) where TEntity : EntityBase
        {
            var query = GetQueryable<TEntity>();
            if (where != null)
            {
                query = query.Where(where);
            }
            return await query.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Danh sách thực thể theo điều kiện và bao gồm các thực thể liên quan
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<TEntity, bool>> where = null, IEnumerable<string>? includes = null) where TEntity : EntityBase
        {
            if (where is null) return await GetQueryable<TEntity>(includes).AsNoTracking().ToListAsync();

            return await GetQueryable<TEntity>(includes).Where(where).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Danh sách thực thể và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TModel>> FindAllAsync<TModel, TEntity>(Expression<Func<TEntity, TModel>> selector) where TEntity : EntityBase where TModel : class
        {
            ArgumentNullException.ThrowIfNull(selector);
            var query = GetQueryable<TEntity>().Select(selector);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm một thực thể theo điều kiện và bao gồm các thực thể liên quan
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> where = null, IEnumerable<string> includes = null) where TEntity : EntityBase
        {
            var query = AsQueryable<TEntity>(includes);
            if (where != null)
            {
                query = query.Where(where);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm kiếm thực thể đầu tiên trong bảng
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<TEntity> FirstAsync<TEntity>() where TEntity : EntityBase
        {
            return await AsQueryable<TEntity>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm kiếm thực thể đầu tiên theo điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<TEntity> FirstAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : EntityBase
        {
            ArgumentNullException.ThrowIfNull(where);
            return await AsQueryable<TEntity>().Where(where).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm kiếm một thực thể theo Id và bao gồm các thực thể liên quan
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<TEntity> FindAsync<TEntity>(Guid id, IEnumerable<string> includes) where TEntity : EntityBase
        {
            return await AsQueryable<TEntity>(includes)
                .AsNoTracking().FirstOrDefaultAsync(c => c.Id.Equals(id));
        }

        /// <summary>
        /// Tìm kiếm một thực thể theo Id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> FindAsync<TEntity>(Guid id) where TEntity : EntityBase
        {
            return await GetQueryable<TEntity>().AsNoTracking().FirstOrDefaultAsync(c => c.Id.Equals(id));
        }

        /// <summary>
        /// Tìm kiếm một thực thể theo Id và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="id"></param>
        /// <param name="selector"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<TModel> FindAsync<TEntity, TModel>(Guid id, Expression<Func<TEntity, TModel>> selector, IEnumerable<string> includes = null) where TEntity : EntityBase where TModel : class
        {
            ArgumentNullException.ThrowIfNull(selector);
            var query = AsQueryable<TEntity>(includes);
            var model = await query.Where(x => x.Id.Equals(id))
                                   .Select(selector)
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync();

            return model;
        }

        /// <summary>
        /// Tìm kiếm thực thể đầu tiên hoặc giá trị mặc định
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<TEntity> FirstOrDefautAynsc<TEntity>() where TEntity : EntityBase
        {
            return await AsQueryable<TEntity>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tìm kiếm thực thể mới nhất theo ngày tạo
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<TEntity> FindNewestAsync<TEntity>(IEnumerable<string> includes) where TEntity : EntityBase
        {
            var query = GetQueryable<TEntity>(includes);
            query = query.OrderByDescending(c => c.CreatedAt);
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy truy vấn cho một thực thể
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : EntityBase
        {
            return AsQueryable<TEntity>();
        }

        /// <summary>
        /// Cập nhật một thực thể trong cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<TEntity> UpdateAsync<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : EntityBase
        {
            var userId = _accessor.GetCurrentUserId();
            if (userId != null)
            {
                entity.ModifiedBy = userId;
            }

            entity.ModifiedAt = DateTime.Now;
            _context.Set<TEntity>().Update(entity);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return entity;
        }

        /// <summary>
        /// Cập nhật nhiều thực thể trong cơ sở dữ liệu
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entites"></param>
        /// <returns></returns>
        public async Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entites, bool saveChanges = true) where TEntity : EntityBase
        {
            _context.UpdateRange(entites);
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Lưu thay đổi vào cơ sở dữ liệu
        /// </summary>
        /// <returns></returns>
        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Tạo truy vấn cho một thực thể với các thực thể liên quan
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        private IQueryable<TEntity> AsQueryable<TEntity>(IEnumerable<string> includes = null) where TEntity : EntityBase
        {
            var query = _context.Set<TEntity>()
                                .Where(x => !x.IsDeleted)
                                .OrderByDescending(x => x.CreatedAt)
                                .AsQueryable();

            if (includes?.Any() != true)
            {
                return query;
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        /// <summary>
        /// Lấy truy vấn cho một thực thể với các thực thể liên quan
        /// </summary>
        /// <typeparam name="TEntity">Entity</typeparam>
        /// <param name="includes">Tables</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetQueryable<TEntity>(IEnumerable<string> includes) where TEntity : EntityBase
        {
            var query = GetQueryable<TEntity>();
            if (includes?.FirstOrDefault() != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        /// <summary>
        /// Đếm số lượng thực thể theo điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> where = null) where TEntity : EntityBase
        {
            var query = GetQueryable<TEntity>();
            if (where != null)
            {
                query = query.Where(where);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Tìm kiếm có phân trang và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="selector"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PagedResult<TModel>> FindPagedAsync<TEntity, TModel>(IQueryable<TEntity> query, Expression<Func<TEntity, TModel>> selector = null, int? page = 1, int? pageSize = 20)
            where TEntity : EntityBase
            where TModel : class
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(selector);
            var (p, s) = ResolvePaging(page, pageSize, 20);
            var modelQuery = query.Select(selector);
            var count = await modelQuery.CountAsync();
            var items = await modelQuery.Skip((p - 1) * s).Take(s).ToListAsync();

            return new PagedResult<TModel>(items, count, p, s);
        }

        /// <summary>
        /// Tìm kiếm có phân trang
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PagedResult<TModel>> FindPagedAsync<TModel>(IQueryable<TModel> query, int? page = 1, int? pageSize = 15) where TModel : class
        {
            ArgumentNullException.ThrowIfNull(query);
            var (p, s) = ResolvePaging(page, pageSize, 15);
            var count = await query.CountAsync();
            var items = await query.Skip((p - 1) * s).Take(s).ToListAsync();

            return new PagedResult<TModel>(items, count, p, s);
        }

        /// <summary>
        /// Tìm kiếm có phân trang và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="where"></param>
        /// <param name="selector"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PagedResult<TModel>> FindPagedAsync<TEntity, TModel>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TModel>> selector = null, int? page = 1, int? pageSize = 20)
            where TEntity : EntityBase
            where TModel : class
        {
            ArgumentNullException.ThrowIfNull(where);
            var query = GetQueryable<TEntity>().Where(where);
            return await FindPagedAsync(query, selector, page, pageSize);
        }

        /// <summary>
        /// Tìm kiếm một thực thể theo Id, 
        /// bao gồm các thực thể liên quan và chọn kết quả theo một bộ chọn cụ thể
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task<TModel> FindAsync<TModel, TEntity>(Guid id, IEnumerable<string> includes = null, Expression<Func<TEntity, TModel>> selector = null)
            where TModel : class
            where TEntity : EntityBase
        {
            ArgumentNullException.ThrowIfNull(selector);
            var model = await GetQueryable<TEntity>(includes)
                .Where(x => x.Id.Equals(id))
                .Select(selector).FirstOrDefaultAsync();

            return model;
        }

        /// <summary>
        /// Kiểm tra sự tồn tại của thực thể theo điều kiện
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<bool> IsExistsAsync<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : EntityBase
        {
            ArgumentNullException.ThrowIfNull(where);
            var query = GetQueryable<TEntity>().Where(where);
            return await query.AnyAsync();
        }

        private static (int Page, int PageSize) ResolvePaging(int? page, int? pageSize, int defaultPageSize)
        {
            var p = page ?? 1;
            var s = pageSize ?? defaultPageSize;
            if (p < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page phải >= 1.");
            }

            if (s < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize phải >= 1.");
            }

            return (p, s);
        }
    }
}
