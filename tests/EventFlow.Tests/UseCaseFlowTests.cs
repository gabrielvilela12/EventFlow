using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;
using EventFlow.Application.UseCases.Events.CreateEvent;
using EventFlow.Application.UseCases.Reservations.CreateReservation;
using EventFlow.Domain;
using EventFlow.Infrastructure;
using EventFlow.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventFlow.Tests;

public sealed class UseCaseFlowTests
{
    [Fact]
    public async Task Creating_reservation_persists_business_state_and_outbox_atomically()
    {
        await WithServicesAsync(async services =>
        {
            var eventItem = await CreateEventAsync(services);
            var useCase = services.GetRequiredService<IUseCase<CreateReservationRequest, CreateReservationResponse>>();
            var created = await useCase.ExecuteAsync(
                new CreateReservationRequest(eventItem.Id, "dev@example.com", 2), CancellationToken.None);

            var db = services.GetRequiredService<EventFlowDbContext>();
            var reservation = await db.Reservations.SingleAsync();
            var outbox = await db.OutboxMessages.SingleAsync();
            var message = JsonSerializer.Deserialize<JobEnvelope>(outbox.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.Equal(ReservationStatus.Pending, created.Reservation.Status);
            Assert.Equal(reservation.Id, outbox.ReservationId);
            Assert.Null(outbox.PublishedAtUtc);
            Assert.Equal(JobTypes.ProcessReservation, message?.JobType);
            Assert.Equal(reservation.Id, message?.ReservationId);
        });
    }

    [Fact]
    public async Task Dispatcher_publishes_once_and_worker_use_cases_advance_reservation()
    {
        var publisher = new RecordingPublisher();
        await WithServicesAsync(async services =>
        {
            var eventItem = await CreateEventAsync(services);
            var create = services.GetRequiredService<IUseCase<CreateReservationRequest, CreateReservationResponse>>();
            var created = await create.ExecuteAsync(
                new CreateReservationRequest(eventItem.Id, "dev@example.com", 1), CancellationToken.None);
            var dispatcher = services.GetRequiredService<IOutboxDispatcher>();
            Assert.Equal(1, await dispatcher.DispatchAsync());
            Assert.Equal(0, await dispatcher.DispatchAsync());
            Assert.Single(publisher.Messages);

            var handlers = services.GetRequiredService<IEnumerable<IJobHandler>>().ToDictionary(x => x.JobType);
            await handlers[JobTypes.ProcessReservation].HandleAsync(publisher.Messages[0], CancellationToken.None);
            Assert.Equal(1, await dispatcher.DispatchAsync());
            await handlers[JobTypes.GenerateTicket].HandleAsync(publisher.Messages[1], CancellationToken.None);
            Assert.Equal(1, await dispatcher.DispatchAsync());
            await handlers[JobTypes.SendNotification].HandleAsync(publisher.Messages[2], CancellationToken.None);

            var db = services.GetRequiredService<EventFlowDbContext>();
            var reservation = await db.Reservations.SingleAsync(x => x.Id == created.Reservation.Id);
            Assert.Equal(ReservationStatus.Notified, reservation.Status);
            Assert.NotNull(reservation.TicketCode);
            Assert.Equal(3, await db.JobExecutions.CountAsync());

            await handlers[JobTypes.ProcessReservation].HandleAsync(publisher.Messages[0], CancellationToken.None);
            Assert.Equal(3, await db.OutboxMessages.CountAsync());
        }, publisher);
    }

    private static async Task<EventFlow.Application.Common.EventView> CreateEventAsync(IServiceProvider services)
    {
        var useCase = services.GetRequiredService<IUseCase<CreateEventRequest, CreateEventResponse>>();
        var result = await useCase.ExecuteAsync(new CreateEventRequest(
            "Portfolio event", "Demo hall", DateTimeOffset.UtcNow.AddDays(2), 20), CancellationToken.None);
        return result.Event;
    }

    private static async Task WithServicesAsync(Func<IServiceProvider, Task> action, IJobPublisher? publisher = null)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"eventflow-usecase-{Guid.NewGuid():N}.db");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Path"] = dbPath
        }).Build();
        var registrations = new ServiceCollection();
        registrations.AddLogging();
        registrations.AddEventFlowPersistence(configuration).AddEventFlowUseCases();
        if (publisher is not null) registrations.AddSingleton(publisher);

        try
        {
            await using var provider = registrations.BuildServiceProvider();
            await provider.InitializeEventFlowDatabaseAsync(seedDemo: false);
            await using var scope = provider.CreateAsyncScope();
            await action(scope.ServiceProvider);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    private sealed class RecordingPublisher : IJobPublisher
    {
        public List<JobEnvelope> Messages { get; } = [];

        public Task PublishMainAsync(JobEnvelope message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task PublishRetryAsync(JobEnvelope message, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
