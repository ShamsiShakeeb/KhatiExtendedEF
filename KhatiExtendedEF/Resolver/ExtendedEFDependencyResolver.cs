using KhatiExtendedEF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KhatiExtendedEF.Resolver
{
    public static class ExtendedEFDependencyResolver
    {
        private static int setRepo = 0;
        public static void ExtendedEF<T>(this IServiceCollection service, T? context = null, ServiceLifetime? lifeTime = null) where T : DbContext
        {
            if (lifeTime == null)
                service.AddDbContext<T>();
            else
                service.AddDbContext<T>(lifeTime.Value);

            if (setRepo == 0)
            {
                service.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                setRepo = setRepo + 1;
            }
        }
    }
}
