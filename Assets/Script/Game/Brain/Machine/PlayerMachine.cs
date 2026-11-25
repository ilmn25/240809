
using UnityEngine;
 
public class PlayerMachine : MobMachine, IActionSecondaryInteract
{
    protected new PlayerInfo Info => GetModule<PlayerInfo>();

    // Party leader the allies follow; null when there is nobody left to follow.
    private static PlayerInfo Leader => Main.PlayerInfo is PlayerInfo { Destroyed: false } leader ? leader : null;

    // Equip the tool Info.Target needs. False (with the target dropped) when the bag has no
    // such tool left — the human path stops the swing, an ally stops the job.
    private bool EnsureToolForTarget()
    {
        if (Info.Target is not StructureInfo target) return true;
        if (Info.Storage.SetTool(target.operationType)) return true;

        Info.CancelTarget();
        return false;
    }

    public static Info CreateInfo()
    { 
        PlayerInfo player = new PlayerInfo()
        {  
            HitboxType = HitboxType.Friendly,
            targetHitboxType = HitboxType.Passive,
            Storage = new Storage(9),
            HealthMax = 18,
            Mana = 100,
            Sanity = 100,
            Stamina = PlayerInfo.StaminaMax,
            SpeedLogic = 5,
            SpeedGround = 6,
            SpeedAir = 6,
            Iframes = 100, 
            PathAmount = 6000,
            MaxStuckCount = 8,
            AccelerationTime = 0.3f,
            DecelerationTime = 0.1f,
            DistAttack = 2,
            Gravity = -40f,
            JumpVelocity = 12f,
            DeathSfx = SfxID.DeathPlayer,
            HitSfx = SfxID.HitPlayer, 
            CharSprite = ID.Chito 
        };
        return player;
    }
    public override void OnStart()
    {    
        AddModule(new SpriteOrbitModule(transform)); 
        AddModule(new GroundAnimationModule()); 
        AddModule(new GroundMovementModule()); 
        AddModule(new GroundPathingModule()); 
        
        AddState(new IncapacitatedState());
        AddState(new MobAttackSwing());
        AddState(new MobAttackShoot());
        AddState(new MobConsume());
        AddState(new MobChaseAction());
        AddState(new MobHit());
        AddState(new MobEscape());
        AddState(new EquipSelectState());   
        AddState(new MobCraftState());

        // Allies need this on start-up; the controlled player gets it via Inventory.RefreshInventory.
        Inventory.RefreshHeldItem(Info);
    }

    public void OnActionSecondary(Info info)
    {
        // A pet carries nothing, so there is no bag to open.
        if (!Info.HasInventory) return;
        // A bag is UI, not a state of its owner: opening it here would stall the ally's
        // brain (which only runs in Default/MobChaseAction) for as long as it is looked at.
        ContainerWindow.Toggle(Info);
    }
    
    /// <summary>True while a snare flea is latched on (its Bugged slow is active).</summary>
    private bool IsBugged() => GetModule<StatusEffectModule>()?.Has(ID.SnareFlea) ?? false;
    
    private void HandleInput()
    {
        if (transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            transform.position = new Vector3(Main.Player.transform.position.x , World.Inst.Bounds.y + 40, Main.Player.transform.position.z);
        }
         
        if (Info.Target != null && Info.ActionType.IsPickUpOrInteract() && AnyMoveKeyDown())
        {
            Info.CancelTarget();
        }
    }

    // Moving cancels a pending pickup/interaction. Uses the bound Control keys instead of
    // hard-coded KeyCodes so rebinds keep working.
    private static bool AnyMoveKeyDown()
    {
        Control control = Control.Inst;
        return control.Up.KeyDown() || control.Down.KeyDown() || control.Left.KeyDown() ||
               control.Right.KeyDown() || control.Jump.KeyDown();
    }
    
    public override void OnUpdate()
    {
        Info.position = transform.position;

        if (PlayerSync.IsClaimedByRemoteClient(Info.uid)) return;
        if (Main.PlayerInfo == Info)
        {
            HandleInput();
            if (!IsCurrentState<DefaultState>() || Info.Resting) return;

            if (Info.Target != null && Info.ActionType.IsPickUpOrInteract())
            {
                SetState<MobChaseAction>();
                return;
            }

            if (GUIStorage.HoveringSlot || GUIMain.Map is { IsOpen: true }) return;
            if (Info.CanAttack && Control.Inst.ActionPrimary.Key() && !IsBugged())
                Attack();
            else if (Info.Equipment?.Info.Type == ItemType.Consumable && Control.Inst.ActionPrimary.KeyDown())
                SetState<MobConsume>();
            return;
        }

        UpdateAllyBrain();
    }
     
    // ---- Ally AI (Death Road to Canada style) ----

    /// <summary>Recall all allies: cancel their current target/action (and any remembered
    /// structure job), so the brain falls back to following the leader. Runs on the host
    /// (authority over ally AI).</summary>
    public static void RecallAllies()
    {
        if (Save.Inst?.players == null) return;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == Main.PlayerInfo || player.Destroyed) continue;
            if (player.Machine is not PlayerMachine ally) continue;
            ally.Info.ReleaseWork();
            ally.Info.CancelTarget();
        }
    }

    // Drives non-controlled party members. Priority order:
    //   1. finish a fight already under way (an ally only starts one when idle),
    //   2. finish the structure job this ally was ordered to do, unless a threat closes on
    //      the ally — then it drops the job (kept in WorkOrder) and fights,
    //   3. roaming/targetless: pick a new fight or sweep up loot,
    //   4. otherwise trail the leader.
    // Allies never adopt the leader's target — the only way they get a structure job is by
    // the player controlling them and hitting it, which leaves the target on the ally when
    // control moves on. SetState is a no-op when already in MobChaseAction, so calling it
    // unconditionally just retargets via Info.Target while following. Protected so
    // DelverMachine can drive itself with the same brain.
    protected void UpdateAllyBrain()
    {
        if (Info.Resting)
        {
            Info.CancelTarget();
            return;
        }
        if (Info.PlayerStatus == PlayerStatus.Incapacitated) return;
        if (!IsCurrentState<DefaultState>() && !IsCurrentState<MobChaseAction>()) return;

        // A pending pickup/interaction owns the brain until it completes or gets dropped.
        if (Info.ActionType.IsPickUpOrInteract())
        {
            if (Info.Target != null) return;
            Info.ActionType = IActionType.Follow;
        }

        // Harvestables (grass, flowers, mud piles, fallen logs) are pickup-scale decor;
        // an ally shouldn't stay locked onto one instead of following the leader.
        if (Info.Target is HarvestableInfo)
            Info.CancelTarget();

        // Low health: don't fight — flee from nearby hostiles, else keep following.
        if (Info.Health <= Info.HealthMax / 4 && TryFleeEnemy())
        {
            SetState<MobEscape>();
            return;
        }

        if (EngagedEnemy is { } enemy)
        {
            if (IsBeyondPartyRange(transform.position) || IsBeyondPartyRange(enemy.position))
            {
                Info.CancelTarget();
                FollowLeader();
                return;
            }

            if (Info.IsTired)
            {
                if (Info.Storage.SetRangedWeapon())
                {
                    Info.ActionType = IActionType.Hit;
                    SetState<MobChaseAction>();
                }
                else SetState<MobEscape>();
                return;
            }

            if (TryEngageEnemy(enemy) || TryContinueWork()) SetState<MobChaseAction>();
            return;
        }

        if (Info.WorkOrder != null && !IsBeyondPartyRange(transform.position) &&
            FindNearestThreat() is { } assailant && !IsBeyondPartyRange(assailant.position) &&
            TryDefendSelf(assailant))
        {
            SetState<MobChaseAction>();
            return;
        }

        StoreLootInNearbyShelf();

        if (TryContinueWork() || (!Info.IsTired && (TryEngageEnemy() || TryAcquireLootTarget())))
        {
            SetState<MobChaseAction>();
            return;
        }

        FollowLeader();
    }

    private void FollowLeader()
    {
        if (!IsCurrentState<DefaultState>()) return;
        if (Leader is not PlayerInfo leader) return;
        Info.Target = leader;
        Info.ActionType = IActionType.Follow;
        SetState<MobChaseAction>();
    }

    /// <summary>The enemy this ally is already trading blows with (its target, when that
    /// target is a threat), or null.</summary>
    private MobInfo EngagedEnemy
    {
        get
        {
            MobInfo enemy = Info.Target as MobInfo;
            return enemy != null && !enemy.Destroyed && IsThreat(enemy) ? enemy : null;
        }
    }

    // Keeps the ally fighting. It stays on the enemy it already engaged (passed in) until the
    // party moves out of range, and otherwise engages the nearest threat around the leader.
    // Either way it needs something it can actually swing.
    private bool TryEngageEnemy(MobInfo engaged = null)
    {
        MobInfo enemy = engaged;
        if (enemy == null)
        {
            if (Leader is not PlayerInfo leader || IsBeyondPartyRange(transform.position)) return false;

            enemy = FindNearestThreat();
            if (enemy == null || IsBeyondPartyRange(enemy.position) ||
                Vector3.Distance(leader.position, enemy.position) > Info.DistAlert) return false;
        }

        if (!TryDefendSelf(enemy))
        {
            if (engaged != null) Info.CancelTarget();
            return false;
        }
        return true;
    }

    private bool TryDefendSelf(MobInfo threat)
    {
        if (Info.HasInventory && !Info.Storage.SetWeapon()) return false;
        Info.Target = threat;
        Info.ActionType = IActionType.Hit;
        return true;
    }

    private const float PartyRange = 8f;

    private bool IsBeyondPartyRange(Vector3 point) =>
        Leader is PlayerInfo leader && Vector3.Distance(leader.position, point) > PartyRange;

    private bool TryContinueWork()
    {
        if (Info.WorkOrder is not { Destroyed: false } job)
        {
            Info.WorkOrder = null;
            return false;
        }

        if (IsBeyondPartyRange(job.position))
        {
            if (Info.IsWorking) Info.CancelTarget();
            return false;
        }

        Info.Target = job;
        if (!EnsureToolForTarget())
        {
            Info.WorkOrder = null;
            return false;
        }

        Info.ActionType = IActionType.Hit;
        return true;
    }

    // Flees from the nearest nearby hostile (used when low on health).
    private bool TryFleeEnemy()
    {
        MobInfo threat = FindNearestThreat();
        if (threat == null) return false;
        Info.Target = threat;
        return true;
    }

    // A threat is anything the party must defend against: an enemy, a mob that has already
    // damaged a player (tree mimics and enraged visitors are hostile despite a passive
    // hitbox), or a passive animal that is fighting back. Docile animals use their target as
    // a flee anchor, so PassiveInfo's default ActionType can't be used as the signal.
    private bool IsThreat(MobInfo mob)
    {
        if (mob == null || mob.Destroyed) return false;
        if (mob.HitboxType == HitboxType.Enemy || mob.HostileToPlayers) return true;
        return mob is SheepInfo sheep && sheep.Retaliating;
    }

    // Nearest threat in the ally's alert radius.
    private MobInfo FindNearestThreat()
    {
        return EntityScan.FindNearest(transform.position, Info.DistAlert, i => i is MobInfo m && IsThreat(m)) as MobInfo;
    }

    // ---- Loot pickup ----

    /// <summary>How close an ordinary drop must already be before an ally detours to grab
    /// it — about touching distance, so allies don't wander off chasing junk.</summary>
    private const float LootRadius = 2f;

    /// <summary>Extra reach per worth tier above common: an ally walks further for loot
    /// that is actually worth carrying.</summary>
    private const float LootReachPerTier = 2f;

    /// <summary>Claim the most valuable drop the ally can reach and fit in their bag.
    /// Worth beats distance, so an ally walks past gravel for a relic; between drops of
    /// equal worth the closer one wins. Contracted companions never loot: their loot would
    /// vanish with the contract.</summary>
    private bool TryAcquireLootTarget()
    {
        if (Info.CantLoot) return false;

        ItemInfo best = null;
        float bestScore = float.MinValue;

        EntityScan.ForEach(transform.position, MaxLootRadius(), IsWantedLoot, info =>
        {
            ItemInfo item = (ItemInfo)info;
            int worth = Trade.Worth(item.item.ID) * item.item.Stack;
            float distance = Vector3.Distance(transform.position, item.position);
            if (distance > LootRadius + (int)Trade.TierOf(worth) * LootReachPerTier) return;

            float score = worth - distance;
            if (score <= bestScore) return;
            bestScore = score;
            best = item;
        });

        if (best == null) return false;

        Info.Target = best;
        Info.ActionType = IActionType.PickUp;
        // Fresh action, not reached yet. Repathing itself is automatic: the pathing module
        // rebuilds whenever Info.Target stops matching the path it holds.
        Info.PathingStatus = PathingStatus.Pending;
        return true;
    }

    /// <summary>Farthest any drop can pull an ally, matching the per-tier reach above.</summary>
    private static float MaxLootRadius() =>
        LootRadius + (int)WorthTier.Legendary * LootReachPerTier;

    // ---- Storing loot ----

    /// <summary>How close a shelf must be before an ally stows its loot in it.</summary>
    private const float ShelfRange = 2.5f;

    /// <summary>Seconds between shelf scans, so an ally standing next to a full shelf doesn't
    /// run a physics query every frame.</summary>
    private const float ShelfScanInterval = 0.5f;

    private float _nextShelfScan;

    /// <summary>Put gathered loot on a nearby shelf. Tools, food and coins stay in the bag;
    /// raw materials and blocks go on the shelf so looting doesn't fill the ally's hands.</summary>
    private void StoreLootInNearbyShelf()
    {
        if (!Helper.IsHost() || !Info.HasInventory || Info.Storage?.List == null) return;
        if (Time.time < _nextShelfScan) return;
        _nextShelfScan = Time.time + ShelfScanInterval;

        if (!Info.Storage.List.Exists(HasStorableLoot)) return;

        if (EntityScan.FindNearest(transform.position, ShelfRange,
                i => i is ContainerInfo { Destroyed: false, Storage: not null } container && container.id == ID.Shelf)
            is not ContainerInfo shelf)
            return;

        bool moved = false;
        foreach (ItemSlot slot in Info.Storage.List)
        {
            if (!HasStorableLoot(slot) || ReferenceEquals(slot, Info.Equipment)) continue;
            if (shelf.Storage.TryStore(slot)) moved = true;
        }
        if (moved) Info.Storage.NotifyChanged();
    }

    private static bool HasStorableLoot(ItemSlot slot) =>
        slot is { Stack: > 0 } && slot.Info != null && !Trade.IsCoin(slot.ID) &&
        slot.Info.Type is ItemType.Material or ItemType.Block;

    private bool IsWantedLoot(Info info)
    {
        if (info is not ItemInfo item || item.Destroyed) return false;
        if (!item.CanPickUp) return false;
        if (!Info.Storage.CanFit(item.item)) return false;
        return !IsLootClaimed(item);
    }

    // Leave it for whoever is already walking to it (the leader, or another ally).
    private static bool IsLootClaimed(ItemInfo item)
    {
        if (Save.Inst?.players == null) return false;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == null || player.Destroyed) continue;
            if (player.Target == item && player.ActionType.IsPickUpOrInteract()) return true;
        }
        return false;
    }

    public override void Attack()
    {
        if (Main.PlayerInfo != Info && !PlayerSync.IsClaimedByRemoteClient(Info.uid))
        {
            if (!EnsureToolForTarget()) return;
            if (Info.IsWorking && !Info.CanSwingWork()) return;
        }

        if (Info.Equipment == null)
        {
            Info.Target = null;
            return;
        }

        ItemSlot equipped = Info.Equipment;

        base.Attack();

        if (equipped.Durability == -1) return;

        equipped.Durability--;
        if (equipped.Durability <= 0)
        {
            equipped.clear();
            Info.SetEquipment(null);
        }
        Info.Storage.NotifyChanged();
    }
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 
