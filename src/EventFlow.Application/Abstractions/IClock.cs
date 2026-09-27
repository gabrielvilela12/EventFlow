namespace EventFlow.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
