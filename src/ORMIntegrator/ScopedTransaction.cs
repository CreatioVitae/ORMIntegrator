namespace ORMIntegrator;

public class ScopedTransaction : IAsyncDisposable {
    public DbTransaction? DbTransaction { get; set; } = null;

    bool ScopeIsComplete { get; set; } = false;

    public ScopedTransaction(DbTransaction? dbTransaction) =>
        DbTransaction = dbTransaction;

    public void Complete() {
        if (DbTransaction.IsInvalid()) {
            throw new ObjectDisposedException(nameof(DbTransaction));
        }

        if (ScopeIsComplete) {
            throw new InvalidOperationException($"Already marked as completed");
        }

        ScopeIsComplete = true;
    }

    public async ValueTask DisposeAsync() {
        if (DbTransaction.IsInvalid()) {
            return;
        }

        if (ScopeIsComplete) {
            await DbTransaction.CommitAsync();
            return;
        }

        await DbTransaction.RollbackAsync();
    }
}
