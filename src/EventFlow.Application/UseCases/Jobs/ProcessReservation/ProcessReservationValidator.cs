using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.ProcessReservation;

public sealed class ProcessReservationValidator : IRequestValidator<ProcessReservationRequest>
{
    public void Validate(ProcessReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        JobEnvelopeValidation.Validate(request.Message, JobTypes.ProcessReservation);
    }
}
