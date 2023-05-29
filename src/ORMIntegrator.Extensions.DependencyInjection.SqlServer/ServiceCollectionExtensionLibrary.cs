using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Service.Extensions.DependencyInjection.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionLibrary {
    public static IServiceCollection AddSqlManager<TDbContext>(this IServiceCollection serviceDescriptors, Func<string, bool, TDbContext> dbContextFactoryMethod, IConfiguration configuration, string dbContextName, IDefaultEnvironmentAccessor defaultEnvironment, bool isHandleAsSingleton = false) where TDbContext : DbContext, new() {

        var databaseConfig = configuration.GetSection(DatabaseConfig.GetDefaultSection(dbContextName)).GetAvailable<DatabaseConfig>();

        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder {
            ConnectionString = databaseConfig.ConnectionString,
            ApplicationName = databaseConfig.ApplicationName
        };

        if (databaseConfig.MinPoolSize is > 0) {
            sqlConnectionStringBuilder.MinPoolSize = databaseConfig.MinPoolSize.Value;
        }

        if (databaseConfig.MaxPoolSize is > 0) {
            sqlConnectionStringBuilder.MaxPoolSize = databaseConfig.MaxPoolSize.Value;
        }
        
        return serviceDescriptors.AddSqlManager(dbContextFactoryMethod, sqlConnectionStringBuilder.ConnectionString, defaultEnvironment, isHandleAsSingleton);
    }
}
