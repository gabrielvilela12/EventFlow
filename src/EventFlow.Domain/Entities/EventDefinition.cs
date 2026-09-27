namespace EventFlow.Domain;

public sealed class EventDefinition
{
    private EventDefinition() { }

    private EventDefinition(string name, string venue, DateTimeOffset startsAtUtc, int capacity)
    {
        Id = Guid.NewGuid();
        Name = name;
        Venue = venue;
        StartsAtUtc = startsAtUtc;
        Capacity = capacity;
        AvailableSeats = capacity;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Venue { get; private set; } = string.Empty;
    public DateTimeOffset StartsAtUtc { get; private set; }
    public int Capacity { get; private set; }
    public int AvailableSeats { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public int Version { get; private set; }

    public static EventDefinition Create(string name, string venue, DateTimeOffset startsAtUtc, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Event name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(venue)) throw new ArgumentException("Venue is required.", nameof(venue));
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        if (startsAtUtc <= DateTimeOffset.UtcNow) throw new ArgumentException("The event must start in the future.", nameof(startsAtUtc));
        return new EventDefinition(name.Trim(), venue.Trim(), startsAtUtc, capacity);
    }

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (AvailableSeats < quantity) return false;
        AvailableSeats -= quantity;
        Version++;
        return true;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        AvailableSeats = Math.Min(Capacity, AvailableSeats + quantity);
        Version++;
    }
}
