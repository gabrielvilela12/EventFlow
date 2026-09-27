using EventFlow.Application.Abstractions;
using EventFlow.Infrastructure;
using Hangfire;
using Hangfire.Storage.SQLite;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services
    .AddEventFlowPersistence(builder.Configuration)
    .AddEventFlowMessaging(builder.Configuration)
    .AddEventFlowUseCases();

var hangfirePath = ResolveFromSolutionRoot(builder.Configuration["Hangfire:Path"] ?? "data/eventflow-hangfire.db");
Directory.CreateDirectory(Path.GetDirectoryName(hangfirePath)!);
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })
    .UseSQLiteStorage(hangfirePath, new SQLiteStorageOptions
    {
        JournalMode = SQLiteStorageOptions.JournalModes.WAL,
        AutoVacuumSelected = SQLiteStorageOptions.AutoVacuum.INCREMENTAL
    }));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.Queues = ["eventflow"];
});

var app = builder.Build();
await app.Services.InitializeEventFlowDatabaseAsync();

app.UseHangfireDashboard("/hangfire");
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/hangfire"));

RecurringJob.AddOrUpdate<IOutboxDispatcher>("dispatch-outbox", "eventflow",
    service => service.DispatchAsync(CancellationToken.None), "* * * * *", new RecurringJobOptions());
RecurringJob.AddOrUpdate<IExpirationScheduler>("expire-reservations", "eventflow",
    service => service.EnqueueExpiredAsync(CancellationToken.None), "* * * * *", new RecurringJobOptions());

app.Run();

static string ResolveFromSolutionRoot(string configuredPath)
{
    if (Path.IsPathRooted(configuredPath)) return configuredPath;
    var cursor = new DirectoryInfo(AppContext.BaseDirectory);
    while (cursor is not null && !File.Exists(Path.Combine(cursor.FullName, "EventFlow.slnx"))) cursor = cursor.Parent;
    return Path.GetFullPath(Path.Combine(cursor?.FullName ?? Environment.CurrentDirectory, configuredPath));
}
