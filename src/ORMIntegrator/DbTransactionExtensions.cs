using System.Diagnostics.CodeAnalysis;

namespace ORMIntegrator;

internal static class DbTransactionExtensions {
    internal static bool IsInvalid([NotNullWhen(false)] this DbTransaction? transaction) =>
        transaction?.Connection is null;
}
