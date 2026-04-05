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
        public Info info;

        public string Name;

        // when true, AddItem/RemoveItem won't trigger Inventory.RefreshInventory
        [NonSerialized] public bool SuppressRefresh;

        public Storage()
        {
        }

        public Storage(int size)
        {
                List = new List<ItemSlot>(size);
                for (int i = 0; i < size; i++)
                        List.Add(new ItemSlot());
                int id = Dictionary.Count;
                Dictionary.Add(id, List);
        }
        public void NotifyChanged()
        {
                if (info is not MobInfo mobInfo || List == null || List.Count == 0) return;
                ItemSlot selectedSlot = List[Key];
                mobInfo.SetEquipment(selectedSlot is { Stack: > 0 } ? selectedSlot : null);
                if (!SuppressRefresh) Inventory.RefreshInventory();
        }

        public bool SetTool(OperationType operation)
        {
                int targetKey = -1;
                int targetBreaking = 0;
                for (int i = 0; i < List.Count; i++)
                {
                        Item item = Item.GetItem(List[i].ID);
                        if (item == null || item.ProjectileInfo == null) continue;
                        if (operation == item.ProjectileInfo.OperationType &&
                            targetBreaking < item.ProjectileInfo.Breaking)
                        {
                                targetBreaking = item.ProjectileInfo.Breaking;
                                targetKey = i;
                        }
                }

                if (targetKey == -1) return false;
                Key = targetKey;
                ((MobInfo)info).SetEquipment(List[Key]);
                return true;
        }

        public void Explode(Vector3 position)
        {
                foreach (ItemSlot itemSlot in List)
                {
                        if (itemSlot.isEmpty()) continue;
                        Entity.SpawnItem(itemSlot, Vector3Int.FloorToInt(position));
                }

                Dictionary.Remove(ID);
        }

        public virtual int RemoveItem(ID stringID, int quantity = 1, int priority = 0)
        {
                int remaining = quantity;
                
                // Prioritize current slot
                if (List[priority].ID == stringID)
                {
                        int removableAmount = Math.Min(remaining, List[priority].Stack);
                        List[priority].Stack -= removableAmount;
                        remaining -= removableAmount;
                        if (List[priority].Stack <= 0) List[priority].clear();
                        if (remaining <= 0)
                        {
                                NotifyChanged();
                                return 0;
                        }
                }

                // Continue with other slots if necessary
                foreach (var slot in List)
                {
                        if (slot.ID == stringID)
                        {
                                int removableAmount = Math.Min(remaining, slot.Stack);
                                slot.Stack -= removableAmount;
                                remaining -= removableAmount;
                                if (slot.Stack <= 0)
                                {
                                        slot.clear();
                                }

                                if (remaining <= 0)
                                {
                                        NotifyChanged();
                                        return 0;
                                }
                        }
                }

                NotifyChanged();
                return remaining;
        }

        public void CreateAndAddItem(ID stringID, int count = 1)
        {
                AddItem(new ItemSlot(stringID, count));
        }

        public virtual void AddItem(ItemSlot newItemSlot, int priority = 0)
        {
                if (List[priority].isSame(newItemSlot))
                {
                        List[priority].Add(newItemSlot);
                        if (newItemSlot.isEmpty())
                        {
                                NotifyChanged();
                                return;
                        }
                }

                foreach (var slot in List)
                {
                        if (slot.isSame(newItemSlot))
                        {
                                slot.Add(newItemSlot);
                                if (newItemSlot.isEmpty())
                                {
                                        NotifyChanged();
                                        return;
                                }
                        }
                }

                while (!newItemSlot.isEmpty())
                {
                        int slotID = GetEmptySlot();
                        if (slotID == -1)
                        {
                                Entity.SpawnItem(newItemSlot, info.position);
                                break;
                        }

                        List[slotID].Add(newItemSlot);
                }

                NotifyChanged();
        }

        public int GetAmount(ID stringID)
        {
                int count = 0;
                foreach (var slot in List)
                {
                        if (slot.ID == stringID)
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

[Serializable]
public class NoRefreshStorage : Storage
{
    public NoRefreshStorage(int size) : base(size) { SuppressRefresh = true; }
}
