namespace Isekai.Data;

public sealed record ClickBatch(Guid BatchId, DateTime BatchDate, Dictionary<string, int> Counts);

public static class MessagingNames
{
    public const string ClickQueue = "click";
}