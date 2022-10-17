using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ORMIntegrator;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionLibrary {
    public static IServiceCollection AddSqlManager<TDbContext>(this IServiceCollection serviceDescriptors, Func<string, bool, TDbContext> dbContextFactoryMethod, string connectionString, IDefaultEnvironmentAccessor defaultEnvironment, bool isHandleAsSingleton = false) where TDbContext : DbContext, new() {
        if (isHandleAsSingleton) {
            serviceDescriptors.AddSingleton(_ =>
                new SqlManager<TDbContext>(
                    dbContextFactoryMethod,
                    connectionString,
                    defaultEnvironment.IsDevelopment()
                ));

            serviceDescriptors.AddSingleton(
                serviceProvider => new ScopedTransactionBuilder<TDbContext>(serviceProvider.GetRequiredService<SqlManager<TDbContext>>()));

            return serviceDescriptors;
        }

        serviceDescriptors.AddScoped(_ =>
            new SqlManager<TDbContext>(
                dbContextFactoryMethod,
                connectionString,
                defaultEnvironment.IsDevelopment()
            ));

        serviceDescriptors.AddScoped(
            serviceProvider => new ScopedTransactionBuilder<TDbContext>(serviceProvider.GetRequiredService<SqlManager<TDbContext>>()));

        return serviceDescriptors;
    }
}
