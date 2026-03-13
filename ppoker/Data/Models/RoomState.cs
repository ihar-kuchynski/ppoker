namespace ppoker.Data.Models;

/// <summary>
/// DTO for broadcasting room state to clients via SignalR.
/// </summary>
public class RoomState
{
    public string RoomId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string? CurrentStory { get; set; }
    public bool IsRevealed { get; set; }
    public string[] Deck { get; set; } = [];
    public List<ParticipantState> Participants { get; set; } = [];
    public Dictionary<string, string?> Votes { get; set; } = [];
}

public class ParticipantState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public bool IsSpectator { get; set; }
    public bool HasVoted { get; set; }
}
