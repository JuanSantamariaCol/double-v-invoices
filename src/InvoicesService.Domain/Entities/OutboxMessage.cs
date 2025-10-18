namespace InvoicesService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string AggregateId { get; private set; } = string.Empty;
    public string AggregateType { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; private set; } = "pending";
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    // EF Core constructor
    private OutboxMessage() { }

    public OutboxMessage(
        string aggregateId,
        string aggregateType,
        string eventType,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException("Aggregate ID cannot be empty", nameof(aggregateId));

        if (string.IsNullOrWhiteSpace(aggregateType))
            throw new ArgumentException("Aggregate type cannot be empty", nameof(aggregateType));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be empty", nameof(payload));

        Id = Guid.NewGuid();
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        EventType = eventType;
        Payload = payload;
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPublished()
    {
        Status = "published";
        PublishedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        Status = "failed";
        ErrorMessage = errorMessage;
    }

    public bool IsPending() => Status == "pending";
    public bool IsPublished() => Status == "published";
    public bool IsFailed() => Status == "failed";
}
