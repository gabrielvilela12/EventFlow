namespace EventFlow.Application.UseCases.Events.CreateEvent;

public sealed record CreateEventRequest(string Name, string Venue, DateTimeOffset StartsAtUtc, int Capacity);
