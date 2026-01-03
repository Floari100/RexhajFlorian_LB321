namespace SharedContracts.Events;

public record ArticleCreated(
    Guid ArticleId,
    string Name,
    DateTimeOffset CreatedAt
);
