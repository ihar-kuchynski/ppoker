using Microsoft.AspNetCore.SignalR;
using Moq;
using ppoker.Data.Hubs;
using ppoker.Data.Models;
using ppoker.Data.Services;

namespace ppoker.Tests.Hubs;

public class PokerHubTests
{
    private readonly RoomManager _roomManager;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly PokerHub _hub;

    public PokerHubTests()
    {
        _roomManager = new RoomManager();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();

        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        _hub = new PokerHub(_roomManager)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object,
            Context = _mockContext.Object
        };
    }

    #region JoinRoom Tests

    [Fact]
    public async Task JoinRoom_ExistingRoom_ReturnsRoomState()
    {
        var room = _roomManager.CreateRoom("Test Room");

        var result = await _hub.JoinRoom(room.Id, "Alice", false);

        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
    }

    [Fact]
    public async Task JoinRoom_ExistingRoom_AddsToGroup()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.JoinRoom(room.Id, "Alice", false);

        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", room.Id, default), Times.Once);
    }

    [Fact]
    public async Task JoinRoom_ExistingRoom_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.JoinRoom(room.Id, "Alice", false);

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task JoinRoom_NonExistentRoom_ReturnsNull()
    {
        var result = await _hub.JoinRoom("nonexistent", "Alice", false);

        Assert.Null(result);
    }

    [Fact]
    public async Task JoinRoom_AsSpectator_SetsSpectatorFlag()
    {
        var room = _roomManager.CreateRoom("Test Room");

        var result = await _hub.JoinRoom(room.Id, "Observer", true);

        Assert.NotNull(result);
        Assert.Contains(result.Participants, p => p.Name == "Observer" && p.IsSpectator);
    }

    #endregion

    #region LeaveRoom Tests

    [Fact]
    public async Task LeaveRoom_ExistingParticipant_RemovesFromGroup()
    {
        var room = _roomManager.CreateRoom("Test Room");
        _roomManager.JoinRoom(room.Id, "Alice", "test-connection-id");

        await _hub.LeaveRoom(room.Id);

        _mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection-id", room.Id, default), Times.Once);
    }

    [Fact]
    public async Task LeaveRoom_WithRemainingParticipants_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");
        _roomManager.JoinRoom(room.Id, "Alice", "other-connection");
        _roomManager.JoinRoom(room.Id, "Bob", "test-connection-id");

        await _hub.LeaveRoom(room.Id);

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    #endregion

    #region CastVote Tests

    [Fact]
    public async Task CastVote_ValidVote_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");
        var participant = _roomManager.JoinRoom(room.Id, "Alice", "test-connection-id");

        await _hub.CastVote(room.Id, participant!.Id, "5");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task CastVote_InvalidRoom_DoesNotBroadcast()
    {
        await _hub.CastVote("nonexistent", "participant", "5");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Never);
    }

    [Fact]
    public async Task CastVote_Spectator_DoesNotBroadcast()
    {
        var room = _roomManager.CreateRoom("Test Room");
        var spectator = _roomManager.JoinRoom(room.Id, "Observer", "test-connection-id", isSpectator: true);

        await _hub.CastVote(room.Id, spectator!.Id, "5");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Never);
    }

    #endregion

    #region RevealVotes Tests

    [Fact]
    public async Task RevealVotes_ValidRoom_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.RevealVotes(room.Id);

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task RevealVotes_InvalidRoom_DoesNotBroadcast()
    {
        await _hub.RevealVotes("nonexistent");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Never);
    }

    #endregion

    #region ResetRound Tests

    [Fact]
    public async Task ResetRound_ValidRoom_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.ResetRound(room.Id);

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task ResetRound_WithNewStory_SetsStory()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.ResetRound(room.Id, "New Story");

        var state = _roomManager.GetRoomState(room.Id);
        Assert.Equal("New Story", state?.CurrentStory);
    }

    [Fact]
    public async Task ResetRound_InvalidRoom_DoesNotBroadcast()
    {
        await _hub.ResetRound("nonexistent");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Never);
    }

    #endregion

    #region SetStory Tests

    [Fact]
    public async Task SetStory_ValidRoom_BroadcastsRoomUpdated()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.SetStory(room.Id, "US-123");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task SetStory_ValidRoom_UpdatesStory()
    {
        var room = _roomManager.CreateRoom("Test Room");

        await _hub.SetStory(room.Id, "US-123: Add feature");

        var state = _roomManager.GetRoomState(room.Id);
        Assert.Equal("US-123: Add feature", state?.CurrentStory);
    }

    [Fact]
    public async Task SetStory_InvalidRoom_DoesNotBroadcast()
    {
        await _hub.SetStory("nonexistent", "Story");

        _mockClientProxy.Verify(c => c.SendCoreAsync("RoomUpdated", It.IsAny<object[]>(), default), Times.Never);
    }

    #endregion
}
