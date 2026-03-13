using ppoker.Data.Services;

namespace ppoker.Tests.Services;

public class RoomManagerVotingTests
{
    private readonly RoomManager _sut = new();

    #region CastVote Tests

    [Fact]
    public void CastVote_ValidParticipant_ReturnsTrue()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");

        var result = _sut.CastVote(room.Id, participant!.Id, "5");

        Assert.True(result);
    }

    [Fact]
    public void CastVote_RecordsVoteValue()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");

        _sut.CastVote(room.Id, participant!.Id, "8");
        _sut.RevealVotes(room.Id);
        var state = _sut.GetRoomState(room.Id);

        Assert.Equal("8", state?.Votes[participant.Id]);
    }

    [Fact]
    public void CastVote_Spectator_ReturnsFalse()
    {
        var room = _sut.CreateRoom("Test");
        var spectator = _sut.JoinRoom(room.Id, "Observer", "conn1", isSpectator: true);

        var result = _sut.CastVote(room.Id, spectator!.Id, "5");

        Assert.False(result);
    }

    [Fact]
    public void CastVote_AfterReveal_ReturnsFalse()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.RevealVotes(room.Id);

        var result = _sut.CastVote(room.Id, participant!.Id, "5");

        Assert.False(result);
    }

    [Fact]
    public void CastVote_NonExistentRoom_ReturnsFalse()
    {
        var result = _sut.CastVote("nonexistent", "participantId", "5");

        Assert.False(result);
    }

    [Fact]
    public void CastVote_CanChangeVote()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");

        _sut.CastVote(room.Id, participant!.Id, "5");
        _sut.CastVote(room.Id, participant.Id, "8");
        _sut.RevealVotes(room.Id);
        var state = _sut.GetRoomState(room.Id);

        Assert.Equal("8", state?.Votes[participant.Id]);
    }

    #endregion

    #region RevealVotes Tests

    [Fact]
    public void RevealVotes_ValidRoom_ReturnsTrue()
    {
        var room = _sut.CreateRoom("Test");

        var result = _sut.RevealVotes(room.Id);

        Assert.True(result);
    }

    [Fact]
    public void RevealVotes_SetsIsRevealedFlag()
    {
        var room = _sut.CreateRoom("Test");

        _sut.RevealVotes(room.Id);
        var state = _sut.GetRoomState(room.Id);

        Assert.True(state?.IsRevealed);
    }

    [Fact]
    public void RevealVotes_NonExistentRoom_ReturnsFalse()
    {
        var result = _sut.RevealVotes("nonexistent");

        Assert.False(result);
    }

    #endregion

    #region ResetRound Tests

    [Fact]
    public void ResetRound_ClearsVotes()
    {
        var room = _sut.CreateRoom("Test");
        var participant = _sut.JoinRoom(room.Id, "Alice", "conn1");
        _sut.CastVote(room.Id, participant!.Id, "5");

        _sut.ResetRound(room.Id);
        var state = _sut.GetRoomState(room.Id);

        Assert.Empty(state!.Votes);
    }

    [Fact]
    public void ResetRound_ClearsIsRevealed()
    {
        var room = _sut.CreateRoom("Test");
        _sut.RevealVotes(room.Id);

        _sut.ResetRound(room.Id);
        var state = _sut.GetRoomState(room.Id);

        Assert.False(state?.IsRevealed);
    }

    [Fact]
    public void ResetRound_WithNewStory_SetsStory()
    {
        var room = _sut.CreateRoom("Test");

        _sut.ResetRound(room.Id, "New user story");
        var state = _sut.GetRoomState(room.Id);

        Assert.Equal("New user story", state?.CurrentStory);
    }

    [Fact]
    public void ResetRound_NonExistentRoom_ReturnsFalse()
    {
        var result = _sut.ResetRound("nonexistent");

        Assert.False(result);
    }

    #endregion

    #region SetStory Tests

    [Fact]
    public void SetStory_ValidRoom_ReturnsTrue()
    {
        var room = _sut.CreateRoom("Test");

        var result = _sut.SetStory(room.Id, "US-123: Add login feature");

        Assert.True(result);
    }

    [Fact]
    public void SetStory_UpdatesCurrentStory()
    {
        var room = _sut.CreateRoom("Test");

        _sut.SetStory(room.Id, "US-123: Add login feature");
        var state = _sut.GetRoomState(room.Id);

        Assert.Equal("US-123: Add login feature", state?.CurrentStory);
    }

    [Fact]
    public void SetStory_NonExistentRoom_ReturnsFalse()
    {
        var result = _sut.SetStory("nonexistent", "Story");

        Assert.False(result);
    }

    #endregion
}
