namespace ppoker.Data.Models;

public class Participant
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public bool IsSpectator { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
