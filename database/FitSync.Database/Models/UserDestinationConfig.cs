namespace FitSync.Database.Models;

public class UserDestinationConfig
{
    public Guid UserId { get; set; }
    public string SourceServiceType { get; set; } = null!;
    public string DestinationServiceType { get; set; } = null!;

    public User User { get; set; } = null!;
}
