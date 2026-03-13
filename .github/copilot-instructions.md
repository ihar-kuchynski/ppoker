# Copilot Instructions for ppoker

Always use Context7 MCP to retrieve documentation for libraries and frameworks.

Before writing code:

1. Query Context7 for the latest documentation
2. Follow the latest API usage
3. Avoid outdated patterns

## Architecture Overview

This is a **Planning Poker** Blazor Server application built on .NET 10.0 with real-time updates via SignalR.

### Core Layers

- **UI (Blazor Components)**: Pages under `Components/Pages/`, reusable components under `Components/Poker/`
- **Real-time (SignalR)**: `Data/Hubs/PokerHub.cs` broadcasts state changes to room groups
- **State (In-Memory)**: `Data/Services/RoomManager.cs` singleton manages all room state

### Key Files

- `Program.cs`: App config, SignalR + RoomManager registration
- `Data/Models/`: Domain models (`Room`, `Participant`, `Vote`, `RoomState`)
- `Data/Services/RoomManager.cs`: In-memory room state manager
- `Data/Hubs/PokerHub.cs`: SignalR hub for real-time voting
- `Components/Pages/Lobby.razor`: Create/join room UI
- `Components/Pages/Room.razor`: Voting room with SignalR client
- `Components/Poker/`: Reusable UI components (`VoteCardDeck`, `ParticipantList`, `VoteResults`)

## Development Workflows

- **Build**: `dotnet build` in the `ppoker/` directory
- **Run**: `dotnet run` (launches on https://localhost:7059 by default)
- **Debug**: Use Visual Studio debugger or attach to process

## Coding Patterns

- Use `@rendermode InteractiveServer` for interactive components
- SignalR client connection in components via `HubConnectionBuilder`
- Broadcast room state via `Clients.Group(roomId).SendAsync("RoomUpdated", state)`
- State managed in `RoomManager` singleton with `ConcurrentDictionary` for thread safety
- CSS isolation with companion `.razor.css` files

## SignalR Hub API

### Client → Server (Hub Methods)

- `JoinRoom(roomId, userName, isSpectator)` → `RoomState`
- `LeaveRoom(roomId)`
- `CastVote(roomId, participantId, voteValue)`
- `RevealVotes(roomId)`
- `ResetRound(roomId, newStory?)`
- `SetStory(roomId, story)`

### Server → Client (Broadcasts)

- `RoomUpdated(RoomState)` - sent to room group on any state change

## Conventions

- Nullable reference types enabled
- Domain models in `Data/Models/`
- Services in `Data/Services/`
- SignalR hubs in `Data/Hubs/`
- Component and class names in PascalCase</content>
  <parameter name="filePath">.github\copilot-instructions.md
