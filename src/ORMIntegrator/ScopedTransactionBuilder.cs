namespace ORMIntegrator;

public class ScopedTransactionBuilder<TDbContext> where TDbContext : DbContext　　　　　 {
    SqlManager<TDbContext> SqlManager { get; }

    public ScopedTransactionBuilder(SqlManager<TDbContext> sqlManager) =>
        SqlManager = sqlManager;

    public async ValueTask<ScopedTransaction> BeginScopedTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) =>
        await SqlManager.BeginScopedTransactionAsync();

    public async ValueTask ExecutePooledCommandsAsync(CancellationToken cancellationToken = default) =>
        await SqlManager.DbContext.SaveChangesAsync(cancellationToken);
}
