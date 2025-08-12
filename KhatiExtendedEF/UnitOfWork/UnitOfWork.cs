using KhatiExtendedEF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KhatiExtendedEF.UnitOfWork
{
    public class UnitOfWork<TEntity> : IUnitOfWork<TEntity>
    {
        private readonly DbContext _dbContext;
        public UnitOfWork(IServiceProvider serviceProvider)
        {
            var dbContextType = GetDbContextForEntity(typeof(TEntity));
            _dbContext = (DbContext)serviceProvider.GetRequiredService(dbContextType);
        }
        public async Task<(bool success, string? message, string? errorMessage)> Commit(Func<Task> operations)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await operations();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Operation Successful", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, string? message, string? errorMessage)> Commit(Action operations)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                operations();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Operation Successful", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<Task<T?>> operations)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var data = await operations();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, data, "Operation Successful", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, default(T), "Operation Failed", ex.Message);
            }
        }
        public async Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<T> operations)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var data = operations();
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, data, "Operation Successful", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, default(T), "Operation Failed", ex.Message);
            }
        }
        private Type GetDbContextForEntity(Type entityType)
        {
            var dbContextType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    !t.IsAbstract &&
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(DatabaseContext<>) &&
                    t.BaseType.GetGenericArguments()[0] == entityType);

            if (dbContextType == null)
                throw new InvalidOperationException($"No DbContext found for entity type {entityType.Name}");

            return dbContextType;
        }
    }
    
}

