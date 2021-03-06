using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ORMIntegrator;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionLibrary {
    public static IServiceCollection AddSqlManager<TDbContext>(this IServiceCollection serviceDescriptors, Func<string, bool, TDbContext> dbContextFactoryMethod, string connectionString) where TDbContext : DbContext, new() {
        serviceDescriptors.AddScoped(_ =>
            new SqlManager<TDbContext>(
               dbContextFactoryMethod,
               connectionString,
               DefaultEnvironment.IsDevelopment()
            ));

        serviceDescriptors.AddScoped(
            serviceProvider => new ScopedTransactionBuilder<TDbContext>(serviceProvider.GetRequiredService<SqlManager<TDbContext>>()));

        return serviceDescriptors;
    }
}
