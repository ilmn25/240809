 
using UnityEngine;
 
public class PlayerMachine : MobMachine, IActionSecondaryInteract
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();

    private bool EnsureCompatibleToolForTarget()
    {
        if (Info.Target is not StructureInfo structureTarget) return true;

        if (Info.Storage.SetTool(structureTarget.operationType)) return true;

        Info.CancelTarget();
        return false;
    }

    public static Info CreateInfo()
    { 
        PlayerInfo player = new PlayerInfo()
        {  
            HitboxType = HitboxType.Friendly,
            targetHitboxType = HitboxType.Passive,
            Storage = new Storage(9)
            {
                Name = "Inventory"
            },
            HealthMax = 18,
            Defense = 0,
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
        player.Storage.Key = 0;
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
        AddState(new InContainerState()
        {
            Storage = Info.Storage
        });

        // Apply the selected inventory slot as the held tool so it renders in hand.
        // The controlled player gets this via Inventory.RefreshInventory; allies need it here.
        Storage storage = Info.Storage;
        if (storage?.List != null && storage.Key >= 0 && storage.Key < storage.List.Count)
        {
            ItemSlot selected = storage.List[storage.Key];
            Info.SetEquipment(selected is { Stack: > 0 } ? selected : null);
        }
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<InContainerState>())
            SetState<DefaultState>();
        else 
            SetState<InContainerState>();
    }
    
    private void HandleInput()
    {
        if (transform.position.y < -50)
        {
            MapCull.ForceRevertMesh(); 
            transform.position = new Vector3(Main.Player.transform.position.x , World.Inst.Bounds.y + 40, Main.Player.transform.position.z);
        }
         
        if (Info.Target != null && Info.ActionType is IActionType.PickUp or IActionType.Interact &&
            (Input.GetKeyDown(KeyCode.A) ||
             Input.GetKeyDown(KeyCode.W) ||
             Input.GetKeyDown(KeyCode.S) ||
             Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.Space)))
        { 
            Info.CancelTarget();
        } 
    }
    
    public override void OnUpdate()
    {   
        Info.position = transform.position;

        bool blockedByOther = PlayerSync.IsClaimedByRemoteClient(Info.uid);

        if (Main.PlayerInfo == Info && !blockedByOther)
        { 
            HandleInput();
            
            if (IsCurrentState<DefaultState>())
            {
                if (Info.Target != null && Info.ActionType is IActionType.PickUp or IActionType.Interact)
                { 
                    SetState<MobChaseAction>();
                }
                else if (!GUIStorage.HoveringSlot && GUIMain.Map is not { IsOpen: true })
                {
                    switch (Info.Equipment?.Info.Type)
                    {
                        case ItemType.Tool: 
                            if (Control.Inst.ActionPrimary.Key())
                                Attack();
                            break;
                        case ItemType.Consumable:
                            if (Control.Inst.ActionPrimary.KeyDown())
                                SetState<MobConsume>();
                            break;
                    }
                } 
            } 
        }
        else if (!blockedByOther)
        {
            UpdateAllyBrain();
        } 
    }
     
    // ---- Ally AI (Death Road to Canada style) ----

    private static readonly Collider[] AllyScanBuffer = new Collider[32];

    /// <summary>Recall all allies: cancel their current target/action. The ally brain
    /// then auto-follows the leader. Runs on the host (authority over ally AI).</summary>
    public static void RecallAllies()
    {
        if (Save.Inst == null || Main.PlayerInfo == null) return;
        // Un-mark any structure the leader had allies work, so the handoff can't
        // re-assign it the moment the ally's target is cancelled.
        if (Main.PlayerInfo.Target is StructureInfo)
            Main.PlayerInfo.Target = null;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == Main.PlayerInfo || player.Destroyed) continue;
            if (player.Machine is not PlayerMachine ally) continue;
            ally.Info.CancelTarget();
        }
    }

    // Drives non-controlled party members: fight nearby hostiles, otherwise trail the leader.
    // SetState is a no-op when already in MobChaseAction, so calling it unconditionally just
    // retargets via Info.Target while following.
    private void UpdateAllyBrain()
    {
        if (Info.PlayerStatus == PlayerStatus.Incapacitated) return;
        if (!IsCurrentState<DefaultState>() && !IsCurrentState<MobChaseAction>()) return;
        if (Info.ActionType is IActionType.PickUp or IActionType.Interact) return;

        bool lowHealth = Info.Health <= Info.HealthMax / 4;

        // Low health: don't fight — flee from nearby hostiles, else keep working/following.
        if (lowHealth && TryFleeEnemy())
        {
            SetState<MobEscape>();
            return;
        }

        // A threat takes priority over cutting — don't let a sheep you clipped kill you.
        if (TryAcquireEnemyTarget())
        {
            Info.ActionType = IActionType.Hit;
            SetState<MobChaseAction>();
            return;
        }

        // Work a structure: the assigned one, or whatever the leader marked.
        StructureInfo work = null;
        if (Info.Target is StructureInfo assigned && Info.ActionType is IActionType.Hit or IActionType.Dig)
            work = assigned;
        else if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed &&
                 Main.PlayerInfo.Target is StructureInfo leaderTask && !leaderTask.Destroyed)
            work = leaderTask;

        // No compatible tool — don't lock onto it, follow the leader instead.
        if (work != null && Info.Storage.SetTool(work.operationType))
        {
            Info.Target = work;
            Info.ActionType = IActionType.Hit;
            SetState<MobChaseAction>();
            return;
        }

        // Idle: trail the controlling character.
        if (!IsCurrentState<DefaultState>()) return;

        if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
        {
            Info.Target = Main.PlayerInfo;
            Info.ActionType = IActionType.Follow;
        }
        SetState<MobChaseAction>();
    }

    // Anchored to the leader: fights enemies on sight, and retaliates against a passive mob
    // that is attacking a player. Breaks off and re-follows the leader if the fight drags it
    // away. Unarmed allies just trail.
    private bool TryAcquireEnemyTarget()
    {
        if (Info.Equipment == null) return false;
        if (Main.PlayerInfo == null) return false; // nothing to defend / follow

        Vector3 leaderPos = Main.PlayerInfo.position;

        // Keep the current target while it's still a threat and the fight stays near.
        if (Info.Target is MobInfo current && IsThreat(current))
        {
            if (current.Destroyed ||
                Vector3.Distance(leaderPos, current.position) > Info.DistDisengage ||
                Vector3.Distance(leaderPos, transform.position) > Info.DistAlert)
            {
                Info.CancelTarget();
                return false;
            }
            return true;
        }

        // Don't pick new fights while away from the leader — head back first.
        if (Vector3.Distance(leaderPos, transform.position) > Info.DistAlert) return false;

        // Engage the nearest threat near the leader.
        MobInfo enemy = FindNearestThreat();
        if (enemy == null || Vector3.Distance(leaderPos, enemy.position) > Info.DistAlert) return false;
        Info.Target = enemy;
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

    // A threat is any enemy (attacked on sight) or a passive mob that is actually
    // attacking a player. A docile sheep/chicken merely fleeing is not a threat —
    // PassiveInfo defaults ActionType to Hit for all passives, so we rely on the
    // sheep's Retaliating flag (set only when it's hit and fights back) instead of
    // ActionType, which would wrongly flag innocent farm animals.
    private bool IsThreat(MobInfo mob)
    {
        if (mob == null || mob.Destroyed) return false;
        if (mob.HitboxType == HitboxType.Enemy) return true;
        if (mob.HitboxType != HitboxType.Passive) return false;
        return mob is SheepInfo sheep && sheep.Retaliating;
    }

    // Nearest threat in the ally's alert radius.
    private MobInfo FindNearestThreat()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, Info.DistAlert, AllyScanBuffer, Main.MaskEntity);
        MobInfo best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (AllyScanBuffer[i].TryGetComponent(out EntityMachine em) &&
                em.Info is MobInfo enemy && IsThreat(enemy))
            {
                float sqr = (em.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = enemy; }
            }
        }
        return best;
    }

    public override void Attack()
    {
        if (Main.PlayerInfo != Info && !PlayerSync.IsClaimedByRemoteClient(Info.uid) && !EnsureCompatibleToolForTarget())
        {
            return;
        }

        if (Info.Equipment == null)
        {
            Info.Target = null;
            return;
        }
        
        if (Info.Equipment.Info.ProjectileInfo != null)
        {
            if (Info.Equipment.Info.ProjectileInfo.Ammo != ID.Null && 
                Info.Storage.GetAmount(Info.Equipment.Info.ProjectileInfo.Ammo) == 0) return;
            Info.Storage.RemoveItem(Info.Equipment.Info.ProjectileInfo.Ammo);
        }

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
