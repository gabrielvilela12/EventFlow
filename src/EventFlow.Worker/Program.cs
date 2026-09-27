using EventFlow.Infrastructure;
using EventFlow.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddEventFlowPersistence(builder.Configuration)
    .AddEventFlowMessaging(builder.Configuration)
    .AddEventFlowUseCases();
builder.Services.AddHostedService<QueueWorker>();

var host = builder.Build();
await host.Services.InitializeEventFlowDatabaseAsync();
await host.RunAsync();
