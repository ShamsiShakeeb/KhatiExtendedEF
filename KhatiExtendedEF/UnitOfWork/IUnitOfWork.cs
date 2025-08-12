namespace KhatiExtendedEF.UnitOfWork
{
    public interface IUnitOfWork<TEntity>
    {
        Task<(bool success, string? message, string? errorMessage)> Commit(Func<Task> operations);
        Task<(bool success, string? message, string? errorMessage)> Commit(Action operations);
        Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<Task<T?>> operations);
        Task<(bool success, T? data, string? message, string? errorMessage)> Commit<T>(Func<T> operations);
    }
}
