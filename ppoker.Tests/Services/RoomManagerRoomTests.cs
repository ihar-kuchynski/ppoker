using ppoker.Data.Services;

namespace ppoker.Tests.Services;

public class RoomManagerRoomTests
{
    private readonly RoomManager _sut = new();

    #region CreateRoom Tests

    [Fact]
    public void CreateRoom_WithValidName_ReturnsRoomWithId()
    {
        var room = _sut.CreateRoom("Sprint Planning");

        Assert.NotNull(room);
        Assert.NotEmpty(room.Id);
        Assert.Equal("Sprint Planning", room.Name);
    }

    [Fact]
    public void CreateRoom_CreatesRoomThatCanBeRetrieved()
    {
        var room = _sut.CreateRoom("Test Room");

        var retrieved = _sut.GetRoom(room.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(room.Id, retrieved.Id);
    }

    [Fact]
    public void CreateRoom_SetsDefaultFibonacciDeck()
    {
        var room = _sut.CreateRoom("Test");

        Assert.Contains("0", room.Deck);
        Assert.Contains("1", room.Deck);
        Assert.Contains("5", room.Deck);
        Assert.Contains("13", room.Deck);
        Assert.Contains("?", room.Deck);
    }

    #endregion

    #region RoomExists Tests

    [Fact]
    public void RoomExists_WithExistingRoom_ReturnsTrue()
    {
        var room = _sut.CreateRoom("Test");

        Assert.True(_sut.RoomExists(room.Id));
    }

    [Fact]
    public void RoomExists_WithNonExistingRoom_ReturnsFalse()
    {
        Assert.False(_sut.RoomExists("nonexistent"));
    }

    #endregion

    #region GetRoomState Tests

    [Fact]
    public void GetRoomState_ReturnsCorrectParticipantCount()
    {
        var room = _sut.CreateRoom("Test");
        _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.JoinRoom(room.Id, "Bob", "conn2");

        var state = _sut.GetRoomState(room.Id);

        Assert.Equal(2, state?.Participants.Count);
    }

    [Fact]
    public void GetRoomState_HidesVotesBeforeReveal()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.CastVote(room.Id, participant!.Id, "5");

        var state = _sut.GetRoomState(room.Id);

        Assert.Null(state?.Votes[participant.Id]); // Vote value hidden
        Assert.True(state?.Participants.First().HasVoted); // But HasVoted is true
    }

    [Fact]
    public void GetRoomState_ShowsVotesAfterReveal()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.CastVote(room.Id, participant!.Id, "5");
        _sut.RevealVotes(room.Id);

        var state = _sut.GetRoomState(room.Id);

        Assert.Equal("5", state?.Votes[participant.Id]);
    }

    [Fact]
    public void GetRoomState_NonExistentRoom_ReturnsNull()
    {
        var state = _sut.GetRoomState("nonexistent");

        Assert.Null(state);
    }

    #endregion
}
