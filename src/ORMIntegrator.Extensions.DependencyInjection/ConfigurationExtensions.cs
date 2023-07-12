using ORMIntegrator;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;
public static class ConfigurationExtensions {
    public static SqlManagerOptions? GetSqlManagerOptions(this IConfiguration configuration) =>
        configuration.GetSection(nameof(SqlManagerOptions)).Get<SqlManagerOptions>();

    public static bool GetDevLoggingIsForceDisabled(this IConfiguration configuration) =>
        (configuration.GetSqlManagerOptions()?.DevLoggingIsForceDisabled ?? false);
}
