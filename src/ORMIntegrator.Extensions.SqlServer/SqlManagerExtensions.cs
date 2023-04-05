using Microsoft.Data.SqlClient;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace ORMIntegrator;

public static class SqlManagerExtensions {
    public static SqlConnection GetSqlConnection<TDbContext>(this SqlManager<TDbContext> sqlManager) where TDbContext : DbContext =>
        (SqlConnection)sqlManager.DbConnection;

    internal static SqlTransaction GetSqlTransaction<TDbContext>(this SqlManager<TDbContext> sqlManager) where TDbContext : DbContext =>
        (SqlTransaction)sqlManager.GetDbTransaction();

    public static void SetDefaultColumnMappings(this SqlBulkCopy sqlBulkCopy, DataTable dataTable) {
        foreach (DataColumn column in dataTable.Columns) {
            sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }
    }

    static IEnumerable<(string propName, object? value)> CreateTuple<T>(PropertyInfo[] props, T item) =>
        props.Select(prop => (prop.Name, prop.GetValue(item, null)));

    public static DataTable ToDataTable<T>(this IList<T> collections, DbContext dbContext) {
        var entityType = dbContext.Model.FindEntityType(typeof(T));

        ArgumentNullException.ThrowIfNull(entityType);

        // new DataTable use Table Name with Schema name.
        var dataTable = new DataTable(entityType.GetSchemaQualifiedTableName());

        var edmProperties = entityType.GetProperties().ToArray();

        // add Column info 
        foreach (var property in edmProperties) {
            dataTable.Columns.Add(property.GetColumnName(), property.ClrType);
        }

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var item in collections) {

            var dr = dataTable.NewRow();

            var propDictionary = CreateTuple(props, item).ToDictionary(e => e.propName, e => e.value);

            foreach (var edmProperty in edmProperties) {
                dr[edmProperty.GetColumnName()] = propDictionary[edmProperty.Name];
            }

            dataTable.Rows.Add(dr);
        }

        return dataTable;
    }

    public static async ValueTask BulkInsertAsync<TDbContext, T>(this SqlManager<TDbContext> sqlManager, IList<T> entities) where TDbContext : DbContext {

        var dataTable = entities.ToDataTable(sqlManager.DbContext);

        using var bulkCopy = new SqlBulkCopy(sqlManager.GetSqlConnection(), SqlBulkCopyOptions.Default, sqlManager.GetSqlTransaction());

        bulkCopy.DestinationTableName = dataTable.TableName;

        bulkCopy.SetDefaultColumnMappings(dataTable);

        await bulkCopy.WriteToServerAsync(dataTable);
    }
}

