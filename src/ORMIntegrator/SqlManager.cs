namespace ORMIntegrator;

public class SqlManager<TDbContext> : IAsyncDisposable where TDbContext : DbContext {
    public TDbContext DbContext { get; set; }

    public DbConnection DbConnection { get; }

    DbTransaction? DbTransaction { get; set; }

    public bool IsOpenedConnection => DbConnection.State == ConnectionState.Open;

    public SqlManager(Func<string, TDbContext> dbContextFactoryMethod, string connectionString) {
        DbContext = dbContextFactoryMethod(connectionString);

        DbConnection = DbContext.Database.GetDbConnection();

        OpenConnection();
    }

    public void OpenConnection() {
        if (IsOpenedConnection) {
            return;
        }

        DbConnection.Open();
    }

    public async ValueTask BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
        DbTransaction = await DbConnection.BeginTransactionAsync(isolationLevel);
        await DbContext.Database.UseTransactionAsync(DbTransaction);
    }

    public async ValueTask<ScopedTransaction> BeginScopedTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
        DbTransaction = await DbConnection.BeginTransactionAsync(isolationLevel);
        await DbContext.Database.UseTransactionAsync(DbTransaction);

        return new ScopedTransaction(DbTransaction);
    }

    public async ValueTask CommitAsync() {
        if (DbTransaction.IsInvalid()) {
            return;
        }

        await DbTransaction.CommitAsync();
    }

    public async ValueTask RollbackIfUncommitedAsync() {
        if (DbTransaction.IsInvalid()) {
            return;
        }

        await DbTransaction.RollbackAsync();
    }

    DbTransaction? GetDbTransactionIfIsBegun() =>
        DbTransaction.IsInvalid() ? null : DbTransaction;

    void DisposeTransaction() {
        if (DbTransaction.IsInvalid()) {
            return;
        }

        DbTransaction.Rollback();
        DbTransaction.Dispose();
    }

    async ValueTask DisposeTransactionAsync() {
        if (DbTransaction.IsInvalid()) {
            return;
        }

        await DbTransaction.RollbackAsync();
        await DbTransaction.DisposeAsync();
    }

    public void CloseConnection() {
        if (!IsOpenedConnection) {
            return;
        }

        DbConnection.Close();
        DbContext.Dispose();
    }

    public async ValueTask CloseConnectionAsync() {
        if (!IsOpenedConnection) {
            return;
        }

        await DbConnection.CloseAsync();
        await DbContext.DisposeAsync();
    }

    public IEnumerable<TResult> Select<TResult>(string query) =>
        DbConnection.Query<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public IEnumerable<TResult> Select<TResult>(string query, object prameters) =>
        DbConnection.Query<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public IEnumerable<TResult> Select<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.Query<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public IEnumerable<TResult> Select<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);

    public IEnumerable<TResult> Select<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);

    public Task<IEnumerable<TResult>> SelectAsync<TResult>(string query) =>
        DbConnection.QueryAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<IEnumerable<TResult>> SelectAsync<TResult>(string query, object prameters) =>
        DbConnection.QueryAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<IEnumerable<TResult>> SelectAsync<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QueryAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<IEnumerable<TResult>> SelectAsync<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.QueryAsync(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);

    public Task<IEnumerable<TResult>> SelectAsync<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.QueryAsync(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);

    public List<TResult> SelectAsList<TResult>(string query) =>
        DbConnection.Query<TResult>(query, transaction: GetDbTransactionIfIsBegun()).AsList();

    public List<TResult> SelectAsList<TResult>(string query, object prameters) =>
        DbConnection.Query<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun()).AsList();

    public List<TResult> SelectAsList<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.Query<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun()).AsList();

    public List<TResult> SelectAsList<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn).AsList();

    public List<TResult> SelectAsList<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id") =>
        DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn).AsList();

    public TResult SelectFirst<TResult>(string query) =>
        DbConnection.QueryFirst<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectFirst<TResult>(string query, object prameters) =>
        DbConnection.QueryFirst<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectFirst<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QueryFirst<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstAsync<TResult>(string query) =>
        DbConnection.QueryFirstAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstAsync<TResult>(string query, object prameters) =>
        DbConnection.QueryFirstAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstAsync<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QueryFirstAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectFirstOrDefault<TResult>(string query) =>
        DbConnection.QueryFirstOrDefault<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectFirstOrDefault<TResult>(string query, object prameters) =>
        DbConnection.QueryFirstOrDefault<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectFirstOrDefault<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QueryFirstOrDefault<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>(string query) =>
        DbConnection.QueryFirstOrDefaultAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>(string query, object prameters) =>
        DbConnection.QueryFirstOrDefaultAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QueryFirstOrDefaultAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingle<TResult>(string query) =>
        DbConnection.QuerySingle<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingle<TResult>(string query, object prameters) =>
        DbConnection.QuerySingle<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingle<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QuerySingle<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleAsync<TResult>(string query) =>
        DbConnection.QuerySingleAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleAsync<TResult>(string query, object prameters) =>
        DbConnection.QuerySingleAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleAsync<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QuerySingleAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingleOrDefault<TResult>(string query) =>
        DbConnection.QuerySingleOrDefault<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingleOrDefault<TResult>(string query, object prameters) =>
        DbConnection.QuerySingleOrDefault<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public TResult SelectSingleOrDefault<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QuerySingleOrDefault<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>(string query) =>
        DbConnection.QuerySingleOrDefaultAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>(string query, object prameters) =>
        DbConnection.QuerySingleOrDefaultAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>((string query, object prameters) queryAndParameters) =>
        DbConnection.QuerySingleOrDefaultAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public BuiltInType GetValue<BuiltInType>(string query) =>
        DbConnection.ExecuteScalar<BuiltInType>(query, transaction: GetDbTransactionIfIsBegun());

    public BuiltInType GetValue<BuiltInType>(string query, object prameters) =>
        DbConnection.ExecuteScalar<BuiltInType>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<BuiltInType> GetValueAsync<BuiltInType>(string query) =>
        DbConnection.ExecuteScalarAsync<BuiltInType>(query, transaction: GetDbTransactionIfIsBegun());

    public Task<BuiltInType> GetValueAsync<BuiltInType>(string query, object prameters) =>
        DbConnection.ExecuteScalarAsync<BuiltInType>(query, prameters, transaction: GetDbTransactionIfIsBegun());

    public int Execute(string command) =>
        DbConnection.Execute(command, transaction: GetDbTransactionIfIsBegun());

    public int Execute(string command, object prameters) =>
        DbConnection.Execute(command, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<int> ExecuteAsync(string command) =>
        DbConnection.ExecuteAsync(command, transaction: GetDbTransactionIfIsBegun());

    public Task<int> ExecuteAsync(string command, object prameters) =>
        DbConnection.ExecuteAsync(command, prameters, transaction: GetDbTransactionIfIsBegun());

    public Task<int> ExecuteAsync((string command, object prameters) commandAndParameters) =>
        DbConnection.ExecuteAsync(commandAndParameters.command, commandAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());

    public void Dispose() {
        DisposeTransaction();
        CloseConnection();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await DisposeTransactionAsync();
        await CloseConnectionAsync();
        GC.SuppressFinalize(this);
    }
}
