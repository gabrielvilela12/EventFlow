using EventFlow.Application.Abstractions;
using EventFlow.Domain;
using EventFlow.Infrastructure.Notifications;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure;

public static class PersistenceRegistration
{
    public static IServiceCollection AddEventFlowPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dataPath = ResolveDataPath(configuration["Database:Path"] ?? "data/eventflow-portfolio.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);
        services.AddDbContext<EventFlowDbContext>(options => options.UseSqlite($"Data Source={dataPath}"));
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventFlowRepository, EventFlowRepository>();
        services.AddScoped<IJobProcessingStore, JobProcessingStore>();
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<INotificationSender, DemoNotificationSender>();
        return services;
    }

    public static async Task InitializeEventFlowDatabaseAsync(this IServiceProvider services, bool seedDemo = true)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EventFlowDbContext>();
        await db.Database.EnsureCreatedAsync();
        if (seedDemo && !await db.Events.AnyAsync())
        {
            db.Events.Add(EventDefinition.Create("EventFlow Tech Conference", "Demo Convention Center",
                DateTimeOffset.UtcNow.AddDays(30), 250));
            await db.SaveChangesAsync();
        }
    }

    private static string ResolveDataPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath)) return configuredPath;
        var cursor = new DirectoryInfo(AppContext.BaseDirectory);
        while (cursor is not null && !File.Exists(Path.Combine(cursor.FullName, "EventFlow.slnx"))) cursor = cursor.Parent;
        return Path.GetFullPath(Path.Combine(cursor?.FullName ?? Environment.CurrentDirectory, configuredPath));
    }
}
