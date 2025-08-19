using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Storage
{
        public static readonly Dictionary<int, List<ItemSlot>> Dictionary = new Dictionary<int, List<ItemSlot>>();
        
        public List<ItemSlot> List;
        public int ID;
        public int Key;
        
        public Storage(int size)
        {
                List = new List<ItemSlot>(size);
                for (int i = 0; i < size; i++)
                        List.Add(new ItemSlot());
                int id = Dictionary.Count;
                Dictionary.Add(id, List);
        } 

        public void Explode(Vector3 position)
        {
                foreach (ItemSlot itemSlot in List)
                {
                        if (itemSlot.Stack == 0) continue;
                        Entity.SpawnItem(itemSlot.StringID, Vector3Int.FloorToInt(position), itemSlot.Stack);
                }
                Dictionary.Remove(ID);
        }
        
        public void RemoveItem(ID stringID, int quantity = 1, int priority = 0)
        {
                // Prioritize current slot
                if (List[priority].StringID == stringID)
                {
                        int removableAmount = Math.Min(quantity, List[priority].Stack);
                        List[priority].Stack -= removableAmount;
                        quantity -= removableAmount;
                        if (List[priority].Stack <= 0) List[priority].clear();
                        if (quantity <= 0)
                        { 
                                // RefreshInventory();
                                return;
                        }
                }

                // Continue with other slots if necessary
                foreach (var slot in List)
                {
                        if (slot.StringID == stringID)
                        {
                                int removableAmount = Math.Min(quantity, slot.Stack);
                                slot.Stack -= removableAmount;
                                quantity -= removableAmount;
                                if (slot.Stack <= 0)
                                {
                                        slot.clear();
                                }
                                if (quantity <= 0)
                                {
                                        // RefreshInventory();
                                        return;
                                }
                        }
                } 
                // RefreshInventory();
        }
                
        public void AddItem(ID stringID, int quantity = 1, int priority = 0, Vector3 position = default)
        {   
                int maxStackSize = Item.GetItem(stringID).StackSize;
                
                // First try to add to the current slot
                if (List[priority].StringID == stringID && List[priority].Stack < maxStackSize)
                {
                        int addableAmount = Math.Min(quantity, maxStackSize - List[priority].Stack);
                        List[priority].Stack += addableAmount;
                        quantity -= addableAmount;

                        if (quantity <= 0)
                        {
                                // RefreshInventory();
                                return;
                        }
                }

                // Try to add to existing slots with the same item
                foreach (var slot in List)
                {
                        if (slot.StringID == stringID && slot.Stack < maxStackSize)
                        {
                                int addableAmount = Math.Min(quantity, maxStackSize - slot.Stack);
                                slot.Stack += addableAmount;
                                quantity -= addableAmount;

                                if (quantity <= 0)
                                { 
                                        // RefreshInventory();
                                        return;
                                }
                        }
                }

                // If there's still quantity left, find new slots
                while (quantity > 0)
                { 
                        int slotID = GetEmptySlot();
                        if (slotID == -1)
                        {
                                Entity.SpawnItem(stringID, position, quantity);
                                break;
                        }
                        int addableAmount = Math.Min(quantity, maxStackSize - List[slotID].Stack);
                        List[slotID].SetItem(List[slotID].Stack + addableAmount, stringID, List[slotID].Modifier, List[slotID].Locked);
                        quantity -= addableAmount;
                }

                // RefreshInventory();
        }
        
        public int GetAmount(ID stringID)
        {
                int count = 0;
                foreach (var slot in List)
                {
                        if (slot.StringID == stringID)
                        { 
                                count += slot.Stack;
                        }
                }
                return count;
        }

        private int GetEmptySlot()
        {
                int slotID = 0;
                while (List[slotID].Stack != 0)
                {
                        slotID++;
                        if (slotID == List.Count)
                                return -1;
                }
                return slotID;
        }

}