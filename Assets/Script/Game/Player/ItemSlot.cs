using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[System.Serializable]
public class ItemSlot
{
    public int Stack = 0;
    public int Durability;
    public ID ID;
    public string Modifier;
    public bool Locked;

    [OnDeserialized]
    private void MigrateLegacyIds(StreamingContext context)
    {
        if (ID == ID.CopperChunksLegacy) ID = ID.LeadChunks;
    }
    [NonSerialized] private Item _item;
    public Item Info
    {
        get
        {
            if (_item == null || _item.ID != ID)
                _item = Item.GetItem(ID);
            return _item;
        }
    }
    public ItemSlot(){}
    public ItemSlot(ID id, int count = 1)
    {
        ID = id;
        Stack = count;
        Durability = Item.GetItem(id).Durability;
    }
    
    /// <summary>Name plus the stack count for items that stack: "wood x 5".</summary>
    public string Title
    {
        get
        {
            bool stacks = Info.Type is ItemType.Block or ItemType.Material or ItemType.Consumable;
            return stacks ? $"{Info.Name} x {Stack}" : Info.Name;
        }
    }

    /// <summary>Tooltip body: the item description, then one "number label" stat per line.</summary>
    public string Describe()
    {
        string stats = string.Join("\n", StatLines());
        string description = Info.Description?.TrimEnd() ?? "";

        if (stats.Length == 0) return description;
        return description.Length == 0 ? stats : description + "\n\n" + stats;
    }

    /// <summary>Full tooltip: the title, then <see cref="Describe"/> directly beneath it.</summary>
    public string Tooltip()
    {
        string body = Describe();
        return body.Length == 0 ? Title : Title + "\n" + body;
    }

    /// <summary>Every stat this item type has, in a fixed order so the same information always
    /// lands in the same place: what it does, then what it costs to use.</summary>
    private IEnumerable<string> StatLines()
    {
        switch (Info.Type)
        {
            case ItemType.Block:
                foreach (string line in BlockStats())
                    yield return line;
                break;

            case ItemType.Structure:
                foreach (string line in StructureStats())
                    yield return line;
                break;

            case ItemType.Tool:
                foreach (string line in ToolStats())
                    yield return line;
                break;

            case ItemType.Consumable:
                foreach (string line in ConsumableStats())
                    yield return line;
                break;
        }
    }

    /// <summary>How much punishment a placed block takes, and how strong a tool must be
    /// to work it (see Block.BreakCost / Block.BreakThreshold).</summary>
    private IEnumerable<string> BlockStats()
    {
        Block block = Block.GetBlock(ID);
        if (block == null) yield break;

        yield return block.BreakCost + " durability";
        if (block.BreakThreshold > 0)
            yield return block.BreakThreshold + " breaking required";
    }

    /// <summary>Same two numbers for a placeable structure, taken from the machine's
    /// authored info (see Item.InitializeStructureStats).</summary>
    private IEnumerable<string> StructureStats()
    {
        if (!Item.TryGetStructureStats(ID, out (float Health, int Threshold) stats)) yield break;

        yield return stats.Health.ToString("0.#") + " durability";
        if (stats.Threshold > 0)
            yield return stats.Threshold + " breaking required";
    }

    private IEnumerable<string> ConsumableStats()
    {
        if (Info.HealValue > 0)
            yield return Info.HealValue + " health restored";
        if (Info.HungerValue > 0)
            yield return Info.HungerValue + " hunger restored";
        if (Info.MaxHpBonus > 0)
            yield return Info.MaxHpBonus + " max health permanently";
        if (Info.MaxHungerBonus > 0)
            yield return Info.MaxHungerBonus + " max hunger permanently";
    }

    private IEnumerable<string> ToolStats()
    {
        if (Durability != -1)
            yield return Durability + " durability";

        ProjectileInfo projectile = Info.ProjectileInfo;
        if (projectile == null) yield break;

        yield return projectile.Damage + " damage";
        yield return projectile.Knockback + " knockback";
        yield return Info.Speed.ToString("0.##") + " attack speed";

        if (projectile.Breaking > 0)
        {
            string operation = projectile.OperationType switch
            {
                OperationType.Dismantling => "dismantling",
                OperationType.Mining => "mining",
                OperationType.Cutting => "breaking",
                _ => "",
            };
            if (operation.Length > 0)
                yield return projectile.Breaking + " " + operation;
        }

        if (projectile.Ammo != ID.Null)
            yield return "ammo " + (AmmoRegistry.DescribeForGun(ID) ?? projectile.Ammo.ToString());
    }

    public void clear()
    {
        Stack = 0;
        ID = ID.Null;
        Modifier = null;
        Locked = false;
    }
 
    public void Add(ItemSlot slot, int amountToAdd = 0)
    { 
        if (slot.isEmpty()) return; 
        int maxStackSize = slot.Info.StackSize;
        int addableAmount;

        if (amountToAdd == 0)
            addableAmount = Math.Min(slot.Stack, maxStackSize - Stack);
        else
            addableAmount = Math.Min(amountToAdd, Math.Min(slot.Stack, maxStackSize - Stack));

        if (isEmpty())
        {
            ID = slot.ID;
            Modifier = slot.Modifier;
            Locked = slot.Locked;
        }

        if (maxStackSize == 1) Durability = slot.Durability;
        Stack += addableAmount;
        slot.Stack -= addableAmount;

        if (slot.Stack == 0) slot.clear();
    }

    public bool isSame(ItemSlot slot)
    {
        return slot.ID == ID && slot.Modifier == Modifier;
    }
    public bool isSame(ID stringID, string modifier)
    {
        return stringID == ID && modifier == Modifier;
    }
    
    public bool isEmpty()
    {
        return Stack == 0;
    }
    public bool isFull()
    {
        return Stack == Info.StackSize;
    }
     
}