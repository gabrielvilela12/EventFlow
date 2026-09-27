namespace EventFlow.Application.Messaging;

public static class JobTypes
{
    public const string ProcessReservation = "ProcessReservation";
    public const string GenerateTicket = "GenerateTicket";
    public const string SendNotification = "SendNotification";
    public const string ExpireReservation = "ExpireReservation";

    public static readonly IReadOnlyList<string> All =
    [
        ProcessReservation,
        GenerateTicket,
        SendNotification,
        ExpireReservation
    ];
}
