using System;

[System.Serializable]
public class ItemSlot
{
    public int Stack = 0;
    public string StringID;
    public string Modifier;
    public bool Locked; 
    
    public void SetItem(int stack, string stringID, string modifier, bool locked)
    {
        Stack = stack;
        StringID = stringID;
        Modifier = modifier;
        Locked = locked;
    }

    public void clear()
    {
        Stack = 0;
        StringID = null;
        Modifier = null;
        Locked = false;
    }
 
    public void Add(ItemSlot slot, int amountToAdd = 0)
    {
        if (slot.isEmpty()) return;
        int maxStackSize = Item.GetItem(slot.StringID).StackSize;
        int addableAmount;

        if (amountToAdd == 0)
            addableAmount = Math.Min(slot.Stack, maxStackSize - Stack);
        else
            addableAmount = Math.Min(amountToAdd, Math.Min(slot.Stack, maxStackSize - Stack));

        if (isEmpty())
        {
            StringID = slot.StringID;
            Modifier = slot.Modifier;
            Locked = slot.Locked;
        }
        
        Stack += addableAmount;
        slot.Stack -= addableAmount;

        if (slot.Stack == 0) slot.clear();
    }

    public bool isSame(ItemSlot slot)
    {
        return slot.StringID == StringID && slot.Modifier == Modifier;
    }
    public bool isSame(string stringID, string modifier)
    {
        return stringID == StringID && modifier == Modifier;
    }
    
    public bool isEmpty()
    {
        return Stack == 0;
    }
}