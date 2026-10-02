namespace Isekai.Data;

public class UserLogin
{
    public string Provider { get; set; } = null!;
    public string ProviderSubject { get; set; } = null!;
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}