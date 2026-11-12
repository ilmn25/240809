using UnityEngine;

/// <summary>A pet — a player-like companion that can be taken over like any party member.
/// It has no inventory at all: while you control it, the only thing it can carry is the
/// item on the cursor. Left to itself it follows the party and attacks with a bear-style
/// bite instead of a weapon.</summary>
public class PetMachine : PlayerMachine
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();

    /// <summary>The pet's natural attack: a bear-style bite, so it needs no weapon and no
    /// bag to fight.</summary>
    private static readonly ProjectileInfo Bite = new ContactDamageProjectileInfo
    {
        Damage = 2,
        Knockback = 12,
        CritChance = 0.1f,
        Radius = 0.9f,
    };

    public static new Info CreateInfo()
    {
        // Start from the exact player stat block, then make it a pet.
        PlayerInfo pet = (PlayerInfo)PlayerMachine.CreateInfo();
        pet.IsPet = true;
        pet.CharSprite = ID.Watchdog;
        // No bag at all: a pet carries nothing (see Inventory.TryPickUpToCursor).
        pet.Storage = new Storage(0);
        pet.HealthMax = 14;
        pet.SpeedLogic = 7;
        pet.SpeedGround = 8;
        pet.SpeedAir = 8;
        pet.Hunger = pet.HungerMax;
        return pet;
    }

    /// <summary>Spawn a pet at <paramref name="position"/> and keep it in the party list,
    /// so it survives save/load and can be selected with the rest of the party.</summary>
    public static PetMachine Spawn(Vector3 position)
    {
        PlayerInfo pet = (PlayerInfo)Entity.CreateInfo(ID.Pet, position);
        Save.Inst.players.Add(pet);
        return (PetMachine)Entity.SpawnFromInfo(pet, false);
    }

    public override void OnStart()
    {
        base.OnStart();
        // Turn the swing the player machine already owns into a natural bite (the bear's
        // attack): it freezes to lunge, then claws whatever is in front of it.
        MobAttackSwing swing = GetState<MobAttackSwing>();
        if (swing != null) swing.NaturalWeapon = Bite;
    }

    /// <summary>A human swings whatever is on the cursor. The AI has no bag to draw a
    /// weapon from, so it bites instead.</summary>
    public override void Attack()
    {
        if (Main.PlayerInfo == Info)
        {
            base.Attack();
            return;
        }

        if (Info.Target == null) return;
        Info.AimPosition = Info.Target.position;
        SetState<MobAttackSwing>();
    }
}
