using System.Text.Json.Serialization;
using AmrFleet.Api.Hubs;
using AmrFleet.Api.Options;
using AmrFleet.Api.Services;
using AmrFleet.Api.Simulators;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FleetConsoleOptions>(
    builder.Configuration.GetSection(FleetConsoleOptions.SectionName));
builder.Services.Configure<OperatorCommandOptions>(
    builder.Configuration.GetSection(OperatorCommandOptions.SectionName));

builder.Services.AddCors(options =>
{
    options.AddPolicy("OperatorUi", policy =>
    {
        var consoleOptions = builder.Configuration
            .GetSection(FleetConsoleOptions.SectionName)
            .Get<FleetConsoleOptions>() ?? new FleetConsoleOptions();

        policy
            .WithOrigins(consoleOptions.CorsOrigins)
            .WithHeaders(HeaderNames.ContentType, "X-Operator-Token", "X-Requested-With", "X-SignalR-User-Agent")
            .WithMethods(HttpMethods.Get, HttpMethods.Post)
            .AllowCredentials();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IRobotFleetStore, InMemoryRobotFleetStore>();
builder.Services.AddSingleton<MissionService>();
builder.Services.AddSingleton<RobotCommandService>();
builder.Services.AddSingleton<RobotMovementSimulator>();
builder.Services.AddHostedService<RobotTelemetryBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("OperatorUi");
app.MapControllers();
app.MapHub<RobotTelemetryHub>("/hubs/robotTelemetry");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
