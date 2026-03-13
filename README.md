# Planning Poker (ppoker)

A real-time Planning Poker application built with Blazor Server and SignalR for agile estimation sessions.

## Features

- **Create & Join Rooms**: Host creates a room, shares the ID with teammates
- **Real-time Voting**: All participants see vote status update instantly via SignalR
- **Hidden Votes**: Votes remain hidden until the host reveals them
- **Vote Statistics**: Average, min/max, and distribution shown after reveal
- **Spectator Mode**: Join as observer without voting
- **Fibonacci Deck**: Default deck includes 0, 1, 2, 3, 5, 8, 13, 21, ?, ☕

## Architecture

```
ppoker/
├── Data/
│   ├── Models/           # Domain models
│   │   ├── Room.cs       # Room with participants & votes
│   │   ├── Participant.cs
│   │   ├── Vote.cs
│   │   └── RoomState.cs  # DTO for SignalR broadcasts
│   ├── Services/
│   │   └── RoomManager.cs  # Singleton in-memory state manager
│   └── Hubs/
│       └── PokerHub.cs     # SignalR hub for real-time updates
├── Components/
│   ├── Pages/
│   │   ├── Lobby.razor     # Create/join room UI
│   │   └── Room.razor      # Voting room with SignalR client
│   └── Poker/
│       ├── VoteCardDeck.razor    # Card selection UI
│       ├── ParticipantList.razor # Shows participants & vote status
│       └── VoteResults.razor     # Vote distribution & statistics
└── Program.cs              # App config, SignalR registration
```

### Tech Stack

- **.NET 10.0** / Blazor Server
- **SignalR** for real-time WebSocket communication
- **Bootstrap 5** for UI styling
- **In-memory state** (no database required)

### SignalR Hub API

**Client → Server:**

- `JoinRoom(roomId, userName, isSpectator)` → `RoomState`
- `LeaveRoom(roomId)`
- `CastVote(roomId, participantId, voteValue)`
- `RevealVotes(roomId)`
- `ResetRound(roomId, newStory?)`
- `SetStory(roomId, story)`

**Server → Client:**

- `RoomUpdated(RoomState)` - broadcast to room group on any state change

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Run Locally

```bash
cd ppoker
dotnet run
```

Open https://localhost:7059 in your browser.

### Test Multi-User

1. Open https://localhost:7059 in two browser tabs
2. Create a room in the first tab
3. Copy the Room ID and join from the second tab
4. Cast votes and see real-time updates

## Development

```bash
# Build
dotnet build

# Run with hot reload
dotnet watch run --project ppoker

# Run tests
dotnet test
```

## Testing

**54 unit tests** covering all business logic:

| Component     | Tests | Coverage                                |
| ------------- | ----- | --------------------------------------- |
| `RoomManager` | 36    | Room CRUD, participants, voting, rounds |
| `PokerHub`    | 18    | SignalR broadcasting, group management  |

Test structure:

```
ppoker.Tests/
├── Services/
│   ├── RoomManagerRoomTests.cs        # Room creation, state
│   ├── RoomManagerParticipantTests.cs # Join/leave, host transfer
│   └── RoomManagerVotingTests.cs      # Votes, reveal, reset
└── Hubs/
    └── PokerHubTests.cs               # SignalR hub methods
```

## License

MIT
