namespace EventFlow.Application.UseCases.Outbox.DispatchOutbox;

public sealed record DispatchOutboxRequest(int BatchSize = 50);
