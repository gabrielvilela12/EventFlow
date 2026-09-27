using EventFlow.Application.Abstractions;
using EventFlow.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure;

public static class MessagingRegistration
{
    public static IServiceCollection AddEventFlowMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMq");
        var options = new RabbitMqOptions
        {
            Enabled = GetBool(section["Enabled"], true),
            HostName = section["HostName"] ?? "localhost",
            Port = GetInt(section["Port"], 5672),
            VirtualHost = section["VirtualHost"] ?? "/",
            UserName = section["UserName"] ?? "eventflow",
            Password = section["Password"] ?? "eventflow-local",
            MainExchange = section["MainExchange"] ?? "eventflow.jobs",
            RetryExchange = section["RetryExchange"] ?? "eventflow.jobs.retry",
            DeadExchange = section["DeadExchange"] ?? "eventflow.jobs.dead",
            RetryDelayMilliseconds = GetInt(section["RetryDelayMilliseconds"], 10_000),
            MaxAttempts = GetInt(section["MaxAttempts"], 3),
            PrefetchCount = (ushort)GetInt(section["PrefetchCount"], 4)
        };
        services.AddSingleton(options);
        services.AddSingleton<RabbitConnection>();
        services.AddSingleton<IJobPublisher, RabbitJobPublisher>();
        return services;
    }

    private static int GetInt(string? value, int fallback) => int.TryParse(value, out var parsed) ? parsed : fallback;
    private static bool GetBool(string? value, bool fallback) => bool.TryParse(value, out var parsed) ? parsed : fallback;
}
