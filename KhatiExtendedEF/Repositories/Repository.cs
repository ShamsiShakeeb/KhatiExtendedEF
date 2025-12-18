using KhatiExtendedEF.Context;
using KhatiExtendedEF.Model;
using KhatiExtendedEF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

// Adjust namespace to match your project
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

			// Ensure we found a context before trying to resolve it
			if (dbContext == null)
			{
				throw new Exception($"Could not find a DbContext for entity {typeof(TEntity).Name}");
			}

			_databaseContext = (DbContext)_serviceProvider.GetService(dbContext);
		}

		public virtual async Task<EntityEntry<TEntity>> InsertAsync(TEntity model)
		{
			var insert = await _databaseContext.Set<TEntity>().AddAsync(model);
			return insert;
		}
		public virtual async Task InsertRangeAsync(IEnumerable<TEntity> model)
		{
			await _databaseContext.Set<TEntity>().AddRangeAsync(model);
		}
		public virtual EntityEntry<TEntity> Update(TEntity model)
		{
			var update = _databaseContext.Set<TEntity>().Update(model);
			return update;
		}
		public virtual void UpdateRange(IEnumerable<TEntity> model)
		{
			_databaseContext.Set<TEntity>().UpdateRange(model);
		}
		public virtual EntityEntry<TEntity> Delete(TEntity model)
		{
			var delete = _databaseContext.Set<TEntity>().Remove(model);
			return delete;
		}
		public virtual void DeleteRange(IEnumerable<TEntity> model)
		{
			_databaseContext.Set<TEntity>().RemoveRange(model);
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

		private async Task<PaginationResponseModel<T>> PaginationAsync<T>(IQueryable<T> list, int pageSize = 10, int pageIndex = 1) where T : class
		{
			var model = new PaginationResponseModel<T>()
			{
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalData = await list.CountAsync(),
				Data = await list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(),
			};

			if (pageSize > 0)
			{
				model.HasNextPage = Math.Ceiling(Convert.ToDecimal(model.TotalData) / Convert.ToDecimal(pageSize)) > model.PageIndex;
				model.HasPreviousPage = model.TotalData > pageSize && model.PageIndex > 1;
			}

			return model;
		}

		#region Reflections

		// --- SAFE TYPE LOADER ---
		// This helper replaces your unchecked .SelectMany(s => s.GetTypes())
		private IEnumerable<Type> GetSafeTypesFromAllAssemblies()
		{
			var allTypes = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies)
			{
				// Skip problematic assemblies explicitly to save time/errors
				if (assembly.IsDynamic ||
					(assembly.FullName?.StartsWith("Microsoft.Data.SqlClient") ?? false) ||
					(assembly.FullName?.StartsWith("System") ?? false))
				{
					continue;
				}

				try
				{
					allTypes.AddRange(assembly.GetTypes());
				}
				catch (ReflectionTypeLoadException ex)
				{
					// If an assembly fails (like SqlClient), grab the types that DID load
					allTypes.AddRange(ex.Types.Where(t => t != null));
				}
				catch
				{
					// Ignore dead assemblies
				}
			}
			return allTypes;
		}

		private Type? GetImplementedInterface(Type type)
		{
			var entity = type.GetInterfaces().FirstOrDefault();
			if (entity == null)
			{
				// Removed Exception throw to avoid breaking flow, allow null check later
				// throw new Exception("This Model Is not a Database Entity"); 
				return null;
			}
			return entity;
		}

		private Type? GetDbContextForEntity(Type? entityType)
		{
			if (entityType == null)
			{
				// If it's not a database entity, we can't find a context.
				// You might want to handle this gracefully or keep the throw.
				throw new Exception($"Model {typeof(TEntity).Name} does not implement an interface required to find its DbContext.");
			}

			var contextInherited = typeof(DatabaseContext<>).MakeGenericType(entityType);

			// USE SAFE LOADER HERE
			var allSafeTypes = GetSafeTypesFromAllAssemblies();

			var dbContextType = allSafeTypes
				.Where(p => contextInherited.IsAssignableFrom(p) && !p.IsAbstract)
				.FirstOrDefault();

			if (dbContextType == null)
			{
				dbContextType = allSafeTypes
					.Where(p =>
						!p.IsAbstract &&
						p.BaseType != null &&
						p.BaseType.IsGenericType &&
						(p.BaseType.GetGenericTypeDefinition() == typeof(DatabaseContextIdentityUser<,>) ||
						 p.BaseType.GetGenericTypeDefinition() == typeof(DatabaseContextTypeOfIdentityUser<,,>)) &&
						p.BaseType.GetGenericArguments()[0] == entityType
					)
					.FirstOrDefault();
			}

			if (dbContextType == null)
			{
				throw new Exception($"The Entity {typeof(TEntity).Name} does not belong to any DbContext. Ensure it inherits the correct interface.");
			}

			return dbContextType;
		}
		#endregion
	}
}