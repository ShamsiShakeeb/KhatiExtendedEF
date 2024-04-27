using KhatiExtendedEF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KhatiExtendedEF.Resolver
{
    public static class ExtendedEFDependencyResolver
    {
        private static int setRepo = 0;
        public static void ExtendedEF<T>(this IServiceCollection service) where T : DbContext
        {
            service.AddDbContext<T>(ServiceLifetime.Scoped);

            if (setRepo == 0)
            {
                service.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                setRepo = setRepo + 1;
            }
        }
    }
}
