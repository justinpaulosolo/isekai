using System.Text;
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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            "click",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken:
            stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(delivery.Body.Span);
                
                var click = JsonSerializer.Deserialize<ClickEvent>(
                                delivery.Body.Span,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? throw new InvalidOperationException("Click message was empty.");
                
                await shortUrlRepository.RecordClickAsync(click.Code, click.OccurredOn);
                
                await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Click handling failed");
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken);
            }
        };
        
        await channel.BasicConsumeAsync("click", false, consumer, stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }

        await channel.CloseAsync(CancellationToken.None);
    }
}