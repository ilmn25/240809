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

        ScreenFade.FadeOut(0.5f);
        yield return new WaitForSeconds(0.7f);

        PortTransport transport = Transport.active as PortTransport;
        int port = transport != null ? transport.Port : DefaultHostPort;
        while (IsPortInUse(port)) port++;
        if (transport != null) transport.Port = (ushort)port;

        NetworkManager.singleton.StartHost();
        Scene.LoadWorld();
        RegisterHandlers();
    }

    public static IEnumerator StartClient(string address = null)
    {
        if (NetworkServer.active)
            StopHost();

        // Clear any previous save so Save.Inst stays null until the host's full save arrives.
        // A non-null (placeholder or stale-from-previous-host) save lets PlayerSync process
        // messages during the transfer and fire TryInitializeScene prematurely, locking the
        // client out of Game mode (no HUD / control). Matters when leaving one host and
        // joining another.
        Save.Inst = null;

        ScreenFade.FadeOut(0.5f);
        yield return new WaitForSeconds(0.7f);

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
        NetworkClient.UnregisterHandler<HostToClientFullSaveMessage>();
        NetworkClient.UnregisterHandler<HostToClientWorldSwitchMessage>();
        NetworkClient.UnregisterHandler<BatchEntityInfoMessage>();
        NetworkClient.UnregisterHandler<PlayerSyncMessage>();
        DropSync.UnregisterHandlers();
        NetworkClient.UnregisterHandler<YourConnectionIdMessage>();
        NetworkServer.UnregisterHandler<ClientToServerPlayerMessage>();
        NetworkServer.UnregisterHandler<PlayerRestMessage>();
        LampSync.UnregisterHandlers();
        BarrelSync.UnregisterHandlers();
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
        SaveSync.RegisterHandlers();
        EntitySync.RegisterHandlers();
        ItemSync.RegisterHandlers();
        PlayerSync.RegisterHandlers();
        StorageSync.RegisterHandlers();
        DropSync.RegisterHandlers();
        EffectSync.RegisterHandlers();
        ProjectileSync.RegisterHandlers();
        NavMapSync.RegisterHandlers();
        LampSync.RegisterHandlers();
        BarrelSync.RegisterHandlers();
        NetworkServer.OnConnectedEvent += serverConnectedHandler;
        NetworkServer.OnDisconnectedEvent += PlayerSync.OnServerDisconnected;
        handlersRegistered = true;
    }

    private static void OnClientConnected()
    {
        Console.Print("Connected to host");
        GUIMain.GUIMenu.Show(false);
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
        ScreenFade.FadeOut(0.5f);
        yield return new WaitForSeconds(0.7f);

        SaveSync.Clear();
        DropSync.Clear();
        EffectSync.Clear();
        PlayerSync.Clear();
        StorageSync.Clear();
        ItemSync.Clear();
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

        // Show main menu after disconnect — fade back in so the screen isn't
        // left black (QuitToMenu's FadeIn is overridden by this coroutine's FadeOut).
        GUIMain.GUIMenu.Show(true);
        ScreenFade.FadeIn(1f, 0f);
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
 
        EntityStaticLoad.SnapshotToChunks();
        SaveSync.SendFullSave(conn, Save.Inst);
        ItemSync.SendActiveItems(conn);
        EntityStaticLoad.LoadActiveChunks();
        World.LoadWorld();
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(networkPrefab));

        PlayerSync.SendConnectionId(conn);

        // Notify all clients
        int userId = conn.connectionId + 1;
        NetworkServer.SendToAll(new ServerToClientTextMessage { text = $"User {userId} connected" });
    }

    private static bool IsPortInUse(int port)
    {
        try { IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties(); foreach (var endpoint in properties.GetActiveUdpListeners()) if (endpoint.Port == port) return true; foreach (var endpoint in properties.GetActiveTcpListeners()) if (endpoint.Port == port) return true; }
        catch { }
        return false;
    }

}
