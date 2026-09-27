using EventFlow.Application.Abstractions;
using EventFlow.Application.Messaging;

namespace EventFlow.Application.UseCases.Jobs.GenerateTicket;

public sealed class GenerateTicketValidator : IRequestValidator<GenerateTicketRequest>
{
    public void Validate(GenerateTicketRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        JobEnvelopeValidation.Validate(request.Message, JobTypes.GenerateTicket);
    }
}
