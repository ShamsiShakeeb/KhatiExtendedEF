using KhatiExtendedEF.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace KhatiExtendedEF.Context
{
    public class DatabaseContextIdentityUser<T, TIdentity> : IdentityDbContext<TIdentity> where TIdentity : IdentityUser
    {
        public virtual string connectionString() => "";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString());
        }
        private Type EntityType()
        {
            return typeof(T);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var assembly = Assembly.GetExecutingAssembly();

            var implementingClasses = assembly.GetTypes()
                .Where(type => EntityType().IsAssignableFrom(type) && type.IsClass)
                .Select(type => new { FullName = type.FullName, Name = type.Name })
                .ToArray();

            List<EntityContext> types = new List<EntityContext>();

            foreach (var item in implementingClasses)
            {
                if (item == null || string.IsNullOrEmpty(item.FullName))
                    throw new Exception(string.Format("Class Value Null Found"));

                Type? entityType = Type.GetType(item.FullName);

                if (entityType == null)
                    throw new Exception(string.Format("{0} Cannot Converted to Entity", item));

                var model = new EntityContext()
                {
                    Entity = item.Name,
                    Type = entityType,
                };

                types.Add(model);
            }

            ConfigureEntities(modelBuilder, types);
        }
        private void ConfigureEntities(ModelBuilder modelBuilder, List<EntityContext> types)
        {
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i] == null || types[i].Type == null)
                {
                    throw new Exception("Entity Type Creation Error");
                }

                else if (string.IsNullOrEmpty(types[i].Entity))
                {
                    throw new Exception("Entity Type is Null");
                }

                modelBuilder.Entity(types[i].Type).ToTable(types[i].Entity);
            }

            EntityBinder(modelBuilder);
        }
        public virtual void EntityBinder(ModelBuilder modelBuilder)
        {

        }

    }
}
