using KhatiExtendedEF.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace KhatiExtendedEF.Context
{
	public class DatabaseContextTypeOfIdentityUser<T, TIdentity, TKey> : IdentityDbContext<TIdentity, IdentityRole<TKey>, TKey>
	where T : class
	where TIdentity : IdentityUser<TKey>
	where TKey : IEquatable<TKey>
	{
		public virtual string connectionString()
		{
			return "";
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(connectionString());
			}
		}

		private Type EntityType()
		{
			return typeof(T);
		}

		// ... [Your existing reflection helper methods remain unchanged] ...
		// ... [GetTypeFromDifferentAssembly method] ...

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Your existing reflection logic
			List<EntityContext> list = new List<EntityContext>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				try
				{
					if (assembly.GetName().Name == "Microsoft.Data.SqlClient") continue;

					// Added simple null check logic to your loop
					var distinctTypes = assembly.GetTypes()
						.Where(type => EntityType().IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
						.Select(type => new { type.FullName, type.Name })
						.ToArray();

					foreach (var anon in distinctTypes)
					{
						if (string.IsNullOrEmpty(anon.FullName)) continue;

						Type typeFromDifferentAssembly = assembly.GetType(anon.FullName); // Simplified lookup

						if (typeFromDifferentAssembly != null)
						{
							list.Add(new EntityContext
							{
								Entity = anon.Name,
								Type = typeFromDifferentAssembly
							});
						}
					}
				}
				catch (Exception ex) {
					throw new Exception(string.Format("Error From KhatiExtendedEf: ", ex.Message));
				}
			}

			ConfigureEntities(modelBuilder, list);
		}

		private void ConfigureEntities(ModelBuilder modelBuilder, List<EntityContext> types)
		{
			foreach (var item in types)
			{
				if (item != null && item.Type != null && !string.IsNullOrEmpty(item.Entity))
				{
					modelBuilder.Entity(item.Type).ToTable(item.Entity);
				}
			}
			EntityBinder(modelBuilder);
		}

		public virtual void EntityBinder(ModelBuilder modelBuilder) { }
	}
}
