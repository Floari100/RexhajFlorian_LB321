using EventLogService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddHostedService<ArticleCreatedConsumer>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok("EventLogService running"));

app.Run();
