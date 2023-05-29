using System.ComponentModel.DataAnnotations;

// ReSharper disable once CheckNamespace
namespace Service.Extensions.DependencyInjection.Options;
public record DatabaseConfig {
    public required string ConnectionString { get; init; }

    public required string ApplicationName { get; init; }

    [Range(0, int.MaxValue)]
    public int? MinPoolSize { get; init; }

    [Range(0, int.MaxValue)]
    public int? MaxPoolSize { get; init; }

    public static string GetDefaultSection(string dbContextName) =>
        $"{dbContextName.Remove("Context")}DatabaseConfig";
}
