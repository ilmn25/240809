using System.Collections.Generic;
using UnityEngine;

public sealed class ItemAffix
{
    public readonly string Name;
    public readonly float Damage;
    public readonly float Crit;
    public readonly float Knockback;
    public readonly float Speed;
    public readonly float Durability;

    public ItemAffix(string name, float damage = 1f, float crit = 0f, float knockback = 1f, float speed = 1f, float durability = 1f)
    {
        Name = name;
        Damage = damage;
        Crit = crit;
        Knockback = knockback;
        Speed = speed;
        Durability = durability;
    }
}

public static class ItemAffixes
{
    private const float LootChance = 0.2f;
    private const float CraftChance = 0.25f;

    private static readonly ItemAffix Sharp = new("Sharp", damage: 1.15f);
    private static readonly ItemAffix Keen = new("Keen", crit: 5f);
    private static readonly ItemAffix Swift = new("Swift", speed: 1.2f);
    private static readonly ItemAffix Heavy = new("Heavy", knockback: 1.3f, speed: 0.9f);
    private static readonly ItemAffix Legendary = new("Legendary", damage: 1.3f, crit: 8f, knockback: 1.25f, speed: 1.15f, durability: 1.5f);
    private static readonly ItemAffix Dull = new("Dull", damage: 0.85f);
    private static readonly ItemAffix Frail = new("Frail", durability: 0.5f);
    private static readonly ItemAffix Broken = new("Broken", damage: 0.7f, durability: 0.4f);

    private static readonly ItemAffix[] RollPool =
    {
        Sharp, Sharp, Sharp, Keen, Keen, Swift, Swift, Heavy, Legendary, Dull, Frail, Broken,
    };

    private static readonly Dictionary<string, ItemAffix> ByName = Build();

    private static Dictionary<string, ItemAffix> Build()
    {
        Dictionary<string, ItemAffix> map = new();
        foreach (ItemAffix affix in RollPool)
            map[affix.Name] = affix;
        return map;
    }

    public static ItemAffix Get(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        return ByName.TryGetValue(name, out ItemAffix affix) ? affix : null;
    }

    public static float SpeedOf(string name) => Get(name)?.Speed ?? 1f;

    public static void Apply(ItemSlot slot, float chance)
    {
        if (slot is not { Stack: > 0 }) return;
        if (slot.Info?.Type != ItemType.Tool || slot.Modifier != null) return;
        if (Random.value > chance) return;

        ItemAffix affix = RollPool[Random.Range(0, RollPool.Length)];
        slot.Modifier = affix.Name;
        if (slot.Info.Durability > 0 && affix.Durability != 1f)
            slot.Durability = Mathf.Max(1, Mathf.RoundToInt(slot.Info.Durability * affix.Durability));
    }

    public static void ApplyLoot(ItemSlot slot) => Apply(slot, LootChance);

    public static void ApplyCraft(ItemSlot slot) => Apply(slot, CraftChance);
}
