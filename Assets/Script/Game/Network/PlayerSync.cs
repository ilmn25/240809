using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// ── Message structs ─────────────────────────────────────────────────────

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
    public bool resting;

    public Vector3 direction;
    public bool isGrounded;
    public float speedCurrent;
    public float speedTarget;
    public bool faceTarget;
    public Vector3 targetScreenDir;
    public int charSprite;

    public int equipmentId;
    public int equipmentDurability;

    public int animTrigger;
    public float animNormalizedTime;
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

    public int animTrigger;
    public float animNormalizedTime;

    public string destroyUid;        // item the client destroyed (pickup)

    public int selectedSlot;         // inventory slot index (-1 = none) the client has selected
}

/// <summary>Client asks the host to set a player's resting state.</summary>
public struct PlayerRestMessage : NetworkMessage
{
    public string uid;
    public bool resting;
}

// Storage sync moved to StorageSyncMessage (see StorageSync.cs)

public static class PlayerSync
{
    #region Fields

    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();
    public static float BroadcastInterval = 0.025f;
    public static float ClientSendInterval => BroadcastInterval;

    /// <summary>Host: uid → connectionId. Free = no entry / -1, host = 0, remote = ≥1.
    /// Client: same, populated from broadcast.</summary>
    internal static readonly Dictionary<string, int> PlayerControllers = new Dictionary<string, int>();

    /// <summary>Client: my own connection ID (set by host on connect). -1 until set.</summary>
    public static int MyConnectionId => _myConnectionId;
    private static int _myConnectionId = -1;
    private static bool _clientSceneInitialized = false;

    /// <summary>Client-side: tracks last received animation hash per player uid.</summary>
    private static readonly Dictionary<string, int> _lastPlayerAnimHash = new Dictionary<string, int>();

    /// <summary>Host-side: remote client anim triggers queued for forwarding in next broadcast.</summary>
    private static readonly Dictionary<string, int> _pendingForwardAnimTriggers = new Dictionary<string, int>();
    private static readonly Dictionary<string, float> _pendingForwardAnimTimes = new Dictionary<string, float>();

    /// <summary>Client sets this when picking up an item; batch loop reads and clears it.</summary>
    private static string _pendingDestroyUid = "";
    public static void SetPendingDestroyUid(string uid) { _pendingDestroyUid = uid; }

    private class PendingUnload { public string uid; public int id; public Vector3 pos; }
    private static readonly List<PendingUnload> _pendingUnloads = new List<PendingUnload>();

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Controller queries
    // ═══════════════════════════════════════════════════════════════

    #region Controller Queries

    /// <summary>Host-side: is this player controlled by a remote client? (host skips input/movement for those)</summary>
    public static bool IsClaimedByRemoteClient(string uid)
    {
        if (!Helper.IsHost() || string.IsNullOrEmpty(uid)) return false;
        if (Info.Dictionary.TryGetValue(uid, out Info info))
            return info.controllerId != 0 && info.controllerId != -1;
        return false;
    }

    /// <summary>Client-side: can WE control this player? (free, or already ours)</summary>
    public static bool CanLocalClientControl(string uid)
    {
        if (Helper.IsHost() || string.IsNullOrEmpty(uid)) return false;
        if (Info.Dictionary.TryGetValue(uid, out Info info))
            return info.controllerId == -1 || info.controllerId == _myConnectionId;
        return false;
    }

    /// <summary>Host: register the player we're controlling (only if free).</summary>
    public static void HostClaimPlayer(string uid)
    {
        if (!NetworkServer.active || string.IsNullOrEmpty(uid)) return;
        int ctrl = PlayerControllers.GetValueOrDefault(uid, -1);
        if (ctrl == -1 || ctrl == 0)
        {
            PlayerControllers[uid] = 0;
            if (Info.Dictionary.TryGetValue(uid, out Info info))
            {
                info.controllerId = 0;
                info.ownerId = 0;
            }
            ResetPlayerAnimatorToIdle(uid);
        }
    }

    /// <summary>Host: release the player we were controlling.</summary>
    public static void HostReleasePlayer(string uid)
    {
        if (!NetworkServer.active || string.IsNullOrEmpty(uid)) return;
        if (PlayerControllers.GetValueOrDefault(uid, -1) == 0)
        {
            PlayerControllers.Remove(uid);
            if (Info.Dictionary.TryGetValue(uid, out Info info))
                info.controllerId = -1;
            ResetPlayerAnimatorToIdle(uid);
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Registration & connection management
    // ═══════════════════════════════════════════════════════════════

    #region Registration

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<PlayerSyncMessage>(OnPlayerSyncMessageReceived, false);
        NetworkServer.ReplaceHandler<ClientToServerPlayerMessage>(OnClientToServerPlayerMessageReceived, false);
        NetworkServer.ReplaceHandler<PlayerRestMessage>(OnPlayerRestMessageReceived, false);
        NetworkClient.ReplaceHandler<YourConnectionIdMessage>(OnYourConnectionIdReceived, false);
        if (Application.isPlaying)
        {
            _ = new CoroutineTask(BatchLoop());
            _ = new CoroutineTask(ClientBatchLoop());
        }
    }

    /// <summary>Host sends a client its own connectionId so it can check the broadcast.</summary>
    public static void SendConnectionId(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;
        conn.Send(new YourConnectionIdMessage { connectionId = conn.connectionId });
    }

    /// <summary>Clean up when a remote client disconnects — release ALL players they controlled.</summary>
    public static void OnServerDisconnected(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active || conn == null) return;
        List<string> toRelease = new List<string>();
        foreach (var kv in PlayerControllers)
            if (kv.Value == conn.connectionId) toRelease.Add(kv.Key);
        foreach (var uid in toRelease)
        {
            PlayerControllers.Remove(uid);
            if (Info.Dictionary.TryGetValue(uid, out Info info))
            {
                info.controllerId = -1;
                info.ownerId = 0;
            }
            ResetPlayerAnimatorToIdle(uid);
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Host: send
    // ═══════════════════════════════════════════════════════════════

    #region Host Send

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
            // Send initial storage for this player (StorageSync only sends on modification)
            if (player.Storage?.List != null)
            {
                conn.Send(new StorageSyncMessage
                {
                    entityUid = player.uid,
                    storageData = Helper.SerializeObject(player.Storage.List)
                });
            }
        }
    }

    private static PlayerSyncMessage BuildMessage(int index, PlayerInfo player, bool destroyed)
    {
        var (animTrigger, animNormalizedTime) = ResolveAnimState(player, destroyed);

        return new PlayerSyncMessage
        {
            playerIndex = index, uid = player.uid, id = (int)player.id,
            position = player.position, destroyed = destroyed,
            controllingClientId = PlayerControllers.GetValueOrDefault(player.uid, -1),
            health = player.Health, healthMax = player.HealthMax,
            mana = player.Mana, sanity = player.Sanity,
            hunger = player.Hunger, hungerMax = player.HungerMax,
            stamina = player.Stamina, playerStatus = (int)player.PlayerStatus,
            resting = player.Resting,
            direction = player.Direction, isGrounded = player.IsGrounded,
            speedCurrent = player.SpeedCurrent, speedTarget = player.SpeedTarget,
            faceTarget = player.FaceTarget, targetScreenDir = ScreenToWorldAligned(player.TargetScreenDir),
            charSprite = (int)player.CharSprite,
            equipmentId = player.Equipment != null ? (int)player.Equipment.ID : 0,
            equipmentDurability = player.Equipment?.Durability ?? 0,
            animTrigger = animTrigger,
            animNormalizedTime = animNormalizedTime
        };
    }

    /// <summary>
    /// For remote clients: use pending trigger queued from ClientToServerPlayerMessage.
    /// For host-controlled: read directly from Animator.
    /// </summary>
    private static (int trigger, float time) ResolveAnimState(PlayerInfo player, bool destroyed)
    {
        if (destroyed) return (0, 0f);

        if (_pendingForwardAnimTriggers.TryGetValue(player.uid, out int pendingTrigger))
        {
            float time = _pendingForwardAnimTimes.GetValueOrDefault(player.uid, 0f);
            _pendingForwardAnimTriggers.Remove(player.uid);
            _pendingForwardAnimTimes.Remove(player.uid);
            return (pendingTrigger, time);
        }

        return ReadAnimatorState(player);
    }

    /// <summary>Read current Animator shortNameHash and normalizedTime.</summary>
    private static (int trigger, float time) ReadAnimatorState(PlayerInfo player)
    {
        if (player.Animator != null && player.Animator.isActiveAndEnabled)
        {
            AnimatorStateInfo state = player.Animator.GetCurrentAnimatorStateInfo(0);
            return (state.shortNameHash, state.normalizedTime);
        }
        return (0, 0f);
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

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Client: send
    // ═══════════════════════════════════════════════════════════════

    #region Client Send

    /// <summary>Client asks the host to set a player's resting state.</summary>
    public static void SendRest(string uid, bool resting)
    {
        if (Helper.IsHost() || !NetworkClient.isConnected || string.IsNullOrEmpty(uid)) return;
        NetworkClient.Send(new PlayerRestMessage { uid = uid, resting = resting });
    }

    public static void SendClientPlayerBatch()
    {
        if (Helper.IsHost() || !NetworkClient.isConnected || Main.PlayerInfo?.Machine == null) return;

        var p = Main.PlayerInfo;
        var (animTrigger, animNormalizedTime) = ReadAnimatorState(p);
        string destroyUid = _pendingDestroyUid;
        _pendingDestroyUid = "";
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
            targetScreenDir = ScreenToWorldAligned(p.TargetScreenDir),
            aimPosition = p.AimPosition,
            animTrigger = animTrigger,
            animNormalizedTime = animNormalizedTime,
            destroyUid = destroyUid,
            selectedSlot = p.Storage != null ? p.Storage.Key : -1
        });
    }

    private static IEnumerator ClientBatchLoop()
    {
        while (true) { yield return new WaitForSeconds(ClientSendInterval); if (NetworkClient.isConnected) SendClientPlayerBatch(); }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Client: receive (handlers run on remote clients only)
    // ═══════════════════════════════════════════════════════════════

    #region Client Receive

    private static void OnYourConnectionIdReceived(YourConnectionIdMessage msg)
    {
        _myConnectionId = msg.connectionId;
    }

    private static void OnPlayerSyncMessageReceived(PlayerSyncMessage msg)
    {
        if (Helper.IsHost()) return;
        if (Save.Inst == null || Scene.Busy) return;

        if (msg.destroyed)
        {
            HandleDestroyedPlayer(msg);
            return;
        }

        PlayerControllers[msg.uid] = msg.controllingClientId;
        if (InfoMap.TryGetValue(msg.uid, out Info syncInfo))
            syncInfo.controllerId = msg.controllingClientId;

        if (!InfoMap.TryGetValue(msg.uid, out Info existing))
            HandleNewPlayer(msg);
        else if (existing is PlayerInfo pi)
            HandleExistingPlayer(msg, pi);

        // Client handles its own animation — skip host broadcast for players we control.
        // Free players (controllerId = -1) still receive animation from host broadcast.
        bool isControlledByUs = !Helper.IsHost() && syncInfo != null && syncInfo.controllerId == _myConnectionId;
        if (!isControlledByUs)
            HandleAnimationTrigger(msg, existing);
        TryInitializeScene(msg);
    }

    private static void HandleDestroyedPlayer(PlayerSyncMessage msg)
    {
        PlayerControllers.Remove(msg.uid);
        if (InfoMap.TryGetValue(msg.uid, out Info dead))
        {
            EntitySync.InfoMap.Remove(msg.uid);
            if (dead.Machine != null) ((EntityMachine)dead.Machine).Unload();
            InfoMap.Remove(msg.uid);
        }
    }

    private static void HandleNewPlayer(PlayerSyncMessage msg)
    {
        PlayerInfo pi = (Save.Inst != null && msg.playerIndex >= 0 && msg.playerIndex < Save.Inst.players.Count)
            ? Save.Inst.players[msg.playerIndex]
            : (PlayerInfo)Entity.CreateInfo((ID)msg.id, msg.position);
        pi.uid = msg.uid;
        pi.position = msg.position;
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

    private static void HandleExistingPlayer(PlayerSyncMessage msg, PlayerInfo pi)
    {
        // Only skip position/animation sync for players we control (provide input for).
        // Free/owner-only players (controllerId = -1) still receive full sync from host.
        bool isControlledByUs = !Helper.IsHost() && pi.controllerId == _myConnectionId;
        if (isControlledByUs)
        {
            // Local player: only sync server-authoritative fields (stats).
            // Inventory is client-authoritative — don't overwrite with host broadcast.
            pi.Health = msg.health; pi.HealthMax = msg.healthMax;
            pi.Mana = msg.mana; pi.Sanity = msg.sanity;
            pi.Hunger = msg.hunger; pi.HungerMax = msg.hungerMax;
            pi.Stamina = msg.stamina;
            pi.PlayerStatus = (PlayerStatus)msg.playerStatus;
            pi.Resting = msg.resting;
            pi.CharSprite = (ID)msg.charSprite;
        }
        else
        {
            // Remote or free player: full state sync
            pi.position = msg.position;
            CopyAll(pi, msg);
            if (pi.Machine != null) pi.Machine.transform.position = msg.position;
        }
    }

    private static void HandleAnimationTrigger(PlayerSyncMessage msg, Info existing)
    {
        if (msg.animTrigger != 0 && existing is PlayerInfo piAnim)
        {
            int prevHash = _lastPlayerAnimHash.GetValueOrDefault(msg.uid, 0);
            if (msg.animTrigger != prevHash && piAnim.Animator != null && piAnim.Animator.isActiveAndEnabled)
            {
                piAnim.Animator.Play(msg.animTrigger, 0, msg.animNormalizedTime);
            }
            _lastPlayerAnimHash[msg.uid] = msg.animTrigger;
        }
        else
        {
            _lastPlayerAnimHash.Remove(msg.uid);
        }
    }

    /// <summary>Remote client: ensure scene is active after the world has loaded.</summary>
    private static void TryInitializeScene(PlayerSyncMessage msg)
    {
        if (!_clientSceneInitialized && msg.playerIndex == 0 && !Scene.Busy)
        {
            _clientSceneInitialized = true;
            Main.SceneMode = SceneMode.Game;
            Environment.Target = EnvironmentType.Null;
            ScreenFade.FadeIn(1f, 2f);
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Host: receive (handlers run on host only)
    // ═══════════════════════════════════════════════════════════════

    #region Host Receive

    private static void OnClientToServerPlayerMessageReceived(NetworkConnectionToClient _, ClientToServerPlayerMessage msg)
    {
        if (!NetworkServer.active || Save.Inst == null) return;
        if (msg.playerIndex < 0 || msg.playerIndex >= Save.Inst.players.Count) return;

        PlayerInfo targetPlayer = Save.Inst.players[msg.playerIndex];
        if (targetPlayer.Machine == null) return;

        if (!TryClaimPlayer(_, targetPlayer)) return;

        ApplyClientState(targetPlayer, msg);
        QueueForwardAnim(targetPlayer, msg);

        // Client-authoritative: equipment (held item) — by slot index so the server
        // reads the actual ItemSlot from storage, letting ReferenceEquals short-circuit.
        // This mirrors Storage.NotifyChanged() but without its side effects.
        // Also sync Storage.Key so HostClaimPlayer → SetPlayer → SyncCurrentItemState
        // reads from the correct slot and doesn't clear the equipment.
        var storage = targetPlayer.Storage;
        ItemSlot equipped = storage != null && (uint)msg.selectedSlot < (uint)storage.List.Count
            ? storage.List[msg.selectedSlot]
            : null;
        targetPlayer.SetEquipment(equipped is { Stack: > 0 } ? equipped : null);
        if (storage != null && msg.selectedSlot >= 0)
            storage.Key = msg.selectedSlot;

        // Client-authoritative: destroy item on host (pickup)
        if (!string.IsNullOrEmpty(msg.destroyUid) && Info.Dictionary.TryGetValue(msg.destroyUid, out Info target))
            target.Destroy();

        // If the host is spectating a player that just became free, auto-claim it
        TryHostClaimCurrentPlayer();
    }

    private static void OnPlayerRestMessageReceived(NetworkConnectionToClient _, PlayerRestMessage msg)
    {
        if (!NetworkServer.active || Save.Inst == null) return;
        if (!Info.Dictionary.TryGetValue(msg.uid, out Info info) || info is not PlayerInfo pi) return;
        pi.Resting = msg.resting;
    }

    /// <summary>If the host's current player is unclaimed, claim it so clients don't think it's free.</summary>
    private static void TryHostClaimCurrentPlayer()
    {
        if (!Helper.IsHost() || Main.PlayerInfo == null) return;
        string currentUid = Main.PlayerInfo.uid;
        int ctrl = PlayerControllers.GetValueOrDefault(currentUid, -1);
        if (ctrl == -1)
            HostClaimPlayer(currentUid);
    }

    /// <summary>Claim the player if free; returns false if another connection already controls it.
    /// Sets both controller and owner so AI runs on the claiming client.
    /// Releases any previous claim from this connection (client tab-switched).</summary>
    private static bool TryClaimPlayer(NetworkConnectionToClient conn, PlayerInfo targetPlayer)
    {
        int currentCtrl = PlayerControllers.GetValueOrDefault(targetPlayer.uid, -1);
        if (currentCtrl != -1 && currentCtrl != conn.connectionId)
            return false;

        if (currentCtrl == -1)
        {
            // Release any previous claim from this connection (client tab-switched to another player)
            string oldUid = null;
            foreach (var kv in PlayerControllers)
                if (kv.Value == conn.connectionId) { oldUid = kv.Key; break; }
            if (oldUid != null)
            {
                PlayerControllers.Remove(oldUid);
                if (Info.Dictionary.TryGetValue(oldUid, out Info oldInfo))
                {
                    oldInfo.controllerId = -1;
                    oldInfo.ownerId = 0;
                }
                ResetPlayerAnimatorToIdle(oldUid);
            }

            PlayerControllers[targetPlayer.uid] = conn.connectionId;
            targetPlayer.controllerId = conn.connectionId;
            targetPlayer.ownerId = conn.connectionId;
        }
        return true;
    }

    private static void ApplyClientState(PlayerInfo targetPlayer, ClientToServerPlayerMessage msg)
    {
        targetPlayer.position = msg.position;
        targetPlayer.Machine.transform.position = msg.position;
        targetPlayer.Direction = msg.direction;
        targetPlayer.IsGrounded = msg.isGrounded;
        targetPlayer.SpeedCurrent = msg.speedCurrent;
        targetPlayer.SpeedTarget = msg.speedTarget;
        targetPlayer.FaceTarget = msg.faceTarget;
        targetPlayer.TargetScreenDir = WorldAlignedToScreen(msg.targetScreenDir);
        targetPlayer.AimPosition = msg.aimPosition;
    }

    private static void QueueForwardAnim(PlayerInfo targetPlayer, ClientToServerPlayerMessage msg)
    {
        if (msg.animTrigger != 0)
        {
            _pendingForwardAnimTriggers[targetPlayer.uid] = msg.animTrigger;
            _pendingForwardAnimTimes[targetPlayer.uid] = msg.animNormalizedTime;
            // Host client ignores PlayerSync broadcast (Helper.IsHost return),
            // so apply the animation directly here for the host's visual.
            if (Helper.IsHost() && targetPlayer.Animator != null && targetPlayer.Animator.isActiveAndEnabled)
            {
                targetPlayer.Animator.speed = 1f;
                targetPlayer.Animator.Play(msg.animTrigger, 0, msg.animNormalizedTime);
            }
        }
    }

    // Storage sync moved to StorageSync (see StorageSync.cs)

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════

    #region Helpers

    private static Vector3 ScreenToWorldAligned(Vector3 screenDir) => EntitySync.ScreenToWorldAligned(screenDir);
    private static Vector3 WorldAlignedToScreen(Vector3 worldDir) => EntitySync.WorldAlignedToScreen(worldDir);

    private static void CopyAll(PlayerInfo pi, PlayerSyncMessage msg)
    {
        pi.Health = msg.health; pi.HealthMax = msg.healthMax;
        pi.Mana = msg.mana; pi.Sanity = msg.sanity;
        pi.Hunger = msg.hunger; pi.HungerMax = msg.hungerMax;
        pi.Stamina = msg.stamina;
        pi.PlayerStatus = (PlayerStatus)msg.playerStatus;
        pi.Resting = msg.resting;
        pi.Direction = msg.direction; pi.IsGrounded = msg.isGrounded;
        pi.SpeedCurrent = msg.speedCurrent; pi.SpeedTarget = msg.speedTarget;
        pi.FaceTarget = msg.faceTarget; pi.TargetScreenDir = WorldAlignedToScreen(msg.targetScreenDir);

        // Equipment sync (visual held item) — only apply when the values actually changed
        // to avoid re-setting the sprite/position every 25ms broadcast.
        int curId = pi.Equipment != null ? (int)pi.Equipment.ID : 0;
        int curDur = pi.Equipment?.Durability ?? 0;
        if (msg.equipmentId != curId || msg.equipmentDurability != curDur)
        {
            if (msg.equipmentId > 0)
            {
                var slot = new ItemSlot((ID)msg.equipmentId, 1);
                slot.Durability = msg.equipmentDurability;
                pi.SetEquipment(slot);
            }
            else
            {
                pi.SetEquipment(null);
            }
        }
    }

    /// <summary>
    /// Reset a player's Animator to the EquipIdle state.
    /// Called when a player's controller is released (host release, remote client disconnect/swap)
    /// to prevent the Animator from being stuck playing a non-idle clip that was set by QueueForwardAnim.
    /// </summary>
    private static void ResetPlayerAnimatorToIdle(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;
        if (!InfoMap.TryGetValue(uid, out Info info) || info is not PlayerInfo pi) return;
        if (pi.Animator == null || !pi.Animator.isActiveAndEnabled) return;
        pi.Animator.speed = 1f;
        pi.Animator.Play("EquipIdle", 0, 0f);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    //  Cleanup
    // ═══════════════════════════════════════════════════════════════

    #region Cleanup

    public static void Clear()
    {
        foreach (var kv in InfoMap)
        {
            EntitySync.InfoMap.Remove(kv.Key);
            if (kv.Value.Machine != null) ((EntityMachine)kv.Value.Machine).Unload();
        }
        InfoMap.Clear();
        _pendingUnloads.Clear();
        PlayerControllers.Clear();
        _myConnectionId = -1;
        _clientSceneInitialized = false;
        _lastPlayerAnimHash.Clear();
        _pendingForwardAnimTriggers.Clear();
        _pendingForwardAnimTimes.Clear();
        EntitySync.Clear();
    }

    #endregion
}
