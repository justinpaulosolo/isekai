namespace Isekai.Server.Models;

public class ShortUrl
{
    public required string LongUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
