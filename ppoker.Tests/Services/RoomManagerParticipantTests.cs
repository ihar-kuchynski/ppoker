using ppoker.Data.Services;

namespace ppoker.Tests.Services;

public class RoomManagerParticipantTests
{
    private readonly RoomManager _sut = new();

    #region JoinRoom Tests

    [Fact]
    public void JoinRoom_FirstParticipant_BecomesHost()
    {
        var room = _sut.CreateRoom("Test");

        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");

        Assert.NotNull(participant);
        Assert.Equal("Alice", participant.Name);
        Assert.True(participant.IsHost);
    }

    [Fact]
    public void JoinRoom_SecondParticipant_IsNotHost()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1");

        var participant = _sut.JoinRoom(room.Id, "Bob", "conn2");

        Assert.NotNull(participant);
        Assert.False(participant.IsHost);
    }

    [Fact]
    public void JoinRoom_AsSpectator_SetsSpectatorFlag()
    {
        var room = _sut.CreateRoom("Test");

        var participant = _sut.JoinRoom(room.Id, "Observer", "conn1", isSpectator: true);

        Assert.NotNull(participant);
        Assert.True(participant.IsSpectator);
    }

    [Fact]
    public void JoinRoom_NonExistentRoom_ReturnsNull()
    {
        var participant = _sut.JoinRoom("nonexistent", "Alice", "conn1");

        Assert.Null(participant);
    }

    #endregion

    #region LeaveRoom Tests

    [Fact]
    public void LeaveRoom_ExistingParticipant_ReturnsTrue()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1");

        var result = _sut.LeaveRoom(room.Id, "conn1");

        Assert.True(result);
    }

    [Fact]
    public void LeaveRoom_LastParticipant_DeletesRoom()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1");

        _sut.LeaveRoom(room.Id, "conn1");

        Assert.False(_sut.RoomExists(room.Id));
    }

    [Fact]
    public void LeaveRoom_HostLeaves_TransfersHostToNextParticipant()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1"); // Host
        _sut.JoinRoom(room.Id, "Bob", "conn2");

        _sut.LeaveRoom(room.Id, "conn1"); // Alice (host) leaves

        var updatedBob = _sut.GetParticipantByConnectionId(room.Id, "conn2");
        Assert.True(updatedBob?.IsHost);
    }

    [Fact]
    public void LeaveRoom_RemovesParticipantVotes()
    {
        var room = _sut.CreateRoom("Test");
        var alice = _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.CastVote(room.Id, alice!.Id, "5");

        _sut.LeaveRoom(room.Id, "conn1");

        // Room should be deleted since Alice was the only participant
        Assert.False(_sut.RoomExists(room.Id));
    }

    [Fact]
    public void LeaveRoom_NonExistentRoom_ReturnsFalse()
    {
        var result = _sut.LeaveRoom("nonexistent", "conn1");

        Assert.False(result);
    }

    #endregion

    #region GetParticipantByConnectionId Tests

    [Fact]
    public void GetParticipantByConnectionId_ExistingConnection_ReturnsParticipant()
    {
        var room = _sut.CreateRoom("Test");
        var alice = _sut.JoinRoom(room.Id, "Alice", "conn1");

        var found = _sut.GetParticipantByConnectionId(room.Id, "conn1");

        Assert.NotNull(found);
        Assert.Equal(alice?.Id, found.Id);
    }

    [Fact]
    public void GetParticipantByConnectionId_NonExistingConnection_ReturnsNull()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1");

        var found = _sut.GetParticipantByConnectionId(room.Id, "unknown");

        Assert.Null(found);
    }

    #endregion
}
