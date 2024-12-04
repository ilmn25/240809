using System;

[System.Serializable]
public class InvSlotData
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
 
    public void Add(InvSlotData slotData, int amountToAdd = 0)
    {
        if (slotData.isEmpty()) return;
        int maxStackSize = ItemSingleton.GetItem(slotData.StringID).StackSize;
        int addableAmount;

        if (amountToAdd == 0)
            addableAmount = Math.Min(slotData.Stack, maxStackSize - Stack);
        else
            addableAmount = Math.Min(amountToAdd, Math.Min(slotData.Stack, maxStackSize - Stack));

        if (isEmpty())
        {
            StringID = slotData.StringID;
            Modifier = slotData.Modifier;
            Locked = slotData.Locked;
        }
        
        Stack += addableAmount;
        slotData.Stack -= addableAmount;

        if (slotData.Stack == 0) slotData.clear();
    }

    public bool isSame(InvSlotData slotData)
    {
        return slotData.StringID == StringID && slotData.Modifier == Modifier;
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