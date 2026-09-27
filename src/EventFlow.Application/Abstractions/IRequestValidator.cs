namespace EventFlow.Application.Abstractions;

public interface IRequestValidator<in TRequest>
{
    void Validate(TRequest request);
}
