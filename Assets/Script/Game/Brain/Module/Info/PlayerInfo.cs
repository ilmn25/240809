using System;
using System.Collections;
using UnityEngine;

public enum PlayerStatus {
        Active,
    Respawn,
    Incapacitated
}
[System.Serializable]
public class PlayerInfo : MobInfo
{
    /// <summary>True for AI-only "delver" entities — a player that is never
    /// controlled by a human. Persists in the save's player list like a party
    /// member but is excluded from control switching, client claims, and the
    /// death handoff.</summary>
    public bool IsDelver;
    /// <summary>True for a pet: a player-like companion that can be taken over like any
    /// party member but carries nothing of its own (its Storage has no slots). It never
    /// counts as the last human standing, and it never starves.</summary>
    public bool IsPet;
    /// <summary>False for a pet (and anything else with a slot-less Storage): there is no
    /// bag to open or fill, so drops go to the cursor instead.</summary>
    public bool HasInventory => Storage?.List != null && Storage.List.Count > 0;
    /// <summary>True for contracted companions (hired mercenaries, escorts): they follow
    /// and fight, but never pick up loot — the party's share stays with the party.</summary>
    [NonSerialized] public bool CantLoot;
    public Storage Storage;
    public float Mana;
    public float Sanity;
    public int Hunger;
    public int HungerMax = 20;
    public float Stamina; 
    [NonSerialized] public bool Resting;

    /// <summary>The structure job (tree/rock/door) this player was ordered to do — set when
    /// a human controlling them hits that structure. Kept apart from Target so a dropped or
    /// fight-driven target doesn't erase the job; the ally picks it back up afterwards.</summary>
    [NonSerialized] public StructureInfo WorkOrder;

    private const float WellFedThreshold = 0.85f;
    private const float MoveHungerInterval = 45f;
    private const float RestHealInterval = 1f;
    private const float TorchBurnInterval = 1f; // seconds of carrying per point of durability
    private float _moveHungerAccumulator;
    private float _restHealAccumulator;
    private float _torchBurnAccumulator;
    private static readonly StatusEffect WellFed = new StatusEffect(
        ID.WellFed, EffectType.Heal, duration: 120f, tickInterval: 40f, amountPerTick: 1, name: "Well Fed");

    public int BaseHealthMax;
    private enum HungerStage { None, I, II, III, IV, V }

    private HungerStage CurrentHungerStage
    {
        get
        {
            if (Hunger <= 0) return HungerStage.V;
            float fraction = (float)Hunger / HungerMax;
            if (fraction < 0.2f) return HungerStage.IV;
            if (fraction < 0.35f) return HungerStage.III;
            if (fraction < 0.5f) return HungerStage.II;
            if (fraction < 0.75f) return HungerStage.I;
            return HungerStage.None;
        }
    }

    private const float JumpGraceTime = 0.1f; 
    private const float CoyoteTime = 0.1f; 
    public float AirTime;
    private float _jumpGraceTimer;
    private float _coyoteTimer;
    public PlayerStatus PlayerStatus = PlayerStatus.Respawn;

    /// <summary>Puts the player into spawn protection (invulnerable, no control)
    /// for a generous number of frames. Used during world switches so a player
    /// whose transform is still at stale coordinates can't fall to their death
    /// while the old world's ground is torn down and the new one loads.</summary>
    public void GrantWorldSwitchProtection(int frames = 600)
    {
        PlayerStatus = PlayerStatus.Respawn;
        IframesCurrent = Mathf.Max(IframesCurrent, frames);
    }

    /// <summary>Ends world-switch spawn protection early once the player has been
    /// safely placed at the destination spawn (revives to full health/control).</summary>
    public void EndWorldSwitchProtection()
    {
        if (PlayerStatus == PlayerStatus.Respawn)
            Revive();
    }

    public override void Initialize()
    { 
        base.Initialize(); 
        IframesCurrent = 150;
        Storage.info = this;  
        if (BaseHealthMax <= 0) BaseHealthMax = HealthMax;

        IEnumerator HungerClock()
        {
            while (!Destroyed)
            {
                yield return new WaitForSeconds(120);
                // A pet has no rations and no bag: it never goes hungry.
                if (IsPet) continue;
                if (Hunger <= 0)  
                {
                    Health--;
                    Audio.PlaySFX(SfxID.HitPlayer);
                }
                else Hunger--; 
                GUIBar.Update();
            } 
        }
        _ = new CoroutineTask(HungerClock());
    }

    protected override void OnHit(Projectile projectile)
    {
        GUIBar.Update(); 
        if (Main.PlayerInfo == this)
        {
            ScreenShake.Shake(55f, 0.3f, 0.22f);
            ScreenDamageFlash.Show(0.42f, 0.28f);
        }
        Machine.SetState<MobHit>();

        if (projectile.SourceInfo?.Machine is IItemThief)
            DropHeldItem();

        // Whatever hurt a party member is hostile from now on, whoever it hurt.
        if (projectile.SourceInfo is not MobInfo { Machine: not null } attacker) return;
        attacker.MarkHostile();

        // An AI player fights back, dropping whatever structure job it was on — the job stays
        // in WorkOrder so the ally resumes it once this fight ends. (Enemies merely standing
        // nearby never pull an ally off a job; only being hit does.) A human player's target
        // is mouse-driven.
        if (controllerId != -1) return;

        Target = attacker;
        ActionType = IActionType.Hit;
        PathingStatus = PathingStatus.Pending;
    }

    [NonSerialized] public ItemInfo DroppedItem;

    /// <summary>True if the player is alive, active, and holding an item a thief
    /// can steal (gnome/rat check this before engaging).</summary>
    public bool CanBeRobbed =>
        !Destroyed && Health > 0 && PlayerStatus != PlayerStatus.Incapacitated &&
        Equipment != null && !Equipment.isEmpty();

    /// <summary>Only a player with human input takes a target from landing a hit (that is
    /// how swinging at a tree starts mining it). An AI-controlled player's brain owns its
    /// target, so a swing that clips a tree while fighting must not turn into a mining job.</summary>
    public override void AcquireTarget(Info target)
    {
        if (controllerId == -1) return;
        // Hitting a structure is an order to work on it, kept after control moves on.
        if (target is StructureInfo job) WorkOrder = job;
        base.AcquireTarget(target);
    }

    /// <summary>True while the player's current target is a structure it is working on.</summary>
    public bool IsWorking => Target is StructureInfo;

    /// <summary>Stop working: forget the remembered structure job and, when the player is
    /// currently on a structure, drop that target too. Returns true if a structure target
    /// was released — false when the player wasn't working on one.</summary>
    public bool ReleaseWork()
    {
        WorkOrder = null;
        if (!IsWorking) return false;
        CancelTarget();
        return true;
    }

    public ItemSlot HeldSlot =>
        ReferenceEquals(this, Main.PlayerInfo) ? Inventory.Held : Storage?.GetSelected();

    public bool DropHeldItem()
    {
        ItemSlot held = HeldSlot;
        if (held == null || held.isEmpty() || Storage == null) return false;

        DroppedItem = Inventory.DropToWorld(held, held.Stack, Storage, position);
        return true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UpdateMovementHunger();

        switch (PlayerStatus)
        {
            case PlayerStatus.Respawn: // spawn protection until Iframes run out
                if (IframesCurrent > 1) return;
                Revive();
                break;

            case PlayerStatus.Incapacitated: // downed: no control, no movement
                return;

            case PlayerStatus.Active:
                if (Health <= 0)
                {
                    if (Helper.IsHost()) Die();
                    return;
                }
                break;
        }

        UpdateHeldBurn();

        if (Resting)
        {
            Direction = Vector3.zero;
            SpeedTarget = 0;
            RestHeal();
        }

        UpdateWellFed();
        UpdateHungerStage();

        FaceTarget = Equipment != null || Target != null;

        bool isSelected = Main.PlayerInfo == this;
        bool blockedByOther = PlayerSync.IsClaimedByRemoteClient(uid);
        bool claimedByOtherClient = !Helper.IsHost() && !PlayerSync.CanLocalClientControl(uid);

        if (isSelected && !Resting && !blockedByOther && !claimedByOtherClient && (Target == null || !ActionType.IsPickUpOrInteract()))
        {
            TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
            AimPosition = Control.MouseTarget ?
                Control.MouseTarget.transform.position + Vector3.up * 0.55f :
                Control.MousePosition + Vector3.up * 0.15f;
            if (!IsInRenderRange) return;
            if (CurrentHungerStage == HungerStage.IV) { SpeedTarget = 0f; }
            else { SpeedTarget = Control.Inst.Sprint.Key() ? SpeedAir : SpeedGround; HandleMovement(); }
        }
        else if (!isSelected && !blockedByOther && !Resting)
        {
            if (Target != null) AimPosition = Target.position;
            // AI-controlled party members move 1.5x faster 
            SpeedTarget = (IsGrounded ? SpeedGround : SpeedAir) * 1.7f;
        }
        SpeedTarget *= EffectiveSpeedModifier;
 
        
        //fall damage
        if (!IsGrounded && Velocity.y < -10) AirTime += 1;
        else {
            if (AirTime > 75)
            {
                Health += (int)(Velocity.y * 3 / Gravity);
                GUIBar.Update();
                Audio.PlaySFX(SfxID.HitPlayer);
            }
            AirTime = 0;
        }
    }

    private void Die()
    {
        Audio.PlaySFX(DeathSfx);
        bool wasControlled = Main.PlayerInfo == this;

        SkeletonMachine.SpawnRemains(position, Storage);

        PlayerSync.BroadcastPlayerUnload(this);
        EntityMachine?.Unload();
        Save.Inst.players.Remove(this);

        // Delvers are AI-only — their death never triggers party control handoff
        // or the game-over screen (nobody was controlling them).
        if (IsDelver) return;

        // Hand control to the next surviving party member. Covers both the normal
        // case (this was the controlled player) and a dangling Main.PlayerInfo that
        // still points at this removed player.
        bool humansAlive = Save.Inst.players.Exists(p => !p.IsDelver && !p.IsPet && !p.Destroyed && p.Machine != null);
        if (wasControlled || Main.PlayerInfo == this || Main.PlayerInfo?.Machine == null)
        {
            if (!humansAlive)
                GUIMain.GUIMenu?.ShowDeath();
            else
                Control.SetNextPlayer();
        }

        GUIBar.Update();
        GUIMain.SyncHudVisibility();
    }

    // Revive: restore health/control and refresh the local HUD.
    private void Revive()
    {
        // A revived player starts clean: drop lingering debuffs (and their
        // max-health penalties) instead of waking up still on fire.
        Machine?.GetModule<StatusEffectModule>()?.RemoveAll();
        Hunger = HungerMax;
        Health = HealthMax;
        Velocity = Vector2.zero;
        PlayerStatus = PlayerStatus.Active;
        if (Main.PlayerInfo == this)
        {
            if (Equipment != null && SpriteTool != null) SpriteTool.gameObject.SetActive(true);
            GUIBar.Update();
        }
        Inventory.RefreshInventory();
        Machine.SetState<DefaultState>();
    }

    private void UpdateWellFed()
    {
        if (!Helper.IsHost()) return;
        StatusEffectModule module = Machine?.GetModule<StatusEffectModule>();
        if (module == null) return;

        bool wellFed = HungerMax > 0 && Hunger >= HungerMax * WellFedThreshold;
        if (wellFed) module.Apply(WellFed);
        else module.Remove(ID.WellFed);
    }

    private void UpdateHungerStage()
    {
        if (IsPet || !Helper.IsHost()) return;

        HungerStage stage = CurrentHungerStage;
        if (stage == HungerStage.V)
        {
            Health = 0;
            return;
        }

        float multiplier = stage switch
        {
            HungerStage.I => 0.9f,
            HungerStage.II => 0.8f,
            HungerStage.III => 0.7f,
            HungerStage.IV => 0.5f,
            _ => 1f,
        };

        HealthMax = Mathf.Max(1, (int)(BaseHealthMax * multiplier));
        if (Health > HealthMax) Health = HealthMax;
    }

    private void UpdateMovementHunger()
    {
        if (IsPet || !Helper.IsHost() || Resting) { _moveHungerAccumulator = 0; return; }
        if (Direction == Vector3.zero) { _moveHungerAccumulator = 0; return; }
        _moveHungerAccumulator += Helper.GetDeltaTime();
        if (_moveHungerAccumulator < MoveHungerInterval) return;
        _moveHungerAccumulator = 0;
        if (Hunger > 0) Hunger--;
        GUIBar.Update();
    }

    /// <summary>A burning torch consumes itself while carried: the held item spends a
    /// point of durability every <see cref="TorchBurnInterval"/> seconds and is used up
    /// like any other worn-out tool. Only the side that owns this player's inventory
    /// burns it, so a shared slot can never drain twice.</summary>
    private void UpdateHeldBurn()
    {
        ItemSlot held = Equipment;
        if (!IsOwner() || held == null || held.Durability <= 0 || held.Info is not { Glow: true })
        {
            _torchBurnAccumulator = 0f;
            return;
        }

        _torchBurnAccumulator += Helper.GetDeltaTime();
        if (_torchBurnAccumulator < TorchBurnInterval) return;
        _torchBurnAccumulator -= TorchBurnInterval;

        held.Durability--;
        if (held.Durability <= 0)
        {
            held.clear();
            SetEquipment(null);
        }
        Storage?.NotifyChanged();
    }

    private void RestHeal()
    {
        if (!Helper.IsHost() || Health >= HealthMax) { _restHealAccumulator = 0; return; }
        _restHealAccumulator += Helper.GetDeltaTime();
        if (_restHealAccumulator < RestHealInterval) return;
        _restHealAccumulator = 0;
        Health++;
        GUIBar.Update();
    }

    private void HandleMovement()
    { 
        if (IsGrounded)
        {
            _coyoteTimer = CoyoteTime; // Reset coyote timer when grounded
        }
        else
        {
            _coyoteTimer -= Helper.GetDeltaTime();; // Decrease coyote timer when not grounded
        }

        if (Control.Inst.Jump.KeyDown())
        {
            if (Main.Fly)
                Velocity.y = JumpVelocity;
            else 
                _jumpGraceTimer = JumpGraceTime; // Reset jump grace timer when jump key is pressed
        }
        else
        {
            _jumpGraceTimer -= Helper.GetDeltaTime();; // Decrease jump grace timer
        }

        if ((IsGrounded || _coyoteTimer > 0) && _jumpGraceTimer > 0)
        {
            Velocity.y = JumpVelocity;
            //  _isGrounded = false;
            _jumpGraceTimer = 0; // Reset jump grace timer after jumping
        }
        IsGrounded = Velocity.y == 0;
        
        
        
        Vector2 rawInput = Control.GetMovementAxis();
        if (rawInput == Vector2.zero)
        {
            Direction = Vector2.zero;
            return;
        }
        float orbitRotation = Mathf.Deg2Rad * -ViewPort.OrbitRotation;
        float cosAngle = Mathf.Cos(orbitRotation);
        float sinAngle = Mathf.Sin(orbitRotation);
        Direction.x = rawInput.x * cosAngle - rawInput.y * sinAngle;
        Direction.z = rawInput.x * sinAngle + rawInput.y * cosAngle; 
        Direction = Direction.normalized; 
        Direction = new Vector3(
            Direction.x > 0.001f ? 1f : Direction.x < -0.001f ? -1f : 0f,
            0f,
            Direction.z > 0.001f ? 1f : Direction.z < -0.001f ? -1f : 0f
        );
    }
}