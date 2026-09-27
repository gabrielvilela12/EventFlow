using EventFlow.Domain;

namespace EventFlow.Application.Abstractions;

public interface INotificationSender
{
    Task SendDemoAsync(Reservation reservation, CancellationToken cancellationToken);
}
