namespace ppoker.Data.Models;

public class Vote
{
    public string ParticipantId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CastAt { get; set; } = DateTime.UtcNow;
}
