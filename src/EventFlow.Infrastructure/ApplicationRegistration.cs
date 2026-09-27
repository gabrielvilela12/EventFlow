using EventFlow.Application.Abstractions;
using EventFlow.Application.UseCases.Events.CreateEvent;
using EventFlow.Application.UseCases.Events.ListEvents;
using EventFlow.Application.UseCases.Jobs.ExpireReservation;
using EventFlow.Application.UseCases.Jobs.GenerateTicket;
using EventFlow.Application.UseCases.Jobs.ProcessReservation;
using EventFlow.Application.UseCases.Jobs.SendNotification;
using EventFlow.Application.UseCases.Outbox.DispatchOutbox;
using EventFlow.Application.UseCases.Reservations.CancelReservation;
using EventFlow.Application.UseCases.Reservations.CreateReservation;
using EventFlow.Application.UseCases.Reservations.GetReservation;
using EventFlow.Application.UseCases.Reservations.ListReservations;
using EventFlow.Application.UseCases.Reservations.ScheduleExpiration;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Infrastructure;

public static class ApplicationRegistration
{
    public static IServiceCollection AddEventFlowUseCases(this IServiceCollection services)
    {
        services.AddScoped<IRequestValidator<CreateEventRequest>, CreateEventValidator>();
        services.AddScoped<IUseCase<CreateEventRequest, CreateEventResponse>, CreateEventUseCase>();
        services.AddScoped<IRequestValidator<ListEventsRequest>, ListEventsValidator>();
        services.AddScoped<IUseCase<ListEventsRequest, ListEventsResponse>, ListEventsUseCase>();

        services.AddScoped<IRequestValidator<CreateReservationRequest>, CreateReservationValidator>();
        services.AddScoped<IUseCase<CreateReservationRequest, CreateReservationResponse>, CreateReservationUseCase>();
        services.AddScoped<IRequestValidator<GetReservationRequest>, GetReservationValidator>();
        services.AddScoped<IUseCase<GetReservationRequest, GetReservationResponse>, GetReservationUseCase>();
        services.AddScoped<IRequestValidator<ListReservationsRequest>, ListReservationsValidator>();
        services.AddScoped<IUseCase<ListReservationsRequest, ListReservationsResponse>, ListReservationsUseCase>();
        services.AddScoped<IRequestValidator<CancelReservationRequest>, CancelReservationValidator>();
        services.AddScoped<IUseCase<CancelReservationRequest, CancelReservationResponse>, CancelReservationUseCase>();

        services.AddScoped<IRequestValidator<ProcessReservationRequest>, ProcessReservationValidator>();
        services.AddJobHandler<ProcessReservationUseCase, ProcessReservationRequest, ProcessReservationResponse>();
        services.AddScoped<IRequestValidator<GenerateTicketRequest>, GenerateTicketValidator>();
        services.AddJobHandler<GenerateTicketUseCase, GenerateTicketRequest, GenerateTicketResponse>();
        services.AddScoped<IRequestValidator<SendNotificationRequest>, SendNotificationValidator>();
        services.AddJobHandler<SendNotificationUseCase, SendNotificationRequest, SendNotificationResponse>();
        services.AddScoped<IRequestValidator<ExpireReservationRequest>, ExpireReservationValidator>();
        services.AddJobHandler<ExpireReservationUseCase, ExpireReservationRequest, ExpireReservationResponse>();

        services.AddScoped<IRequestValidator<DispatchOutboxRequest>, DispatchOutboxValidator>();
        services.AddScoped<DispatchOutboxUseCase>();
        services.AddScoped<IUseCase<DispatchOutboxRequest, DispatchOutboxResponse>>(
            provider => provider.GetRequiredService<DispatchOutboxUseCase>());
        services.AddScoped<IOutboxDispatcher>(provider => provider.GetRequiredService<DispatchOutboxUseCase>());

        services.AddScoped<IRequestValidator<ScheduleExpirationRequest>, ScheduleExpirationValidator>();
        services.AddScoped<ScheduleExpirationUseCase>();
        services.AddScoped<IUseCase<ScheduleExpirationRequest, ScheduleExpirationResponse>>(
            provider => provider.GetRequiredService<ScheduleExpirationUseCase>());
        services.AddScoped<IExpirationScheduler>(provider => provider.GetRequiredService<ScheduleExpirationUseCase>());
        return services;
    }

    private static void AddJobHandler<TUseCase, TRequest, TResponse>(this IServiceCollection services)
        where TUseCase : class, IUseCase<TRequest, TResponse>, IJobHandler
    {
        services.AddScoped<TUseCase>();
        services.AddScoped<IUseCase<TRequest, TResponse>>(provider => provider.GetRequiredService<TUseCase>());
        services.AddScoped<IJobHandler>(provider => provider.GetRequiredService<TUseCase>());
    }
}
