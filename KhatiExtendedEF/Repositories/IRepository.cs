using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace KhatiExtendedEF.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, new()
    {
        Task<EntityEntry<TEntity>> Insert(TEntity model);
        Task<EntityEntry<TEntity>> Update(TEntity model);
        Task<EntityEntry<TEntity>> Delete(TEntity model);
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> expression);
        Task<TEntity?> GetEntity(Expression<Func<TEntity, bool>> expression);
        IQueryable<TEntity> Get();
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> expression = null);

        #region Commits
        Task<(bool success, string? message, string? errorMessage)> Commit(Func<Task> operations);
        Task<(bool success, string? message, string? errorMessage)> Commit(Action operations);
        Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<Task<T?>> operations);
        Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<T> operations);
        #endregion
    }
}
