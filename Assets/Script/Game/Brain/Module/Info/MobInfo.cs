using System;
using UnityEngine;
public enum IActionType {Follow, Interact, Hit, Dig, PickUp}

public static class ActionTypeExtensions
{
    /// <summary>Pick-up and interaction targets are held until they complete or the carrier
    /// moves, so every "is this a pending interaction?" test goes through here.</summary>
    public static bool IsPickUpOrInteract(this IActionType actionType)
        => actionType is IActionType.PickUp or IActionType.Interact;
}
 
[System.Serializable]
public class MobInfo : DynamicInfo
{ 
    public int DistAttack = 2;
    public float DistFollow = 0.65f; // how close allies trail the leader (tight)
    public int DistRanged = 10;      // preferred firing range for ranged allies
    public int DistAlert = 5;        // enemy detection radius + how close allies stick to the leader
    public int DistDisengage = 14;   // how far an enemy pursues before giving up 
    
    public int DistEscape = 20;
    public int DistRoam = 10;
    public int DistStrafe = 2;
    
    public int PathJump = 1;
    public int PathHeight = 1;
    public int PathFall = 15;
    public int PathAir = 4;
    public int PathAmount = 3000;
    public int MaxStuckCount = 250; 
    public int PointLostDistance = 5;
    public int NormalSkipAmount = 1;
    public bool mustLandFirst = false;
    public HitboxType targetHitboxType;
 
    [NonSerialized] public ItemSlot Equipment;
    [NonSerialized] private Light _toolGlow;
    /// <summary>Slots mutate in place (a bucket filling, a stack refilling), so comparing the
    /// slot reference alone can miss a changed hand.</summary>
    [NonSerialized] private ID _renderedItem;
    
    [NonSerialized] public Info Target;
    [NonSerialized] public IActionType ActionType;
    
    [NonSerialized] public bool FaceTarget;
    [NonSerialized] public Vector3 AimPosition; 
    [NonSerialized] public PathingStatus PathingStatus = PathingStatus.Pending;

    /// <summary>True once this mob has damaged a player. HitboxType can't express hostility —
    /// tree mimics and enraged visitors carry a passive hitbox — so "it hurt us" is the one
    /// signal that is always true, and allies fight anything flying this flag.</summary>
    [NonSerialized] public bool HostileToPlayers;

    // Animation inputs are serialized by EntitySync for remote clients.

    public override void Initialize()
    {
        base.Initialize();
        // A pooled GameObject can be reused for a new entity while still showing the
        // previous owner's held item (SpriteTool). Clear it so the new entity doesn't
        // appear to be holding an item it doesn't have.
        Equipment = null;
        HostileToPlayers = false;
        if (SpriteTool != null)
        {
            SetToolGlow(false);
            SpriteTool.gameObject.SetActive(false);
        }
    }

    public void CancelTarget()
    {
        Target = null; 
        PathingStatus = PathingStatus.Stuck;
        Direction = Vector3.zero;            
        Machine.SetState<DefaultState>();
    }

    /// <summary>Acquire a target from landing a hit. Overridden by entity types that own
    /// their targeting policy (see <see cref="PlayerInfo.AcquireTarget"/>).</summary>
    public virtual void AcquireTarget(Info target)
    {
        Target = target;
        ActionType = IActionType.Hit;
    }

    /// <summary>Record that this mob has attacked a player, so the party treats it as a
    /// threat even when its hitbox type is passive.</summary>
    public void MarkHostile() => HostileToPlayers = true;

    /// <summary>React to being hit: drop what we were doing, take the attacker as the new
    /// target and play the hit reaction.</summary>
    protected void Retaliate(Projectile projectile)
    {
        if (Target == null)
            Machine.SetState<DefaultState>();

        Target = projectile.SourceInfo;
        PathingStatus = PathingStatus.Reached;
        Machine.SetState<MobHit>();
    }
    
    protected override void OnUpdate()
    { 
        base.OnUpdate();
        // Server-side broadcasting is handled centrally by EntitySync's batch loop.

        if (Target != null)
        {
            if (Target.Destroyed)
            {
                // Just drop the dead target without resetting the state machine,
                // so the swing animation isn't interrupted on the killing blow.
                Target = null;
                PathingStatus = PathingStatus.Stuck;
            }
            else
            {
                TargetScreenDir = (Camera.main.WorldToScreenPoint(Target.position) - 
                                   Camera.main.WorldToScreenPoint(Machine.transform.position)).normalized;
            }
        }
    }

    public Vector3 GetDirection()
    {
        Vector3 direction = SpriteToolTrack.right;
        if (SpriteToolTrack.lossyScale.x < 0f) 
            direction *= -1;
        direction.y = 0;
        direction.Normalize();
        return direction;
    }

    /// <summary>Height above an entity's feet that an AI aims a projectile at — the body,
    /// not the ground it stands on. Aiming at the feet lands the shot in the floor.</summary>
    public const float AimBodyHeight = 0.55f;

    /// <summary>The point an AI should aim at to hit its current target with the given
    /// projectile: the target's body, led by how far it will move before the shot arrives.
    /// Two passes sharpen the flight time, because the time depends on the aim point.</summary>
    public Vector3 GetAimPoint(Vector3 origin, ProjectileInfo projectile)
    {
        if (Target == null) return AimPosition;

        Vector3 body = Target.position + Vector3.up * AimBodyHeight;
        float speed = projectile?.Speed ?? 0f;
        if (speed <= 0f) return body;

        // Walkers move their transform directly, so Velocity only holds knockback and air
        // drift; the walked motion is Direction scaled by the current speed.
        Vector3 velocity = Target is DynamicInfo moving
            ? moving.Direction * moving.SpeedCurrent + moving.Velocity
            : Vector3.zero;
        velocity.y = 0f;

        Vector3 aim = body;
        for (int i = 0; i < 2; i++)
            aim = body + velocity * (Vector3.Distance(origin, aim) / speed);
        return aim;
    }
    public void SetEquipment(ItemSlot target)
    {
        ID shown = target is { Stack: > 0 } ? target.ID : ID.Null;
        if (ReferenceEquals(target, Equipment) && shown == _renderedItem)
            return;

        if (SpriteTool == null || SpriteToolTrack == null || SpriteToolRenderer == null)
        {
            Equipment = target;
            return;
        }

        if (shown != ID.Null && shown == _renderedItem)
        {
            Equipment = target;
            return;
        }

        _renderedItem = shown;

        if (target == null)
        {
            Equipment = null;
            SetToolGlow(false);
            SpriteTool.gameObject.SetActive(false);
            return;
        }

        Equipment = target;
        SpriteTool.gameObject.SetActive(true);
        SpriteTool.localPosition = new Vector3(Equipment.Info.HoldoutOffset.x, Equipment.Info.HoldoutOffset.y, 0);
        SpriteTool.localRotation = Quaternion.Euler(0, 0, Equipment.Info.RotationOffset);
        string spriteName = Equipment.Info.ID.ToString();
        if (Equipment.ID == ID.MiningBox || Equipment.Info.Type == ItemType.Structure)
            spriteName = "Blueprint";
        
        SpriteToolRenderer.sprite = Cache.LoadSprite("Sprite/" + spriteName);
        SpriteToolTrack.transform.localScale = Vector3.one * Equipment.Info.Scale;
        SetToolGlow(Equipment.Info.Glow);
        if (Machine != null && Machine.IsCurrentState<DefaultState>() && (Helper.IsHost() || IsOwner()))
            Machine.SetState<EquipSelectState>();
    }

    private void SetToolGlow(bool on)
    {
        if (_toolGlow == null && SpriteTool != null)
            _toolGlow = SpriteTool.Find("Glow")?.GetComponent<Light>();
        if (_toolGlow != null)
            _toolGlow.enabled = on;
    }
}