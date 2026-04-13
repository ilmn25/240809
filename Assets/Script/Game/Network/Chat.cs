using Mirror;
using UnityEngine;

public struct ClientToServerTextMessage : NetworkMessage { public string text; }
public struct ServerToClientTextMessage : NetworkMessage { public string text; }

public static class Chat
{
    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<ServerToClientTextMessage>(OnServerToClientTextMessage, false);
        NetworkServer.ReplaceHandler<ClientToServerTextMessage>(OnClientToServerTextMessage, false);
    }

    public static void SendClientTextMessage(string text)
    {
        if (!NetworkClient.isConnected) return;
        NetworkClient.Send(new ClientToServerTextMessage { text = text });
    }

    private static void OnClientToServerTextMessage(NetworkConnectionToClient conn, ClientToServerTextMessage message)
    {
        int clientId = conn.connectionId + 1;
        string clientLabel = clientId == 1 ? $"Client {clientId} (Host)" : $"Client {clientId}";
        NetworkServer.SendToAll(new ServerToClientTextMessage { text = $"{clientLabel} > {message.text}" });
    }

    private static void OnServerToClientTextMessage(ServerToClientTextMessage message)
    {
        Console.Print(message.text);
    }
}
