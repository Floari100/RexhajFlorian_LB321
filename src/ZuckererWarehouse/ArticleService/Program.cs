
using ArticleService.Contracts;
using ArticleService.Messaging; 
using SharedContracts.Events;

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

    app.UseSwaggerUI(c =>
    {
        // Wichtig: RELATIV (damit es hinter /api/articles/ funktioniert)
        c.SwaggerEndpoint("./v1/swagger.json", "ArticleService v1");
        c.RoutePrefix = "swagger";
    });
}


app.UseHttpsRedirection();

// Health endpoint
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok("ArticleService running"));

app.MapPost("/articles", (CreateArticleRequest req, RabbitMqPublisher pub) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest("Name is required.");

    var id = Guid.NewGuid();

    pub.Publish("article.created", new ArticleCreated(
        id,
        req.Name,
        DateTimeOffset.UtcNow
    ));

    return Results.Created($"/articles/{id}", new { id, req.Name });
})
.WithName("CreateArticle")
.WithOpenApi();

app.Run();
