using System.Runtime.CompilerServices;

namespace ORMIntegrator;

public class SqlManager<TDbContext> : IAsyncDisposable where TDbContext : DbContext {
    public TDbContext DbContext { get; set; }

    public DbConnection DbConnection { get; }

    DbTransaction? DbTransaction { get; set; }

    public bool IsOpenedConnection => DbConnection.State == ConnectionState.Open;

    bool ConsolelogIsRequired { get; }

    internal static class SqlLogger {
        internal const string DefaultCallerMethodName = "N/A";

        internal static void LoggingIf(bool predicate, string sql, object? prameters = null, string callerMethodName = DefaultCallerMethodName, [CallerMemberName] string sqlManagerMethodName = DefaultCallerMethodName) {
            if (predicate is false) {
                return;
            }

            Console.WriteLine($"{nameof(callerMethodName)}:{callerMethodName}");
            Console.WriteLine($"{nameof(sqlManagerMethodName)}:{sqlManagerMethodName}");

            static void LoggingParametersLocal(object? prameters) {
                if (prameters is null) {
                    return;
                }

                Console.WriteLine($"{nameof(prameters)}:");

                if (prameters is not DynamicParameters dynamicParameters) {
                    Console.WriteLine($"{prameters}");
                    return;
                }

                foreach (var ParameterName in dynamicParameters.ParameterNames) {
                    Console.WriteLine($"{ParameterName}:{dynamicParameters.Get<dynamic>(ParameterName).ToString()}");
                }
            }

            LoggingParametersLocal(prameters);

            Console.WriteLine($"{nameof(sql)}:{sql}");
        }
    }

    public SqlManager(Func<string, bool, TDbContext> dbContextFactoryMethod, string connectionString, bool consolelogIsRequired = false) {
        DbContext = dbContextFactoryMethod(connectionString, consolelogIsRequired);

        DbConnection = DbContext.Database.GetDbConnection();

        ConsolelogIsRequired = consolelogIsRequired;

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

    public IEnumerable<TResult> Select<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public IEnumerable<TResult> Select<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public IEnumerable<TResult> Select<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public IEnumerable<TResult> Select<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);
    }

    public IEnumerable<TResult> Select<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);
    }

    public Task<IEnumerable<TResult>> SelectAsync<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QueryAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<IEnumerable<TResult>> SelectAsync<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<IEnumerable<TResult>> SelectAsync<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<IEnumerable<TResult>> SelectAsync<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryAsync(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);
    }

    public Task<IEnumerable<TResult>> SelectAsync<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryAsync(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn);
    }

    public List<TResult> SelectAsList<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(query, transaction: GetDbTransactionIfIsBegun()).AsList();
    }

    public List<TResult> SelectAsList<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun()).AsList();
    }

    public List<TResult> SelectAsList<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.Query<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun()).AsList();
    }

    public List<TResult> SelectAsList<TResult, TInclude1>(string query, Func<TResult, TInclude1, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: includeFunc, callerMethodName: callerMethodName);

        return DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn).AsList();
    }

    public List<TResult> SelectAsList<TResult, TInclude1, TInclude2>(string query, Func<TResult, TInclude1, TInclude2, TResult> includeFunc, object prameters, string splitOn = "Id", [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Query(query, includeFunc, prameters, transaction: GetDbTransactionIfIsBegun(), true, splitOn).AsList();
    }

    public TResult SelectFirst<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QueryFirst<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectFirst<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirst<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectFirst<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirst<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstAsync<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstAsync<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstAsync<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectFirstOrDefault<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefault<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectFirstOrDefault<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefault<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectFirstOrDefault<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefault<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefaultAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefaultAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectFirstOrDefaultAsync<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QueryFirstOrDefaultAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingle<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QuerySingle<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingle<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingle<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingle<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingle<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleAsync<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleAsync<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleAsync<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingleOrDefault<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefault<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingleOrDefault<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefault<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public TResult SelectSingleOrDefault<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefault<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefaultAsync<TResult>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefaultAsync<TResult>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<TResult> SelectSingleOrDefaultAsync<TResult>((string query, object prameters) queryAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: queryAndParameters.query, prameters: queryAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.QuerySingleOrDefaultAsync<TResult>(queryAndParameters.query, queryAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }
    public BuiltInType GetValue<BuiltInType>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.ExecuteScalar<BuiltInType>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public BuiltInType GetValue<BuiltInType>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.ExecuteScalar<BuiltInType>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<BuiltInType> GetValueAsync<BuiltInType>(string query, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, callerMethodName: callerMethodName);

        return DbConnection.ExecuteScalarAsync<BuiltInType>(query, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<BuiltInType> GetValueAsync<BuiltInType>(string query, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: query, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.ExecuteScalarAsync<BuiltInType>(query, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public int Execute(string command, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: command, callerMethodName: callerMethodName);

        return DbConnection.Execute(command, transaction: GetDbTransactionIfIsBegun());
    }

    public int Execute(string command, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: command, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.Execute(command, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<int> ExecuteAsync(string command, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: command, callerMethodName: callerMethodName);

        return DbConnection.ExecuteAsync(command, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<int> ExecuteAsync(string command, object prameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: command, prameters: prameters, callerMethodName: callerMethodName);

        return DbConnection.ExecuteAsync(command, prameters, transaction: GetDbTransactionIfIsBegun());
    }

    public Task<int> ExecuteAsync((string command, object prameters) commandAndParameters, [CallerMemberName] string callerMethodName = SqlLogger.DefaultCallerMethodName) {
        SqlLogger.LoggingIf(predicate: ConsolelogIsRequired, sql: commandAndParameters.command, prameters: commandAndParameters.prameters, callerMethodName: callerMethodName);

        return DbConnection.ExecuteAsync(commandAndParameters.command, commandAndParameters.prameters, transaction: GetDbTransactionIfIsBegun());
    }

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
