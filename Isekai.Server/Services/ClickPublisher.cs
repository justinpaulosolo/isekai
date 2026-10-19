using System.Text.Json;
using Isekai.Data;
using RabbitMQ.Client;

namespace Isekai.Server.Services;

public sealed class ClickPublisher(ClickBuffer buffer, IConnection connection, ILogger<ClickPublisher> logger) : BackgroundService
{
    private const string QueueName = "click";
    private static readonly TimeSpan FlushInternal = TimeSpan.FromSeconds(2);
    private IChannel? _channel;
    private ClickBatch? _pending;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(FlushInternal);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await FlushAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task FlushAsync(CancellationToken stoppingToken)
    {
        _pending ??= DrainBuffer();
        if (_pending is null) return;

        try
        {
            var channel =  await GetChannelAsync(stoppingToken);
            var body = JsonSerializer.SerializeToUtf8Bytes(_pending);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = _pending.BatchId.ToString("N"),
            };
            
            await channel.BasicPublishAsync("", QueueName, false, props, body, stoppingToken);

            _pending = null;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to publish click batch {BatchId}: will retry", _pending?.BatchId);
            await ResetChannelAsync();
        }
    }

    private ClickBatch? DrainBuffer()
    {
        Dictionary<string, int>? counts = null;
        while (buffer.Reader.TryRead(out var code))
        {
            counts ??= new Dictionary<string, int>();
            counts[code] = counts.GetValueOrDefault(code) + 1;
        }
        
        return counts is null
            ? null
            : new ClickBatch(Guid.NewGuid(), DateTime.UtcNow, counts);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is { IsOpen: true }) return _channel;

        await ResetChannelAsync();

        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        
        var channel = await connection.CreateChannelAsync(options, ct);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);
        
        _channel = channel;
        return channel;
    }

    private async Task ResetChannelAsync()
    {
        if (_channel is null) return;

        try
        {
            await _channel.DisposeAsync();
        }
        catch
        {
            // ignored
        }

        _channel = null;
    }
}