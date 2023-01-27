using System.Data;
using System.Reflection;

namespace ORMIntegrator;

public class BulkInsertDataReader<T> : IDataReader {
    private IEnumerator<T> DataEnumerator { get; }

    public BulkInsertDataReader(IEnumerator<T> enumerator)
        => DataEnumerator = enumerator;

    public BulkInsertDataReader(IEnumerable<T> data)
        : this(data.GetEnumerator()) { }

    public int FieldCount
        => PropertyInfoCache<T>.Instances.Length;

    int IDataReader.Depth => throw new NotImplementedException();

    bool IDataReader.IsClosed => throw new NotImplementedException();

    int IDataReader.RecordsAffected => throw new NotImplementedException();

    int IDataRecord.FieldCount => throw new NotImplementedException();

    object IDataRecord.this[string name] => throw new NotImplementedException();

    object IDataRecord.this[int i] => throw new NotImplementedException();

    public void Dispose()
        => DataEnumerator.Dispose();

    public object GetValue(int i) {
        // 対象テーブルの列とプロパティの個数 / 並び順が一致している前提
        var prop = PropertyInfoCache<T>.Instances[i];
        var obj = DataEnumerator.Current;
        return prop.GetValue(obj)!;
    }

    public bool Read()
        => DataEnumerator.MoveNext();

    void IDataReader.Close() => throw new NotImplementedException();
    DataTable? IDataReader.GetSchemaTable() => throw new NotImplementedException();
    bool IDataReader.NextResult() => throw new NotImplementedException();
    bool IDataReader.Read() => throw new NotImplementedException();
    bool IDataRecord.GetBoolean(int i) => throw new NotImplementedException();
    byte IDataRecord.GetByte(int i) => throw new NotImplementedException();
    long IDataRecord.GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
    char IDataRecord.GetChar(int i) => throw new NotImplementedException();
    long IDataRecord.GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
    IDataReader IDataRecord.GetData(int i) => throw new NotImplementedException();
    string IDataRecord.GetDataTypeName(int i) => throw new NotImplementedException();
    DateTime IDataRecord.GetDateTime(int i) => throw new NotImplementedException();
    decimal IDataRecord.GetDecimal(int i) => throw new NotImplementedException();
    double IDataRecord.GetDouble(int i) => throw new NotImplementedException();
    Type IDataRecord.GetFieldType(int i) => throw new NotImplementedException();
    float IDataRecord.GetFloat(int i) => throw new NotImplementedException();
    Guid IDataRecord.GetGuid(int i) => throw new NotImplementedException();
    short IDataRecord.GetInt16(int i) => throw new NotImplementedException();
    int IDataRecord.GetInt32(int i) => throw new NotImplementedException();
    long IDataRecord.GetInt64(int i) => throw new NotImplementedException();
    string IDataRecord.GetName(int i) => throw new NotImplementedException();
    int IDataRecord.GetOrdinal(string name) => throw new NotImplementedException();
    string IDataRecord.GetString(int i) => throw new NotImplementedException();
    object IDataRecord.GetValue(int i) => throw new NotImplementedException();
    int IDataRecord.GetValues(object[] values) => throw new NotImplementedException();
    bool IDataRecord.IsDBNull(int i) => throw new NotImplementedException();
    void IDisposable.Dispose() => throw new NotImplementedException();

    static class PropertyInfoCache<TU> {
        internal static PropertyInfo[] Instances { get; }

        static PropertyInfoCache()
            => Instances = typeof(TU).GetProperties(BindingFlags.Instance | BindingFlags.Public);
    }
}
