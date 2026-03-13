using System.Collections.Concurrent;
using ppoker.Data.Models;

namespace ppoker.Data.Services;

/// <summary>
/// Singleton service managing all active poker rooms in memory.
/// </summary>
public class RoomManager
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public Room CreateRoom(string roomName)
    {
        var room = new Room { Name = roomName };
        _rooms[room.Id] = room;
        return room;
    }

    public Room? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public bool RoomExists(string roomId) => _rooms.ContainsKey(roomId);

    public Participant? JoinRoom(string roomId, string participantName, string connectionId, bool isSpectator = false)
    {
        var room = GetRoom(roomId);
        if (room is null) return null;

        var participant = new Participant
        {
            Name = participantName,
            ConnectionId = connectionId,
            IsHost = room.Participants.IsEmpty,
            IsSpectator = isSpectator
        };

        room.Participants[participant.Id] = participant;
        return participant;
    }

    public bool LeaveRoom(string roomId, string connectionId)
    {
        var room = GetRoom(roomId);
        if (room is null) return false;

        var participant = room.Participants.Values.FirstOrDefault(p => p.ConnectionId == connectionId);
        if (participant is null) return false;

        room.Participants.TryRemove(participant.Id, out _);
        room.Votes.TryRemove(participant.Id, out _);

        // If room is empty, delete it
        if (room.Participants.IsEmpty)
        {
            _rooms.TryRemove(roomId, out _);
        }
        // Transfer host if the host left
        else if (participant.IsHost)
        {
            var newHost = room.Participants.Values.FirstOrDefault();
            if (newHost is not null) newHost.IsHost = true;
        }

        return true;
    }

    public bool CastVote(string roomId, string participantId, string voteValue)
    {
        var room = GetRoom(roomId);
        if (room is null || room.IsRevealed) return false;

        var participant = room.Participants.GetValueOrDefault(participantId);
        if (participant is null || participant.IsSpectator) return false;

        room.Votes[participantId] = new Vote
        {
            ParticipantId = participantId,
            Value = voteValue
        };

        return true;
    }

    public bool RevealVotes(string roomId)
    {
        var room = GetRoom(roomId);
        if (room is null) return false;

        room.IsRevealed = true;
        return true;
    }

    public bool ResetRound(string roomId, string? newStory = null)
    {
        var room = GetRoom(roomId);
        if (room is null) return false;

        room.Votes.Clear();
        room.IsRevealed = false;
        room.CurrentStory = newStory;
        return true;
    }

    public bool SetStory(string roomId, string story)
    {
        var room = GetRoom(roomId);
        if (room is null) return false;

        room.CurrentStory = story;
        return true;
    }

    public RoomState? GetRoomState(string roomId)
    {
        var room = GetRoom(roomId);
        if (room is null) return null;

        return new RoomState
        {
            RoomId = room.Id,
            RoomName = room.Name,
            CurrentStory = room.CurrentStory,
            IsRevealed = room.IsRevealed,
            Deck = room.Deck,
            Participants = room.Participants.Values.Select(p => new ParticipantState
            {
                Id = p.Id,
                Name = p.Name,
                IsHost = p.IsHost,
                IsSpectator = p.IsSpectator,
                HasVoted = room.Votes.ContainsKey(p.Id)
            }).ToList(),
            Votes = room.IsRevealed
                ? room.Votes.ToDictionary(v => v.Key, v => (string?)v.Value.Value)
                : room.Votes.ToDictionary(v => v.Key, _ => (string?)null)
        };
    }

    public Participant? GetParticipantByConnectionId(string roomId, string connectionId)
    {
        var room = GetRoom(roomId);
        return room?.Participants.Values.FirstOrDefault(p => p.ConnectionId == connectionId);
    }
}
