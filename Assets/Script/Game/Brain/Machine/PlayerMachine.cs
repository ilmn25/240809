
using UnityEngine;
 
public class PlayerMachine : MobMachine, IActionSecondaryInteract
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();

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
            Stamina = 100,
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

        // Apply the selected inventory slot as the held tool so it renders in hand.
        // The controlled player gets this via Inventory.RefreshInventory; allies need it here.
        Info.Storage.ApplySelectedEquipment();
    }

    public void OnActionSecondary(Info info)
    {
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
            switch (Info.Equipment?.Info.Type)
            {
                case ItemType.Tool when Control.Inst.ActionPrimary.Key() && !IsBugged():
                    Attack();
                    break;
                case ItemType.Consumable when Control.Inst.ActionPrimary.KeyDown():
                    SetState<MobConsume>();
                    break;
            }
            return;
        }

        UpdateAllyBrain();
    }
     
    // ---- Ally AI (Death Road to Canada style) ----

    /// <summary>Recall all allies: cancel their current target/action, so the brain
    /// falls back to following the leader. Runs on the host (authority over ally AI).</summary>
    public static void RecallAllies()
    {
        if (Save.Inst?.players == null) return;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == Main.PlayerInfo || player.Destroyed) continue;
            if (player.Machine is not PlayerMachine ally) continue;
            ally.Info.CancelTarget();
        }
    }

    // Drives non-controlled party members. Priority order:
    //   1. finish a fight already under way,
    //   2. finish the structure job this ally was ordered to do,
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

        // Priority, highest first: fighting, then the structure job this ally was ordered to
        // do. Only a roaming/targetless ally picks up loot.
        if (TryEngageEnemy() || TryContinueWork() || TryAcquireLootTarget())
        {
            SetState<MobChaseAction>();
            return;
        }

        if (!IsCurrentState<DefaultState>()) return;
        if (Leader is not PlayerInfo leader) return;
        Info.Target = leader;
        Info.ActionType = IActionType.Follow;
        SetState<MobChaseAction>();
    }

    // Keeps the ally fighting. It stays on the enemy it already engaged — that fight may
    // drift out to disengage range — and otherwise engages the nearest threat around the
    // leader. Either way it needs something it can actually swing.
    private bool TryEngageEnemy()
    {
        if (Leader is not PlayerInfo leader) return false;

        MobInfo enemy = Info.Target as MobInfo;
        bool engaged = enemy != null && !enemy.Destroyed && IsThreat(enemy);

        if (engaged)
        {
            if (IsBeyondAlert(leader.position) ||
                Vector3.Distance(leader.position, enemy.position) > Info.DistDisengage)
            {
                Info.CancelTarget();
                return false;
            }
        }
        else
        {
            // Don't start new fights while away from the leader — head back first.
            if (IsBeyondAlert(leader.position)) return false;

            enemy = FindNearestThreat();
            if (enemy == null || Vector3.Distance(leader.position, enemy.position) > Info.DistAlert) return false;
        }

        // Draw a weapon first: an ally that just finished a job is still holding its pickaxe.
        if (!Info.Storage.SetWeapon())
        {
            if (engaged) Info.CancelTarget();
            return false;
        }

        Info.Target = enemy;
        Info.ActionType = IActionType.Hit;
        return true;
    }

    // An ally finishes the structure job it was ordered to do — the target it kept from the
    // last time the player controlled it and hit that structure. Runs until the structure is
    // gone or the tool it needs breaks; nothing here ever adopts another player's target.
    private bool TryContinueWork()
    {
        if (Info.Target is not StructureInfo job || job.Destroyed) return false;
        if (!EnsureToolForTarget()) return false;

        Info.ActionType = IActionType.Hit;
        return true;
    }

    // True when the ally has strayed past its alert radius from the given point.
    private bool IsBeyondAlert(Vector3 from) => Vector3.Distance(from, transform.position) > Info.DistAlert;

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

    /// <summary>How close a drop must already be before an ally detours to grab it — about
    /// touching distance, so allies don't wander off chasing loot.</summary>
    private const float LootRadius = 1f;

    /// <summary>Claim the nearest drop the ally can actually collect and fit in their bag.</summary>
    private bool TryAcquireLootTarget()
    {
        if (EntityScan.FindNearest(transform.position, LootRadius, IsWantedLoot) is not ItemInfo item)
            return false;

        Info.Target = item;
        Info.ActionType = IActionType.PickUp;
        // Fresh action, not reached yet. Repathing itself is automatic: the pathing module
        // rebuilds whenever Info.Target stops matching the path it holds.
        Info.PathingStatus = PathingStatus.Pending;
        return true;
    }

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
        if (Main.PlayerInfo != Info && !PlayerSync.IsClaimedByRemoteClient(Info.uid) && !EnsureToolForTarget())
        {
            return;
        }

        if (Info.Equipment == null)
        {
            Info.Target = null;
            return;
        }

        // Ammo is resolved and consumed in MobAttackShoot (guns fire whatever
        // accepted ammo the shooter carries; throwables keep their own ammo).
        base.Attack();

        if (Info.Equipment.Durability != -1)
        {
            Info.Equipment.Durability--;
            if (Info.Equipment.Durability == 0)
            {
                Info.Equipment.clear();
                Info.SetEquipment(null);
            }
            Info.Storage.NotifyChanged();
        }
    }
    public void OnDrawGizmos()
    {
        if (Camera.current != Camera.main)
            return;

        GetModule<GroundPathingModule>().DrawGizmos();
    }
} 
