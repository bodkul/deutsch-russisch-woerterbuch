using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Woerterbuch.Core;

public static class WoerterbuchDb
{
    public const string ConnectionName = "Woerterbuch";

    /// <summary>The <c>data/</c> folder at the repository root (where Woerterbuch.slnx lives).</summary>
    public static string DataDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Woerterbuch.slnx")))
            dir = dir.Parent;

        var root = dir?.FullName ?? Directory.GetCurrentDirectory();
        var data = Path.Combine(root, "data");
        Directory.CreateDirectory(data);
        return data;
    }

    /// <summary>Folder for auto-backups of personal words: <c>data/backups</c>.</summary>
    public static string BackupDirectory() => Path.Combine(DataDirectory(), "backups");

    public static string DefaultConnectionString() =>
        $"Data Source={Path.Combine(DataDirectory(), "woerterbuch.db")}";

    /// <summary>
    /// Registers the DbContext. The connection string can be overridden via
    /// <c>ConnectionStrings:Woerterbuch</c>, otherwise <c>data/woerterbuch.db</c> is used.
    /// </summary>
    public static IServiceCollection AddWoerterbuchDb(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString(ConnectionName) ?? DefaultConnectionString();
        services.AddDbContext<WoerterbuchDbContext>(o => o.UseSqlite(cs));
        services.AddScoped<Search.SearchService>();
        return services;
    }
}
