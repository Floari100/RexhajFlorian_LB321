using ArticleService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health endpoint
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok("ArticleService running"));

app.Run();
