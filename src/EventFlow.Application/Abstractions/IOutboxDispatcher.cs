namespace EventFlow.Application.Abstractions;

public interface IOutboxDispatcher
{
    Task<int> DispatchAsync(CancellationToken cancellationToken = default);
}
