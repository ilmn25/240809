using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public static class Server
{
    internal const int MaxSnapshotChunkSize = 12000;
    private static bool handlersRegistered;
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

    public static void RegisterHandlers()
    {
        if (handlersRegistered) return;

        networkPrefab = Resources.Load<GameObject>("Prefab/Network");
        NetworkClient.RegisterPrefab(networkPrefab);
        NetworkManager.singleton.spawnPrefabs.Add(networkPrefab);

        NetworkClient.OnConnectedEvent += OnClientConnected;
        Chat.RegisterHandlers();
        ChunkSync.RegisterHandlers();
        EntitySync.RegisterHandlers();
        NetworkServer.OnConnectedEvent += conn => NetworkManager.singleton.StartCoroutine(OnServerConnected(conn));
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
        ChunkSync.SendSaveChunks(conn, Save.Inst);
        World.LoadWorld();
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(networkPrefab));
    }
}
