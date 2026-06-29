namespace PCL_CE.Neo.Core.Link.Lobby;

public enum LobbyState
{
    Idle,
    Initializing,
    Initialized,
    Discovering,
    Creating,
    Joining,
    Connected,
    Leaving,
    Error
}