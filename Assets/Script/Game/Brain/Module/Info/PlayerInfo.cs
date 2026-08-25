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
    public Storage Storage;
    public float Mana;
    public float Sanity;
    public int Hunger;
    public int HungerMax = 20;
    public float Stamina; 
    [NonSerialized] public bool Resting;

    private const float WellFedThreshold = 0.85f;
    private const float MoveHungerInterval = 45f;
    private const float RestHealInterval = 1f;
    private float _moveHungerAccumulator;
    private float _restHealAccumulator;
    private static readonly StatusEffect WellFed = new StatusEffect(
        ID.WellFed, EffectType.Heal, duration: 120f, tickInterval: 60f, amountPerTick: 1, name: "Well Fed");

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
        Machine.SetState<MobHit>();

        if (projectile.SourceInfo?.Machine is IItemThief)
            DropHeldItem();
    }

    [NonSerialized] public ItemInfo DroppedItem;

    /// <summary>True if the player is alive, active, and holding an item a thief
    /// can steal (gnome/rat check this before engaging).</summary>
    public bool CanBeRobbed =>
        !Destroyed && Health > 0 && PlayerStatus != PlayerStatus.Incapacitated &&
        Equipment != null && !Equipment.isEmpty();

    public bool DropHeldItem()
    {
        if (Storage?.List == null || Storage.Key < 0 || Storage.Key >= Storage.List.Count) return false;
        ItemSlot held = Storage.List[Storage.Key];
        if (held == null || held.isEmpty()) return false;

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

        if (isSelected && !Resting && !blockedByOther && !claimedByOtherClient && (Target == null || ActionType != IActionType.PickUp && ActionType != IActionType.Interact))
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
            SpeedTarget = IsGrounded ? SpeedGround + 0.2f : SpeedAir * 2;
        }
        SpeedTarget *= SpeedModifier;
 
        
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

        CorpseMachine.SpawnCorpse(position, CharSprite, Storage);

        PlayerSync.BroadcastPlayerUnload(this);
        EntityMachine?.Unload();
        Save.Inst.players.Remove(this);

        // Hand control to the next surviving party member. Covers both the normal
        // case (this was the controlled player) and a dangling Main.PlayerInfo that
        // still points at this removed player.
        if (wasControlled || Main.PlayerInfo == this || Main.PlayerInfo?.Machine == null)
        {
            if (Save.Inst.players.Count == 0)
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
        if (!Helper.IsHost()) return;

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
        if (!Helper.IsHost() || Resting) { _moveHungerAccumulator = 0; return; }
        if (Direction == Vector3.zero) { _moveHungerAccumulator = 0; return; }
        _moveHungerAccumulator += Helper.GetDeltaTime();
        if (_moveHungerAccumulator < MoveHungerInterval) return;
        _moveHungerAccumulator = 0;
        if (Hunger > 0) Hunger--;
        GUIBar.Update();
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
 
    // for later passive effects boosts
    public float GetRange()
    {
        return 1 * Inventory.CurrentItemData.Range;
    }
    public float GetSpeed()
    {
        return 1 * Inventory.CurrentItemData.Speed;
    } 
}