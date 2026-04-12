using Mirror;

public struct ClientToServerTextMessage : NetworkMessage
{
    public string text;
}

public struct ServerToClientTextMessage : NetworkMessage
{
    public string text;
}

public static class MirrorAutoHost
{
    public static void StartHostIfNeeded()
    {
        if (NetworkClient.isConnected || NetworkServer.active)
            return;

        Console.Print("Mirror host starting...");
        NetworkClient.ReplaceHandler<ServerToClientTextMessage>(OnServerToClientTextMessage, false);
        NetworkServer.ReplaceHandler<ClientToServerTextMessage>(OnClientToServerTextMessage, false);
        NetworkManager.singleton.StartHost();
    }

    public static void RegisterClientHandler()
    {
        NetworkClient.ReplaceHandler<ServerToClientTextMessage>(OnServerToClientTextMessage, false);
    }

    public static void SendClientTextMessage(string text)
    {
        NetworkClient.Send(new ClientToServerTextMessage { text = text });
    }

    private static void OnClientToServerTextMessage(NetworkConnectionToClient conn, ClientToServerTextMessage message)
    {
        int clientId = conn.connectionId + 1;
        NetworkServer.SendToAll(new ServerToClientTextMessage { text = $"PLayer {clientId} > {message.text}" });
    }

    private static void OnServerToClientTextMessage(ServerToClientTextMessage message)
    {
        Console.Print(message.text);
    }
}
