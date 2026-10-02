namespace Isekai.Server.Models;

public record ShortenRequest(string Url, string? Title = null, long? Userid = null);