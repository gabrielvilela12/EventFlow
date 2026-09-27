using EventFlow.Application.Abstractions;
using EventFlow.Domain;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Notifications;

internal sealed class DemoNotificationSender(ILogger<DemoNotificationSender> logger) : INotificationSender
{
    public Task SendDemoAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Demo notification sent to {Email} for ticket {TicketCode}.", reservation.CustomerEmail, reservation.TicketCode);
        return Task.CompletedTask;
    }
}
