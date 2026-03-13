using Microsoft.AspNetCore.SignalR;
using ppoker.Data.Models;
using ppoker.Data.Services;

namespace ppoker.Data.Hubs;

public class PokerHub : Hub
{
    private readonly RoomManager _roomManager;

    public PokerHub(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task<RoomState?> JoinRoom(string roomId, string userName, bool isSpectator = false)
    {
        var participant = _roomManager.JoinRoom(roomId, userName, Context.ConnectionId, isSpectator);
        if (participant is null) return null;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        var state = _roomManager.GetRoomState(roomId);
        await Clients.Group(roomId).SendAsync("RoomUpdated", state);

        return state;
    }

    public async Task LeaveRoom(string roomId)
    {
        _roomManager.LeaveRoom(roomId, Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        var state = _roomManager.GetRoomState(roomId);
        if (state is not null)
        {
            await Clients.Group(roomId).SendAsync("RoomUpdated", state);
        }
    }

    public async Task CastVote(string roomId, string participantId, string voteValue)
    {
        if (_roomManager.CastVote(roomId, participantId, voteValue))
        {
            var state = _roomManager.GetRoomState(roomId);
            await Clients.Group(roomId).SendAsync("RoomUpdated", state);
        }
    }

    public async Task RevealVotes(string roomId)
    {
        if (_roomManager.RevealVotes(roomId))
        {
            var state = _roomManager.GetRoomState(roomId);
            await Clients.Group(roomId).SendAsync("RoomUpdated", state);
        }
    }

    public async Task ResetRound(string roomId, string? newStory = null)
    {
        if (_roomManager.ResetRound(roomId, newStory))
        {
            var state = _roomManager.GetRoomState(roomId);
            await Clients.Group(roomId).SendAsync("RoomUpdated", state);
        }
    }

    public async Task SetStory(string roomId, string story)
    {
        if (_roomManager.SetStory(roomId, story))
        {
            var state = _roomManager.GetRoomState(roomId);
            await Clients.Group(roomId).SendAsync("RoomUpdated", state);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Find and remove participant from all rooms they might be in
        // This is a simple implementation - in production you'd track room membership
        await base.OnDisconnectedAsync(exception);
    }
}
