using EventFlow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventFlow.Infrastructure.Persistence;

public sealed class EventFlowDbContext(DbContextOptions<EventFlowDbContext> options) : DbContext(options)
{
    public DbSet<EventDefinition> Events => Set<EventDefinition>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var nullableUtcConverter = new ValueConverter<DateTimeOffset?, DateTime?>(
            value => value.HasValue ? value.Value.UtcDateTime : null,
            value => value.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
                : null);

        modelBuilder.Entity<EventDefinition>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Venue).HasMaxLength(160).IsRequired();
            entity.Property(x => x.StartsAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.CreatedAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerEmail).HasMaxLength(254).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.TicketCode).HasMaxLength(64);
            entity.Property(x => x.ExpiresAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.CreatedAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.UpdatedAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => new { x.Status, x.ExpiresAtUtc });
            entity.HasIndex(x => x.TicketCode).IsUnique();
            entity.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JobType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.Property(x => x.CreatedAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.Property(x => x.PublishedAtUtc).HasConversion(nullableUtcConverter);
            entity.HasIndex(x => new { x.PublishedAtUtc, x.CreatedAtUtc });
        });

        modelBuilder.Entity<JobExecution>(entity =>
        {
            entity.ToTable("JobExecutions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.JobType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(500);
            entity.Property(x => x.OccurredAtUtc).HasConversion(
                value => value.UtcDateTime,
                value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
            entity.HasIndex(x => new { x.MessageId, x.Outcome }).IsUnique();
            entity.HasIndex(x => new { x.ReservationId, x.OccurredAtUtc });
        });
    }
}
