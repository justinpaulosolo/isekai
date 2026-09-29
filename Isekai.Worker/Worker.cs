using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Isekai.Worker;

public class Worker(ILogger<Worker> logger, IConnection connection) : BackgroundService
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
            prefetchSize: 1,
            prefetchCount: 0,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(delivery.Body.Span);
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