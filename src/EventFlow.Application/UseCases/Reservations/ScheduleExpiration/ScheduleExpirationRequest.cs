namespace EventFlow.Application.UseCases.Reservations.ScheduleExpiration;

public sealed record ScheduleExpirationRequest(int BatchSize = 100);
