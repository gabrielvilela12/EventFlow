using EventFlow.Domain;

namespace EventFlow.Application.Common;

public sealed record EventView(
    Guid Id,
    string Name,
    string Venue,
    DateTimeOffset StartsAtUtc,
    int Capacity,
    int AvailableSeats)
{
    public static EventView From(EventDefinition item) =>
        new(item.Id, item.Name, item.Venue, item.StartsAtUtc, item.Capacity, item.AvailableSeats);
}
