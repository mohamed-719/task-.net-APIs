using BD_Assignment.Repositories.Interfaces;
using BD_Assignment.Repositories;

namespace BD_Assignment.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddSingleton<ICountryBlockRepository, CountryBlockRepository>();
        services.AddSingleton<IBlockAttemptLogRepository, BlockAttemptLogRepository>();
        return services;
    }
}
