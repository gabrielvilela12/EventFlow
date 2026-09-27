using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.SendNotification;

public sealed class SendNotificationValidator : IRequestValidator<SendNotificationRequest>
{
    public void Validate(SendNotificationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        JobEnvelopeValidation.Validate(request.Message, JobTypes.SendNotification);
    }
}
