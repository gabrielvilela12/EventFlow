using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.ExpireReservation;

public sealed class ExpireReservationValidator : IRequestValidator<ExpireReservationRequest>
{
    public void Validate(ExpireReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        JobEnvelopeValidation.Validate(request.Message, JobTypes.ExpireReservation);
    }
}
