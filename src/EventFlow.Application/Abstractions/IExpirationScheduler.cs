namespace EventFlow.Application.Abstractions;

public interface IExpirationScheduler
{
    Task<int> EnqueueExpiredAsync(CancellationToken cancellationToken = default);
}
