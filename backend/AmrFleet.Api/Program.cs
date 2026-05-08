using System.Text.Json.Serialization;
using AmrFleet.Api.Hubs;
using AmrFleet.Api.Services;
using AmrFleet.Api.Simulators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("OperatorUi", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
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
