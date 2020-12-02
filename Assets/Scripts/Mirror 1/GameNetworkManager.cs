using Mirror;

public class GameNetworkManager : NetworkManager
{
    public bool IsHost { get; private set; } = false;

    public override void OnStartHost()
    {
        IsHost = true;
    }
}
