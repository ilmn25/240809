using System;
using System.Collections.Generic;
using System.IO;
using Mirror;

public struct ClientToServerTextMessage : NetworkMessage { public string text; }
public struct ServerToClientTextMessage : NetworkMessage { public string text; }
public struct HostToClientSnapshotChunkMessage : NetworkMessage
{
    public int index;
    public int totalChunks;
    public byte[] chunk;
}

public static class MirrorAutoHost
{
    private const int MaxSnapshotChunkSize = 12000;
    private static bool handlersRegistered;
    private static readonly Dictionary<int, byte[]> receivedSnapshotChunks = new();
    private static int expectedSnapshotChunks;

    public static void StartHostIfNeeded()
    {
        if (NetworkClient.isConnected || NetworkServer.active) return;
        NetworkManager.singleton.StartHost();
        RegisterHandlers();
    }

    public static void RegisterClientHandler() => RegisterHandlers();

    public static void SendClientTextMessage(string text)
    {
        NetworkClient.Send(new ClientToServerTextMessage { text = text });
    }

    private static void RegisterHandlers()
    {
        if (handlersRegistered) return;
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.ReplaceHandler<ServerToClientTextMessage>(OnServerToClientTextMessage, false);
        NetworkClient.ReplaceHandler<HostToClientSnapshotChunkMessage>(OnHostToClientSnapshotChunkMessage, false);
        NetworkServer.OnConnectedEvent += OnServerConnected;
        NetworkServer.ReplaceHandler<ClientToServerTextMessage>(OnClientToServerTextMessage, false);
        handlersRegistered = true;
    }

    private static void OnClientConnected() => Console.Print("Connected to host, type any text to send.");

    private static void OnServerConnected(NetworkConnectionToClient conn)
    {
        if (conn == NetworkServer.localConnection || Save.Inst == null) return;

        World.UnloadWorld();
        byte[] bytes = Helper.SerializeObject(Save.Inst);
        byte[][] chunks = Helper.SplitBytes(bytes, MaxSnapshotChunkSize);
        int totalChunks = chunks.Length;
        for (int i = 0; i < totalChunks; i++)
        {
            conn.Send(new HostToClientSnapshotChunkMessage { index = i, totalChunks = totalChunks, chunk = chunks[i] });
        }
        World.LoadWorld();
    }

    private static void OnClientToServerTextMessage(NetworkConnectionToClient conn, ClientToServerTextMessage message)
    {
        int clientId = conn.connectionId + 1;
        NetworkServer.SendToAll(new ServerToClientTextMessage { text = $"Client {clientId} > {message.text}" });
    }

    private static void OnServerToClientTextMessage(ServerToClientTextMessage message) => Console.Print(message.text);

    private static void OnHostToClientSnapshotChunkMessage(HostToClientSnapshotChunkMessage message)
    {
        if (expectedSnapshotChunks == 0) expectedSnapshotChunks = message.totalChunks;
        if (message.totalChunks != expectedSnapshotChunks)
        {
            expectedSnapshotChunks = message.totalChunks;
            receivedSnapshotChunks.Clear();
        }

        receivedSnapshotChunks[message.index] = message.chunk;
        if (receivedSnapshotChunks.Count < expectedSnapshotChunks) return;

        byte[] allBytes = Helper.CombineChunks(receivedSnapshotChunks, expectedSnapshotChunks);
        receivedSnapshotChunks.Clear();
        expectedSnapshotChunks = 0;
        Scene.SwitchSave(Helper.DeserializeObject<Save>(allBytes));
        Console.Print("Received data from host, loading in...");
    }
}
