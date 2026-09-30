namespace Isekai.Server.Models;

public class ShortUrl
{
    public required string LongUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Title { get; set; }
    public long ClickCount { get; set; }
    public DateTime? LastClickedAt { get; set; }
}
