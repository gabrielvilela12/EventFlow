using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.SendNotification;

public sealed record SendNotificationRequest(JobEnvelope Message);
