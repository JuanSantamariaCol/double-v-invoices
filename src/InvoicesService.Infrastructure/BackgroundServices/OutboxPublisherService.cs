using InvoicesService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoicesService.Infrastructure.BackgroundServices;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisherService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly int _batchSize = 10;

    public OutboxPublisherService(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisherService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Publisher Service is stopping");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingMessages = await outboxRepository.GetPendingMessagesAsync(_batchSize, cancellationToken);

        if (!pendingMessages.Any())
        {
            _logger.LogDebug("No pending outbox messages found");
            return;
        }

        _logger.LogInformation("Processing {Count} pending outbox messages", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            try
            {
                _logger.LogInformation(
                    "Publishing event: {EventType} for {AggregateType}#{AggregateId}",
                    message.EventType,
                    message.AggregateType,
                    message.AggregateId
                );

                // TODO: Publish to message broker (Kafka, RabbitMQ, Google Pub/Sub, etc.)
                await PublishToBrokerAsync(message, cancellationToken);

                // Mark as published
                await outboxRepository.MarkAsPublishedAsync(message.Id, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully published event {EventType} for {AggregateType}#{AggregateId}",
                    message.EventType,
                    message.AggregateType,
                    message.AggregateId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish event {EventId}: {ErrorMessage}",
                    message.Id,
                    ex.Message
                );

                // Mark as failed
                await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task PublishToBrokerAsync(Domain.Entities.OutboxMessage message, CancellationToken cancellationToken)
    {
        // TODO: Implement integration with real broker (Kafka, RabbitMQ, Google Pub/Sub, etc.)
        // For now, just simulate the publication

        _logger.LogDebug("Would publish to broker: {Payload}", message.Payload);

        // Simulate async operation
        await Task.Delay(100, cancellationToken);

        // Here you would implement the actual broker integration, for example:
        
        // For Google Pub/Sub:
        // var topicName = new TopicName(projectId, topicId);
        // var publisher = await PublisherClient.CreateAsync(topicName);
        // await publisher.PublishAsync(message.Payload);
        
        // For RabbitMQ:
        // var factory = new ConnectionFactory() { HostName = "localhost" };
        // using var connection = factory.CreateConnection();
        // using var channel = connection.CreateModel();
        // var body = Encoding.UTF8.GetBytes(message.Payload);
        // channel.BasicPublish(exchange: "invoices", routingKey: message.EventType, body: body);
        
        // For Kafka:
        // var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        // using var producer = new ProducerBuilder<string, string>(config).Build();
        // await producer.ProduceAsync(message.EventType, new Message<string, string> { Key = message.AggregateId, Value = message.Payload });
    }
}
