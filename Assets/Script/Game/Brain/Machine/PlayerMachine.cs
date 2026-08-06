 
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
            HealthMax = 12,
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
        // Spawn with a CrudeHatchet in hand (inventory slot 0, selected).
        player.Storage.List[0] = new ItemSlot(ID.CrudeHatchet);
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
        AddState(new MobChaseAction());
        AddState(new MobHit());
        AddState(new MobEscape());
        AddState(new EquipSelectState());   
        AddState(new InContainerState()
        {
            Storage = Info.Storage
        });
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
                else if (!GUIMain.IsHover)
                {
                    switch (Info.Equipment?.Info.Type)
                    {
                        case ItemType.Tool: 
                            if (Control.Inst.ActionPrimary.Key())
                                Attack();
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

        // Keep working a structure the player assigned instead of chasing zombies.
        if (Info.Target is StructureInfo && Info.ActionType is IActionType.Hit or IActionType.Dig)
        {
            EnsureCompatibleToolForTarget();
            SetState<MobChaseAction>();
            return;
        }

        // Fight nearby hostiles (reacts even while trailing the leader), unless low on health.
        if (!lowHealth && TryAcquireEnemyTarget())
        {
            Info.ActionType = IActionType.Hit;
            SetState<MobChaseAction>();
            return;
        }

        // Idle: take pending tasks, otherwise trail the controlling character.
        if (!IsCurrentState<DefaultState>()) return;

        if (PlayerTask.Pending.Count != 0)
        {
            foreach (StructureInfo si in PlayerTask.Pending)
                if (Info.Storage.SetTool(si.operationType))
                { Info.Target = si; Info.ActionType = IActionType.Hit; SetState<MobChaseAction>(); return; }
        }

        if (Main.PlayerInfo != null && !Main.PlayerInfo.Destroyed)
        {
            Info.Target = Main.PlayerInfo;
            Info.ActionType = IActionType.Follow;
        }
        SetState<MobChaseAction>();
    }

    // Anchored to the leader: only fights hostiles that are actively attacking a player, breaks
    // off and re-follows the leader if the fight drags it away. Unarmed allies just trail.
    private bool TryAcquireEnemyTarget()
    {
        if (Info.Equipment == null) return false;
        if (Main.PlayerInfo == null) return false; // nothing to defend / follow

        Vector3 leaderPos = Main.PlayerInfo.position;

        // Keep the current target only while it's still attacking a player and the fight stays near.
        if (Info.Target is MobInfo current && current.HitboxType == HitboxType.Enemy)
        {
            if (current.Destroyed ||
                current.Target is not PlayerInfo ||
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

        // Only engage mobs near the leader that are actively attacking a player.
        MobInfo enemy = FindNearestEnemy(onlyAggroedOnPlayer: true);
        if (enemy == null || Vector3.Distance(leaderPos, enemy.position) > Info.DistAlert) return false;
        Info.Target = enemy;
        return true;
    }

    // Flees from the nearest nearby hostile (used when low on health).
    private bool TryFleeEnemy()
    {
        MobInfo threat = FindNearestEnemy(onlyAggroedOnPlayer: false);
        if (threat == null) return false;
        Info.Target = threat;
        return true;
    }

    // Nearest hostile in the ally's alert radius. When onlyAggroedOnPlayer is true, only mobs
    // currently attacking a player count; otherwise any hostile counts (for fleeing while low).
    private MobInfo FindNearestEnemy(bool onlyAggroedOnPlayer)
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, Info.DistAlert, AllyScanBuffer, Main.MaskEntity);
        MobInfo best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (AllyScanBuffer[i].TryGetComponent(out EntityMachine em) &&
                em.Info is MobInfo enemy &&
                enemy.HitboxType == HitboxType.Enemy && !enemy.Destroyed &&
                (!onlyAggroedOnPlayer || enemy.Target is PlayerInfo))
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
