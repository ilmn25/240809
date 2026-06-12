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
    private static bool _disconnecting;
    private static readonly Action<NetworkConnectionToClient> serverConnectedHandler = conn => NetworkManager.singleton.StartCoroutine(OnServerConnected(conn));
    private static GameObject networkPrefab;
    static Server()
    {
        networkPrefab = Resources.Load<GameObject>("Prefab/Network");
        NetworkClient.RegisterPrefab(networkPrefab);
        NetworkManager.singleton.spawnPrefabs.Add(networkPrefab);
    }

    public static IEnumerator StartHost()
    {
        if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        if (Save.Inst == null)
            Save.Inst = new Save(GenType.SuperFlat);

        // Fade to black first (Save.Inst must exist so Environment.Update()
        // actually transitions). The fade back in happens naturally in
        // Scene.Update() when _hostFirstGenDone sets Environment.Target = Null.
        Environment.Target = EnvironmentType.Black;
        yield return new WaitForSeconds(2f);

        PortTransport transport = Transport.active as PortTransport;
        int port = transport != null ? transport.Port : DefaultHostPort;
        while (IsPortInUse(port)) port++;
        if (transport != null) transport.Port = (ushort)port;

        NetworkManager.singleton.StartHost();
        Scene.LoadWorld();
        RegisterHandlers();
        NetworkManager.singleton.StartCoroutine(ChunkBatchLoop());
    }

    public static IEnumerator StartClient(string address = null)
    {
        if (NetworkServer.active)
            StopHost();

        // Ensure Save.Inst exists so Environment.Update() actually transitions.
        if (Save.Inst == null)
            Save.Inst = new Save(GenType.SuperFlat);

        // Fade to black first, then connect to the server.
        Environment.Target = EnvironmentType.Black;
        yield return new WaitForSeconds(2f);

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
        // Unregister first to prevent double-subscription if called again
        // (e.g. after a disconnect that didn't run UnregisterHandlers)
        NetworkClient.OnConnectedEvent -= OnClientConnected;
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
        Chat.RegisterHandlers();
        ChunkSync.RegisterHandlers();
        EntitySync.RegisterHandlers();
        PlayerSync.RegisterHandlers();
        StorageSync.RegisterHandlers();
        DropSync.RegisterHandlers();
        EffectSync.RegisterHandlers();
        ProjectileSync.RegisterHandlers();
        NavMapSync.RegisterHandlers();
        NetworkServer.OnConnectedEvent += serverConnectedHandler;
        NetworkServer.OnDisconnectedEvent += PlayerSync.OnServerDisconnected;
        handlersRegistered = true;
    }

    private static void OnClientConnected()
    {
        Console.Print("Connected to host");
        GUIMain.GUIMenu.Show(false);
        GUIMain.GUIHostMenu.Show(false);
        GUIMain.GUILoad.Show(false);
    }

    private static void OnClientDisconnected()
    {
        if (_disconnecting) return;
        _disconnecting = true;
        Console.Print("Disconnected from host");

        // Clear HUD text, then switch to menu — stops game logic updates.
        // Environment.Update() still runs (called before the SceneMode guard in Main.Update()),
        // so the fade continues.
        if (Main.GUIHudText != null) Main.GUIHudText.text = "";
        GUIMain.OnGameEnd();
        Main.SceneMode = SceneMode.Menu;

        // Reset client sync state immediately so a fast reconnect doesn't
        // inherit stale state (e.g. _clientSceneInitialized still true from
        // the previous session, which would block TryInitializeScene).
        PlayerSync.Clear();

        // Fade to black, then clean up players/world after it completes
        _ = new CoroutineTask(DisconnectCleanup());
    }

    private static IEnumerator DisconnectCleanup()
    {
        Environment.Target = EnvironmentType.Black;
        yield return new WaitForSeconds(2f);

        ChunkSync.Clear();
        DropSync.Clear();
        EffectSync.Clear();
        PlayerSync.Clear();
        StorageSync.Clear();
        handlersRegistered = false;
        Scene.Busy = false;
        World.UnloadWorld();

        // Reset environment to DaySnow (menu default) so a future game
        // starts from a clean state — avoids transitioning to whatever
        // Save.Inst.weather happened to be.
        // Target must also be reset so Environment.Update() doesn't
        // immediately transition back to the previous target (e.g. Black).
        Environment.SetStartEnvironment(EnvironmentType.DaySnow);
        Environment.Target = EnvironmentType.DaySnow;
        _disconnecting = false;

        // Show main menu after disconnect
        GUIMain.GUIMenu.Show(true);
    }

    private static IEnumerator OnServerConnected(NetworkConnectionToClient conn)
    {
        // Reject join until the host has finished initial world gen
        if (Scene.Busy)
        {
            conn.Disconnect();
            yield break;
        }

        while (!conn.isReady)
            yield return null;
 
        World.UnloadWorld();
        ChunkSync.SendSaveChunks(conn, Save.Inst);
        World.LoadWorld();
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(networkPrefab));

        PlayerSync.SendConnectionId(conn);

        // Notify all clients
        int userId = conn.connectionId + 1;
        NetworkServer.SendToAll(new ServerToClientTextMessage { text = $"User {userId} connected" });
    }

    private static IEnumerator ChunkBatchLoop()
    {
        var wait = new WaitForSeconds(2f);
        while (NetworkServer.active)
        {
            yield return wait;
            if (Gen.PendingNewChunks.Count == 0) continue;
            var batch = new List<Vector3Int>(Gen.PendingNewChunks);
            Gen.PendingNewChunks.Clear();
            ChunkSync.SendChunkBatchToAll(batch);
        }
    }

    private static bool IsPortInUse(int port)
    {
        try { IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties(); foreach (var endpoint in properties.GetActiveUdpListeners()) if (endpoint.Port == port) return true; foreach (var endpoint in properties.GetActiveTcpListeners()) if (endpoint.Port == port) return true; }
        catch { }
        return false;
    }

}
