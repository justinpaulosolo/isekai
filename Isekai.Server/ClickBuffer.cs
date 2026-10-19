using System.Threading.Channels;

namespace Isekai.Server;

public sealed class ClickBuffer
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(100_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        });
    
    public void Record(string code) => _channel.Writer.TryWrite(code);
    
    public ChannelReader<string> Reader => _channel.Reader;
}