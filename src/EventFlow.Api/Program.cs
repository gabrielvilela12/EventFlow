using EventFlow.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services
    .AddEventFlowPersistence(builder.Configuration)
    .AddEventFlowMessaging(builder.Configuration)
    .AddEventFlowUseCases();

var app = builder.Build();
await app.Services.InitializeEventFlowDatabaseAsync();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var status = error switch
    {
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ArgumentException or InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };
    context.Response.StatusCode = status;
    await Results.Problem(statusCode: status, title: "EventFlow request failed",
        detail: status == 500 ? "An unexpected error occurred." : error?.Message).ExecuteAsync(context);
}));

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "EventFlow API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "EventFlow API";
});

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.Run();

public partial class Program;
