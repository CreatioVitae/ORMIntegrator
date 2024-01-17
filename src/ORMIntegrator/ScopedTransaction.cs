namespace ORMIntegrator;

public class ScopedTransaction(DbTransaction? dbTransaction) : IAsyncDisposable {
    public DbTransaction? DbTransaction { get; set; } = dbTransaction;

    bool ScopeIsComplete { get; set; } = false;

    public void Complete() {

        ObjectDisposedException.ThrowIf(DbTransaction.IsInvalid(), typeof(DbTransaction));

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
