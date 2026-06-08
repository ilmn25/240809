using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Mirror;
using UnityEngine;

public static class Server
{
    internal const int MaxSnapshotChunkSize = 12000;
    private const int DefaultHostPort = 7777;
    private static bool handlersRegistered;
    private static readonly Action<NetworkConnectionToClient> serverConnectedHandler = conn => NetworkManager.singleton.StartCoroutine(OnServerConnected(conn));
    private static GameObject networkPrefab;

    static Server()
    {
        networkPrefab = Resources.Load<GameObject>("Prefab/Network");
        NetworkClient.RegisterPrefab(networkPrefab);
        NetworkManager.singleton.spawnPrefabs.Add(networkPrefab);
    }

    public static bool StartHost()
    {
        if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        if (Save.Inst == null)
            Save.Inst = new Save(GenType.SkyBlock);

        PortTransport transport = Transport.active as PortTransport;
        int port = transport != null ? transport.Port : DefaultHostPort;
        while (IsPortInUse(port)) port++;
        if (transport != null) transport.Port = (ushort)port;
        NetworkManager.singleton.StartHost();
        Scene.LoadWorld();
        RegisterHandlers();
        return true;
    }

    public static void StartClient(string address = null)
    {
        if (NetworkServer.active)
            StopHost();

        NetworkManager.singleton.networkAddress = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address;
        NetworkManager.singleton.StartClient();
        RegisterHandlers(); 
    }

    public static void StopHost()
    {
        NetworkManager.singleton.StopHost();
        UnregisterHandlers();
        handlersRegistered = false;
        if (Transport.active is PortTransport transport) transport.Port = DefaultHostPort;
    }

    private static void UnregisterHandlers()
    {
        NetworkClient.OnConnectedEvent -= OnClientConnected;
        NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
        NetworkServer.OnConnectedEvent -= serverConnectedHandler;
        NetworkServer.OnDisconnectedEvent -= PlayerSync.OnServerDisconnected;
        NetworkClient.UnregisterHandler<ServerToClientTextMessage>();
        NetworkServer.UnregisterHandler<ClientToServerTextMessage>();
        NetworkClient.UnregisterHandler<HostToClientSnapshotChunkMessage>();
        NetworkClient.UnregisterHandler<BatchEntityInfoMessage>();
        NetworkClient.UnregisterHandler<PlayerSyncMessage>();
        DropSync.UnregisterHandlers();
        NetworkClient.UnregisterHandler<YourConnectionIdMessage>();
        NetworkServer.UnregisterHandler<ClientToServerPlayerMessage>();
    }

    private static void RegisterHandlers()
    {
        if (handlersRegistered) return;
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
        Chat.RegisterHandlers();
        ChunkSync.RegisterHandlers();
        EntitySync.RegisterHandlers();
        PlayerSync.RegisterHandlers();
        StorageSync.RegisterHandlers();
        DropSync.RegisterHandlers();
        EffectSync.RegisterHandlers();
        NetworkServer.OnConnectedEvent += serverConnectedHandler;
        NetworkServer.OnDisconnectedEvent += PlayerSync.OnServerDisconnected;
        handlersRegistered = true;
    }

    private static void OnClientConnected() => Console.Print("Connected to host, type any text to send.");

    private static void OnClientDisconnected()
    {
        Console.Print("Disconnected from host, cleaning up...");
        PlayerSync.Clear();
    }

    private static IEnumerator OnServerConnected(NetworkConnectionToClient conn)
    {
        while (!conn.isReady)
            yield return null;
 
        World.UnloadWorld();
        ChunkSync.SendSaveChunks(conn, Save.Inst);
        World.LoadWorld();
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(networkPrefab));

        // Send authoritative player state to the new client
        PlayerSync.SendPlayerData(conn);
        PlayerSync.SendConnectionId(conn);
    }

    private static bool IsPortInUse(int port)
    {
        try { IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties(); foreach (var endpoint in properties.GetActiveUdpListeners()) if (endpoint.Port == port) return true; foreach (var endpoint in properties.GetActiveTcpListeners()) if (endpoint.Port == port) return true; }
        catch { }
        return false;
    }

}
