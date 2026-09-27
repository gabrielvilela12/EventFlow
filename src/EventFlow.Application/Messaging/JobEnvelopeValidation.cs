namespace EventFlow.Application.Messaging;

public static class JobEnvelopeValidation
{
    public static void Validate(JobEnvelope? message, string? expectedJobType = null)
    {
        if (message is null) throw new InvalidOperationException("Message body is empty.");
        if (message.SchemaVersion != JobEnvelope.CurrentSchemaVersion) throw new InvalidOperationException("Unsupported schema version.");
        if (message.MessageId == Guid.Empty || message.CorrelationId == Guid.Empty || message.ReservationId == Guid.Empty)
            throw new InvalidOperationException("Message identifiers are required.");
        if (!JobTypes.All.Contains(message.JobType)) throw new InvalidOperationException("Unknown job type.");
        if (expectedJobType is not null && message.JobType != expectedJobType)
            throw new InvalidOperationException($"Expected {expectedJobType}, but message is {message.JobType}.");
        if (message.Attempt < 0) throw new InvalidOperationException("Attempt cannot be negative.");
    }
}
