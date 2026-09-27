namespace EventFlow.Application.UseCases.Jobs.SendNotification;

public sealed record SendNotificationResponse(bool AlreadyProcessed, string Detail);
