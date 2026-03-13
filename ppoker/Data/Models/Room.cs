using System.Collections.Concurrent;

namespace ppoker.Data.Models;

public class Room
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ConcurrentDictionary<string, Participant> Participants { get; } = new();
    public ConcurrentDictionary<string, Vote> Votes { get; } = new();

    public string? CurrentStory { get; set; }
    public bool IsRevealed { get; set; }

    public string[] Deck { get; set; } = ["0", "1", "2", "3", "5", "8", "13", "21", "?", "☕"];
}
