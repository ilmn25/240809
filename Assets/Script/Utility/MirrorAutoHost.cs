using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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
    private static GameObject networkPrefab;

    public static void StartHost()  
    {
        if (NetworkClient.isConnected || NetworkServer.active) return;
        NetworkManager.singleton.StartHost(); 
        RegisterHandlers(); 
    }

    public static void StartClient()
    {
        NetworkManager.singleton.StartClient();
        RegisterHandlers(); 
    }

    public static void SendClientTextMessage(string text)
    {
        NetworkClient.Send(new ClientToServerTextMessage { text = text });
    }

    public static void RegisterHandlers()
    {
        if (handlersRegistered) return;

        networkPrefab = Resources.Load<GameObject>("Prefab/Network");
        NetworkClient.RegisterPrefab(networkPrefab);
        NetworkManager.singleton.spawnPrefabs.Add(networkPrefab);

        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.ReplaceHandler<ServerToClientTextMessage>(OnServerToClientTextMessage, false);
        NetworkClient.ReplaceHandler<HostToClientSnapshotChunkMessage>(OnHostToClientSnapshotChunkMessage, false);
        NetworkServer.OnConnectedEvent += conn => NetworkManager.singleton.StartCoroutine(OnServerConnected(conn));
        NetworkServer.ReplaceHandler<ClientToServerTextMessage>(OnClientToServerTextMessage, false);
        handlersRegistered = true;
    }

    private static void OnClientConnected() => Console.Print("Connected to host, type any text to send.");

    private static IEnumerator OnServerConnected(NetworkConnectionToClient conn)
    {
        while (NetworkServer.active && conn != null && NetworkServer.connections.ContainsKey(conn.connectionId) && !conn.isReady)
            yield return null;
        if (!NetworkServer.active || conn == null || !NetworkServer.connections.ContainsKey(conn.connectionId) || !conn.isReady)
            yield break;

        if (conn == NetworkServer.localConnection || Save.Inst == null) yield break;
        World.UnloadWorld();
        byte[] bytes = Helper.SerializeObject(Save.Inst);
        byte[][] chunks = Helper.SplitBytes(bytes, MaxSnapshotChunkSize);
        int totalChunks = chunks.Length;
        for (int i = 0; i < totalChunks; i++)
        {
            conn.Send(new HostToClientSnapshotChunkMessage { index = i, totalChunks = totalChunks, chunk = chunks[i] });
        }
        World.LoadWorld();
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(networkPrefab));
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
