namespace ORMIntegrator;

public class ScopedTransactionBuilder<TDbContext>(SqlManager<TDbContext> sqlManager) where TDbContext : DbContext {
    SqlManager<TDbContext> SqlManager { get; } = sqlManager;

    public async ValueTask<ScopedTransaction> BeginScopedTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) =>
        await SqlManager.BeginScopedTransactionAsync(isolationLevel);

    public async ValueTask ExecutePooledCommandsAsync(CancellationToken cancellationToken = default) =>
        await SqlManager.DbContext.SaveChangesAsync(cancellationToken);
}
