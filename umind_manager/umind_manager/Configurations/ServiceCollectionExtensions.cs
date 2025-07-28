using umind_manager.Core.Contracts.Repositories;
using umind_manager.Repositories;
using Umind_Manager.Repositories.Dependencies;

namespace umind_manager.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataDependencies(this IServiceCollection services)
        {
            services.AddScoped<DataDependency>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IMissionRepository, MissionRepository>();
        }
    }
}
