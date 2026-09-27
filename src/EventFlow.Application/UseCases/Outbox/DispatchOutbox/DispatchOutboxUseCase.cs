using System.Text.Json;
using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Outbox.DispatchOutbox;

public sealed class DispatchOutboxUseCase(
    IOutboxStore store,
    IJobPublisher publisher,
    IClock clock,
    IRequestValidator<DispatchOutboxRequest> validator)
    : IUseCase<DispatchOutboxRequest, DispatchOutboxResponse>, IOutboxDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DispatchOutboxResponse> ExecuteAsync(DispatchOutboxRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);
        var pending = await store.ListUnpublishedAsync(request.BatchSize, cancellationToken);
        var published = 0;
        foreach (var item in pending)
        {
            var message = JsonSerializer.Deserialize<JobEnvelope>(item.Payload, JsonOptions)
                ?? throw new InvalidOperationException($"Outbox message {item.Id} has an invalid payload.");
            JobEnvelopeValidation.Validate(message);
            await publisher.PublishMainAsync(message, cancellationToken);
            await store.MarkPublishedAsync(item, clock.UtcNow, cancellationToken);
            published++;
        }
        return new DispatchOutboxResponse(published);
    }

    public async Task<int> DispatchAsync(CancellationToken cancellationToken = default) =>
        (await ExecuteAsync(new DispatchOutboxRequest(), cancellationToken)).Published;
}
