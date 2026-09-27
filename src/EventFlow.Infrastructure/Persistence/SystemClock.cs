using EventFlow.Application.Abstractions;

namespace EventFlow.Infrastructure.Persistence;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

