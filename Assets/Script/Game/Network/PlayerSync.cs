using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Host broadcasts all players.  Client sends its controlled player.
/// controllingClientId: -1 = free, 0 = host, &gt;0 = remote client connection ID.
/// All devices see the same control state because it's synced in the broadcast.
/// </summary>
public struct PlayerSyncMessage : NetworkMessage
{
    public int playerIndex;
    public string uid;
    public int id;
    public Vector3 position;
    public bool destroyed;
    public int controllingClientId;  // -1 = free, 0 = host, >0 = remote client

    public int health;
    public int healthMax;
    public float mana;
    public float sanity;
    public int hunger;
    public int hungerMax;
    public float stamina;
    public int playerStatus;

    public Vector3 direction;
    public bool isGrounded;
    public float speedCurrent;
    public float speedTarget;
    public bool faceTarget;
    public Vector3 targetScreenDir;
    public int charSprite;

    public int equipmentId;
    public int equipmentDurability;
    public byte[] storageData;
}

public struct YourConnectionIdMessage : NetworkMessage
{
    public int connectionId;
}

public struct ClientToServerPlayerMessage : NetworkMessage
{
    public int playerIndex;          // -1 = no claim
    public string uid;
    public Vector3 position;
    public Vector3 direction;
    public bool isGrounded;
    public float speedCurrent;
    public float speedTarget;
    public bool faceTarget;
    public Vector3 targetScreenDir;
    public Vector3 aimPosition;
    public string targetUid;
    public int actionType;
}

public static class PlayerSync
{
    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();
    public static float BroadcastInterval = 0.025f;
    public static float ClientSendInterval => BroadcastInterval;

    /// <summary>Host: uid → connectionId. Free = no entry / -1, host = 0, remote = ≥1.
    /// Client: same, populated from broadcast.</summary>
    private static readonly Dictionary<string, int> _playerControllers = new Dictionary<string, int>();

    /// <summary>Client: my own connection ID (set by host on connect). -1 until set.</summary>
    private static int _myConnectionId = -1;

    private static bool _clientSceneInitialized = false;

    /// <summary>
    /// On host: is this uid controlled by a remote client?  (host skips movement for those)
    /// On client: can WE control this uid?  (yes if free, or host confirmed our claim)
    /// </summary>
    public static bool IsClientControlled(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return false;
        if (Helper.IsHost())
            return _playerControllers.GetValueOrDefault(uid, -1) >= 1;
        else
        {
            int controller = _playerControllers.GetValueOrDefault(uid, -1);
            return controller == -1 || controller == _myConnectionId;
        }
    }

    /// <summary>Client calls this when pressing Tab.</summary>
    public static void NotifyClientClaim(int playerIndex) { }

    /// <summary>Host: register the player we're controlling (only if free).</summary>
    public static void HostClaimPlayer(string uid)
    {
        if (!NetworkServer.active || string.IsNullOrEmpty(uid)) return;
        int owner = _playerControllers.GetValueOrDefault(uid, -1);
        if (owner == -1 || owner == 0)
            _playerControllers[uid] = 0;
    }

    /// <summary>Host: release the player we were controlling.</summary>
    public static void HostReleasePlayer(string uid)
    {
        if (!NetworkServer.active || string.IsNullOrEmpty(uid)) return;
        if (_playerControllers.GetValueOrDefault(uid, -1) == 0)
            _playerControllers.Remove(uid);
    }

    private class PendingUnload { public string uid; public int id; public Vector3 pos; }
    private static readonly List<PendingUnload> _pendingUnloads = new List<PendingUnload>();

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<PlayerSyncMessage>(OnPlayerSyncMessageReceived, false);
        NetworkServer.ReplaceHandler<ClientToServerPlayerMessage>(OnClientToServerPlayerMessageReceived, false);
        NetworkClient.ReplaceHandler<YourConnectionIdMessage>(OnYourConnectionIdReceived, false);
        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
            _ = new CoroutineTask(ClientBatchLoop());
        }
    }

    // ── Host sends all players ─────────────────────────────────────

    /// <summary>Host sends a client its own connectionId so it can check the broadcast.</summary>
    public static void SendConnectionId(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;
        conn.Send(new YourConnectionIdMessage { connectionId = conn.connectionId });
    }

    public static void BroadcastPlayerUnload(Info info)
    {
        if (!Helper.IsHost() || !NetworkServer.active || info == null) return;
        _pendingUnloads.Add(new PendingUnload { uid = info.uid, id = (int)info.id, pos = info.position });
    }

    public static void SendPlayerData(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null || Save.Inst == null) return;
        for (int i = 0; i < Save.Inst.players.Count; i++)
        {
            PlayerInfo player = Save.Inst.players[i];
            if (player.Machine == null) continue;
            conn.Send(BuildMessage(i, player, false));
        }
    }

    private static PlayerSyncMessage BuildMessage(int index, PlayerInfo player, bool destroyed)
    {
        byte[] storageBytes = !destroyed && player.Storage?.List != null
            ? Helper.SerializeObject(player.Storage.List) : null;
        return new PlayerSyncMessage
        {
            playerIndex = index, uid = player.uid, id = (int)player.id,
            position = player.position, destroyed = destroyed,
            controllingClientId = _playerControllers.GetValueOrDefault(player.uid, -1),
            health = player.Health, healthMax = player.HealthMax,
            mana = player.Mana, sanity = player.Sanity,
            hunger = player.Hunger, hungerMax = player.HungerMax,
            stamina = player.Stamina, playerStatus = (int)player.PlayerStatus,
            direction = player.Direction, isGrounded = player.IsGrounded,
            speedCurrent = player.SpeedCurrent, speedTarget = player.SpeedTarget,
            faceTarget = player.FaceTarget, targetScreenDir = player.TargetScreenDir,
            charSprite = (int)player.CharSprite,
            equipmentId = player.Equipment != null ? (int)player.Equipment.ID : 0,
            equipmentDurability = player.Equipment?.Durability ?? 0,
            storageData = storageBytes
        };
    }

    private static void SendPlayerBatch()
    {
        if (!NetworkServer.active || Save.Inst == null) return;
        for (int i = 0; i < Save.Inst.players.Count; i++)
        {
            PlayerInfo player = Save.Inst.players[i];
            if (player.Machine == null) continue;
            NetworkServer.SendToAll(BuildMessage(i, player, false));
        }
        foreach (var pu in _pendingUnloads)
            NetworkServer.SendToAll(new PlayerSyncMessage
            { playerIndex = -1, uid = pu.uid, id = pu.id, position = pu.pos, destroyed = true });
        _pendingUnloads.Clear();
    }

    private static IEnumerator BatchLoop()
    {
        while (true) { yield return new WaitForSeconds(BroadcastInterval); if (NetworkServer.active) SendPlayerBatch(); }
    }

    // ── Client receives its connection ID from host ────────────────

    private static void OnYourConnectionIdReceived(YourConnectionIdMessage msg)
    {
        _myConnectionId = msg.connectionId;
    }

    // ── Client receives host broadcast ─────────────────────────────

    private static void OnPlayerSyncMessageReceived(PlayerSyncMessage msg)
    {
        if (Helper.IsHost()) return;

        if (Save.Inst == null) return;
        if (Scene.Busy) return;

        if (msg.destroyed)
        {
            _playerControllers.Remove(msg.uid);
            if (InfoMap.TryGetValue(msg.uid, out Info dead))
            {
                EntitySync.InfoMap.Remove(msg.uid);
                if (dead.Machine != null) ((EntityMachine)dead.Machine).Unload();
                InfoMap.Remove(msg.uid);
            }
            return;
        }

        // Track who controls this player from the broadcast (used by IsClientControlled)
        _playerControllers[msg.uid] = msg.controllingClientId;

        if (!InfoMap.TryGetValue(msg.uid, out Info existing))
        {
            PlayerInfo pi = (Save.Inst != null && msg.playerIndex >= 0 && msg.playerIndex < Save.Inst.players.Count)
                ? Save.Inst.players[msg.playerIndex]
                : (PlayerInfo)Entity.CreateInfo((ID)msg.id, msg.position);
            pi.uid = msg.uid; pi.position = msg.position;
            CopyAll(pi, msg);
            InfoMap[msg.uid] = pi;

            if (pi.Machine == null)
                Entity.SpawnFromInfo(pi, true);
            else
                pi.Machine.transform.position = msg.position;

            if (msg.playerIndex == 0 && (Main.PlayerInfo == null || Main.PlayerInfo.Machine == null))
                Main.PlayerInfo = pi;

            EntitySync.InfoMap[msg.uid] = pi;
        }
        else if (existing is PlayerInfo pi)
        {
            if (IsClientControlled(msg.uid))
            {
                pi.Health = msg.health; pi.HealthMax = msg.healthMax;
                pi.Mana = msg.mana; pi.Sanity = msg.sanity;
                pi.Hunger = msg.hunger; pi.HungerMax = msg.hungerMax;
                pi.Stamina = msg.stamina;
                pi.PlayerStatus = (PlayerStatus)msg.playerStatus;
                pi.CharSprite = (ID)msg.charSprite;
                CopyStorage(pi, msg);
            }
            else
            {
                pi.position = msg.position;
                CopyAll(pi, msg);
                if (pi.Machine != null) pi.Machine.transform.position = msg.position;
            }
        }

        // Remote client: ensure scene is active after the world has loaded.
        if (!_clientSceneInitialized && msg.playerIndex == 0 && !Scene.Busy)
        {
            _clientSceneInitialized = true;
            Main.SceneMode = SceneMode.Game;
            Environment.Target = EnvironmentType.Null;
        }
    }

    // ── Client sends its controlled player to host ─────────────────

    private static void SendClientPlayerBatch()
    {
        if (Helper.IsHost() || !NetworkClient.isConnected || Main.PlayerInfo?.Machine == null) return;

        var p = Main.PlayerInfo;
        NetworkClient.Send(new ClientToServerPlayerMessage
        {
            playerIndex = Control.CurrentPlayerIndex,
            uid = p.uid,
            position = p.position,
            direction = p.Direction,
            isGrounded = p.IsGrounded,
            speedCurrent = p.SpeedCurrent,
            speedTarget = p.SpeedTarget,
            faceTarget = p.FaceTarget,
            targetScreenDir = p.TargetScreenDir,
            aimPosition = p.AimPosition,
            targetUid = p.Target?.uid ?? "",
            actionType = (int)p.ActionType
        });
    }

    private static IEnumerator ClientBatchLoop()
    {
        while (true) { yield return new WaitForSeconds(ClientSendInterval); if (NetworkClient.isConnected) SendClientPlayerBatch(); }
    }

    /// <summary>Clean up when a remote client disconnects.</summary>
    public static void OnServerDisconnected(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;
        string oldUid = null;
        foreach (var kv in _playerControllers)
            if (kv.Value == conn.connectionId) { oldUid = kv.Key; break; }
        if (oldUid != null)
        {
            Console.Print($"Client {conn.connectionId} disconnected, released player {oldUid}");
            _playerControllers.Remove(oldUid);
        }
    }

    // ── Host receives client's claim / state ───────────────────────

    private static void OnClientToServerPlayerMessageReceived(NetworkConnectionToClient _, ClientToServerPlayerMessage msg)
    {
        if (!NetworkServer.active || Save.Inst == null) return;

        if (msg.playerIndex >= 0 && msg.playerIndex < Save.Inst.players.Count)
        {
            PlayerInfo targetPlayer = Save.Inst.players[msg.playerIndex];
            if (targetPlayer.Machine == null) return;

            // Only take control if the player is free (-1) or already ours
            int currentOwner = _playerControllers.GetValueOrDefault(targetPlayer.uid, -1);
            if (currentOwner != -1 && currentOwner != _.connectionId) return;

            if (currentOwner == -1)
            {
                // Claim the free player: release any previous claim from this connection
                string oldUid = null;
                foreach (var kv in _playerControllers)
                    if (kv.Value == _.connectionId) { oldUid = kv.Key; break; }
                if (oldUid != null) _playerControllers.Remove(oldUid);

                _playerControllers[targetPlayer.uid] = _.connectionId;
            }

            // Always apply the client's state
            targetPlayer.position = msg.position;
            targetPlayer.Machine.transform.position = msg.position;
            targetPlayer.Direction = msg.direction;
            targetPlayer.IsGrounded = msg.isGrounded;
            targetPlayer.SpeedCurrent = msg.speedCurrent;
            targetPlayer.SpeedTarget = msg.speedTarget;
            targetPlayer.FaceTarget = msg.faceTarget;
            targetPlayer.TargetScreenDir = msg.targetScreenDir;
            targetPlayer.AimPosition = msg.aimPosition;
            Info.Dictionary.TryGetValue(msg.targetUid, out Info t);
            targetPlayer.Target = t;
            targetPlayer.ActionType = (IActionType)msg.actionType;
        }
    }

    // ── Copy helpers ───────────────────────────────────────────────

    private static void CopyAll(PlayerInfo pi, PlayerSyncMessage msg)
    {
        pi.Health = msg.health; pi.HealthMax = msg.healthMax;
        pi.Mana = msg.mana; pi.Sanity = msg.sanity;
        pi.Hunger = msg.hunger; pi.HungerMax = msg.hungerMax;
        pi.Stamina = msg.stamina;
        pi.PlayerStatus = (PlayerStatus)msg.playerStatus;
        pi.Direction = msg.direction; pi.IsGrounded = msg.isGrounded;
        pi.SpeedCurrent = msg.speedCurrent; pi.SpeedTarget = msg.speedTarget;
        pi.FaceTarget = msg.faceTarget; pi.TargetScreenDir = msg.targetScreenDir;
        pi.CharSprite = (ID)msg.charSprite;
        CopyStorage(pi, msg);
    }

    private static void CopyStorage(PlayerInfo pi, PlayerSyncMessage msg)
    {
        if (msg.storageData?.Length > 0 && pi.Storage != null)
        {
            var list = Helper.DeserializeObject<List<ItemSlot>>(msg.storageData);
            if (list != null) pi.Storage.List = list;
        }
        if (msg.equipmentId > 0 && pi.Storage != null)
        {
            foreach (var s in pi.Storage.List)
                if ((int)s.ID == msg.equipmentId && s.Stack > 0)
                { s.Durability = msg.equipmentDurability; pi.SetEquipment(s); break; }
        }
        else pi.SetEquipment(null);
    }

    public static void Clear()
    {
        foreach (var kv in InfoMap)
        {
            EntitySync.InfoMap.Remove(kv.Key);
            if (kv.Value.Machine != null) ((EntityMachine)kv.Value.Machine).Unload();
        }
        InfoMap.Clear();
        _pendingUnloads.Clear();
        _playerControllers.Clear();
        _myConnectionId = -1;
        _clientSceneInitialized = false;
    }
}
