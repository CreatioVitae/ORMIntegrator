ORMIntegrator</br>
ORMIntegrator.Extensions.DependencyInjection
===

Getting Started(.NET 6 / ASP.NET Core)
---
Create DBContext.Partial Object
```csharp
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Kashilog.DbContexts;

public partial class KashilogContext : DbContext {

    readonly string _connectionString;

    public KashilogContext(string connectionString) =>
        _connectionString = connectionString;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer(_connectionString);
}

```

DI Settings(ServiceDescriptor Configuration)
---
```csharp
using Database.Kashilog.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Database.Kashilog;

public static class StartupExtensionLibrary {
    public static IServiceCollection AddSqlManagerFromKashilogDatabase(this IServiceCollection services, string connectionString) =>
        services.AddSqlManager(
            (connectionString) => new KashilogContext(connectionString),
            connectionString
        );
}
```

Constructor Injection
---
```csharp
using MicroORMWrapper;
using Service.Extensions.DependencyInjection.Markers;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Kashilog.DomainObjects.Kashi;
using WebApi.Kashilog.Repositories.DatabaseConnections;
using WebApi.Kashilog.Repositories.Kashi.Products.Sqls;

namespace Repository.Kashilog.Kashi.Products {
    public class ProductRepository : IRepository {

        SqlManager<KashilogContext> KashilogSqlManager { get; }

        public ProductRepository(SqlManager<KashilogContext> kashilogSqlManager) {
            KashilogSqlManager = kashilogSqlManager;
        }

        // ...
    }
}
```

Use TransactionBuilder
---

using Database.Kashilog.DbContexts;
using DomainObject.Kashilog.Configurations;
using Microsoft.Extensions.Options;
using Repository.Kashilog.Kashi.Products;

namespace Service.Kashilog.Kashi.Products;

public class DummyService : IService {
    ProductRepository ProductRepository { get; }

    ScopedTransactionBuilder<KashilogContext> TransactionBuilder { get; }

    RequestContext RequestContext { get; }

    public DummyService(ProductRepository productRepository, ScopedTransactionBuilder<KashilogContext> transactionBuilder, RequestContext requestContext) =>
       (ProductRepository, TransactionBuilder, RequestContext) = (productRepository, transactionBuilder, requestContext);

    public async ValueTask UpdateDummyAsync() {
        await using var scopedTransaction = await TransactionBuilder.BeginScopedTransactionAsync();

        // Execute Commands Use Repositories and SqlManager<TDbContext>...

        // Savechanges Trigger...Available without injection of SQLManager...
        await BbsScopedTransactionBuilder.ExecutePooledCommandsAsync();

        //Complete Mark...a.k.a. Commitable Mark...
        scopedTransaction.Complete();
    }
}
