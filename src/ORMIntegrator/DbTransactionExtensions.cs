using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace MicroORMWrapper {
    internal static class DbTransactionExtensions {
        internal static bool IsInvalid([NotNullWhen(false)] this  DbTransaction? transaction) =>
            transaction == null || transaction.Connection == null;
    }
}
