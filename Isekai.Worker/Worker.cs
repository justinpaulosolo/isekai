using System.Text.Json;
using Isekai.Data;
using Isekai.Data.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Isekai.Worker;

public class Worker(
    ILogger<Worker> logger,
    IConnection connection,
    IShortUrlRepository shortUrlRepository
    ) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Pause before requeueing after a transient failure (e.g. Postgres down),
    // otherwise the broker redelivers instantly and we spin in a hot loop.
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await channel.QueueDeclareAsync(
            MessagingNames.ClickQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                await HandleAsync(channel, delivery, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unexpected error handling delivery {DeliveryTag}", delivery.DeliveryTag);
            }
        };

        await channel.BasicConsumeAsync(MessagingNames.ClickQueue, false, consumer, stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }

        await channel.CloseAsync(CancellationToken.None);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken ct)
    {
        ClickBatch? batch;
        try
        {
            batch = JsonSerializer.Deserialize<ClickBatch>(delivery.Body.Span, JsonOptions);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "Malformed click message (tag {DeliveryTag}); rejecting", delivery.DeliveryTag);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: false, ct);
            return;
        }

        if (batch?.Counts is null || batch.Counts.Count == 0 || batch.Counts.Values.Any(c => c <= 0))
        {
            logger.LogError("Invalid click batch (tag {DeliveryTag}); rejecting", delivery.DeliveryTag);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: false, ct);
            return;
        }

        try
        {
            var applied = await shortUrlRepository.ApplyClickBatchAsync(batch, ct);

            if (!applied)
                logger.LogInformation("Batch {BatchId} already processed; skipping (redelivered: {Redelivered})",
                    batch.BatchId, delivery.Redelivered);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to apply batch {BatchId}; requeueing in {Delay}s",
                batch.BatchId, RetryDelay.TotalSeconds);

            await Task.Delay(RetryDelay, ct);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, requeue: true, ct);
            return;
        }

        await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
    }
}