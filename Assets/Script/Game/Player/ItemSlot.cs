using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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

    /// <summary>Tooltip body: one "label value" stat per line, then the item description.</summary>
    public string Describe() => JoinSections(string.Join("\n", StatLines()), Info.Description);

    /// <summary>Full tooltip: the title, then <see cref="Describe"/> directly beneath it.</summary>
    public string Tooltip()
    {
        string body = Describe();
        return body.Length == 0 ? Title : Title + "\n" + body;
    }

    /// <summary>Joins the non-empty sections with a single blank line.</summary>
    private static string JoinSections(params string[] sections)
    {
        StringBuilder text = new StringBuilder();
        foreach (string section in sections)
        {
            if (string.IsNullOrWhiteSpace(section)) continue;
            if (text.Length > 0) text.Append("\n\n");
            text.Append(section.TrimEnd());
        }
        return text.ToString();
    }

    /// <summary>Every stat this item type has, in a fixed order so the same information always
    /// lands in the same place: what it does, then what it costs to use.</summary>
    private IEnumerable<string> StatLines()
    {
        switch (Info.Type)
        {
            case ItemType.Structure:
                yield return "structure";
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

    private IEnumerable<string> ConsumableStats()
    {
        if (Info.HealValue > 0)
            yield return "restores " + Info.HealValue + " health";
        if (Info.HungerValue > 0)
            yield return "restores " + Info.HungerValue + " hunger";
        if (Info.MaxHpBonus > 0)
            yield return "permanently increases max health by " + Info.MaxHpBonus;
        if (Info.MaxHungerBonus > 0)
            yield return "permanently increases max hunger by " + Info.MaxHungerBonus;
    }

    private IEnumerable<string> ToolStats()
    {
        if (Durability != -1)
            yield return "durability " + Durability;

        ProjectileInfo projectile = Info.ProjectileInfo;
        if (projectile == null) yield break;

        yield return "damage " + projectile.Damage;
        yield return "knockback " + projectile.Knockback;

        string breaking = projectile.Breaking == 0
            ? ""
            : projectile.OperationType switch
            {
                OperationType.Building => "building " + projectile.Breaking,
                OperationType.Mining => "mining " + projectile.Breaking,
                OperationType.Cutting => "breaking " + projectile.Breaking,
                _ => "",
            };
        if (breaking.Length > 0)
            yield return breaking;

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