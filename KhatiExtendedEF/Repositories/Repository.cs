using KhatiExtendedEF.Context;
using KhatiExtendedEF.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace KhatiExtendedEF.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly DbContext _databaseContext;
        private readonly IServiceProvider _serviceProvider;
        public Repository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var getInterface = GetImplementedInterface(typeof(TEntity));
            var dbContext = GetDbContextForEntity(getInterface);
            _databaseContext = (DbContext)_serviceProvider.GetService(dbContext);
        }

        public virtual async Task<EntityEntry<TEntity>> InsertAsync(TEntity model)
        {
            var insert = await _databaseContext.Set<TEntity>().AddAsync(model);
            return insert;
        }
        public virtual EntityEntry<TEntity> Update(TEntity model)
        {
            var update = _databaseContext.Set<TEntity>().Update(model);
            return update;
        }
        public virtual EntityEntry<TEntity> Delete(TEntity model)
        {
            var delete = _databaseContext.Set<TEntity>().Remove(model);
            return delete;
        }

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> expression)
        {
            var list = _databaseContext.Set<TEntity>().Where(expression).AsNoTracking();
            return list;
        }

        public virtual async Task<PaginationResponseModel<TEntity>> GetPagination(Expression<Func<TEntity, bool>> expression, int pageSize = 10, int pageIndex = 1)
        {
            var list = _databaseContext.Set<TEntity>().Where(expression).AsNoTracking();
            var pagination = await PaginationAsync(list, pageSize, pageIndex);
            return pagination;
        }

        public virtual async Task<TEntity?> GetEntity(Expression<Func<TEntity, bool>> expression)
        {
            var model = await _databaseContext.Set<TEntity>().Where(expression).AsNoTracking().FirstOrDefaultAsync();
            return model;
        }
        public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            var list = new List<TEntity>();
            if (expression != null)
                list = await _databaseContext.Set<TEntity>().Where(expression).AsNoTracking().ToListAsync();
            else
                list = await _databaseContext.Set<TEntity>().AsNoTracking().ToListAsync();
            return list;
        }
        public IQueryable<TEntity> Get()
        {
            var model = _databaseContext.Set<TEntity>().AsNoTracking();
            return model;
        }

        #region Commits
        public async Task<(bool success, string? message, string? errorMessage)> Commit(Func<Task> operations)
        {
            try
            {
                await operations();
                await _databaseContext.SaveChangesAsync();
                return (true, "Operation Successfull", null);
            }
            catch (Exception ex)
            {
                return (false, "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, string? message, string? errorMessage)> Commit(Action operations)
        {
            try
            {
                operations();
                await _databaseContext.SaveChangesAsync();
                return (true, "Operation Successfull", null);
            }
            catch (Exception ex)
            {
                return (false, "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<Task<T?>> operations)
        {
            try
            {
                var data = await operations();
                await _databaseContext.SaveChangesAsync();
                return (true, data, "Operation Successfull", null);
            }
            catch (Exception ex)
            {
                return (false, default(T), "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<T> operations)
        {
            try
            {
                var data = operations();
                await _databaseContext.SaveChangesAsync();
                return (true, data, "Operation Successfull", null);
            }
            catch (Exception ex)
            {
                return (false, default(T), "Operation Failed", ex.Message);
            }
        }

        private async Task<PaginationResponseModel<T>> PaginationAsync<T>(IQueryable<T> list, int pageSize = 10, int pageIndex = 1) where T : class
        {
            var model = new PaginationResponseModel<T>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalData = await list.CountAsync(),
                Data = await list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(),
            };

            model.HasNextPage = Math.Ceiling(Convert.ToDecimal(model.TotalData) / Convert.ToDecimal(pageSize)) > model.PageIndex;
            model.HasPreviousPage = model.TotalData > pageSize && model.PageIndex > 1;

            return model;
        }


        #endregion

        #region Reflections
        private Type? GetImplementedInterface(Type type)
        {
            var entity = type.GetInterfaces().FirstOrDefault();
            if (entity == null)
            {
                throw new Exception("This Model Is not a Database Entity");
            }
            return entity;
        }
        private Type? GetDbContextForEntity(Type? entityType)
        {
            if (entityType == null)
            {
                throw new Exception("This Model is not a Database Entity");
            }
            var contextInherited = typeof(DatabaseContext<>).MakeGenericType(entityType);
            var dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => contextInherited.IsAssignableFrom(p) && !p.IsAbstract)
                .FirstOrDefault();

            if (dbContextType == null)
            {
                contextInherited = typeof(DatabaseContextIdentityUser<,>).MakeGenericType(entityType, typeof(IdentityUser));

                dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => contextInherited.IsAssignableFrom(p) && !p.IsAbstract)
                .FirstOrDefault();

            }

            if (dbContextType == null)
            {
                throw new Exception("The Entity Does not belongs to any DBContext. Make Sure to Inherit the DBContext Interface Identifier at the model class very beginning. If you have multiple inheritance of interfaces inside model class.");
            }

            return dbContextType;
        }
        #endregion
    }
}
