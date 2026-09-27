using EventFlow.Application.Abstractions;
using EventFlow.Application.UseCases.Reservations.CancelReservation;
using EventFlow.Application.UseCases.Reservations.CreateReservation;
using EventFlow.Application.UseCases.Reservations.GetReservation;
using EventFlow.Application.UseCases.Reservations.ListReservations;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController(
    IUseCase<CreateReservationRequest, CreateReservationResponse> createReservation,
    IUseCase<GetReservationRequest, GetReservationResponse> getReservation,
    IUseCase<ListReservationsRequest, ListReservationsResponse> listReservations,
    IUseCase<CancelReservationRequest, CancelReservationResponse> cancelReservation)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await listReservations.ExecuteAsync(
            new ListReservationsRequest(),
            cancellationToken);

        return Ok(result.Reservations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await getReservation.ExecuteAsync(
            new GetReservationRequest(id),
            cancellationToken);

        return result.Reservation is null
            ? NotFound()
            : Ok(result.Reservation);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createReservation.ExecuteAsync(
            request,
            cancellationToken);

        return Accepted(
            $"/api/reservations/{result.Reservation.Id}",
            result.Reservation);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await cancelReservation.ExecuteAsync(
            new CancelReservationRequest(id),
            cancellationToken);

        return result.Cancelled
            ? NoContent()
            : NotFound();
    }
}