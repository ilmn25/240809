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

        // when true, AddItem/RemoveItem won't trigger Inventory.RefreshInventory
        [NonSerialized] public bool SuppressRefresh;
        // when true, Storage.NotifyChanged won't fire Storage.OnChanged (used during bulk transfers)
        [NonSerialized] public bool SuppressSync;

        /// <summary>Fires when the storage contents change.  StorageSync subscribes to this
        /// to immediately sync inventory changes to the host.</summary>
        public static event Action<Storage> OnChanged;

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
                if (!SuppressSync) OnChanged?.Invoke(this);
                if (info is not MobInfo || List == null || List.Count == 0) return;
                ApplySelectedEquipment();
                if (!SuppressRefresh) Inventory.RefreshInventory();
        }

        /// <summary>Hold whatever the selected slot contains (nothing when it is empty),
        /// so the owner renders the right item in hand.</summary>
        public void ApplySelectedEquipment()
        {
                if (info is not MobInfo mobInfo) return;
                ItemSlot selected = GetSelected();
                mobInfo.SetEquipment(selected is { Stack: > 0 } ? selected : null);
        }

        /// <summary>The currently selected (highlighted) slot, clamped to valid
        /// bounds. Returns null when the storage is empty.</summary>
        public ItemSlot GetSelected()
        {
            if (List == null || List.Count == 0) return null;
            return List[Mathf.Clamp(Key, 0, List.Count - 1)];
        }

        /// <summary>Whether a slot's contents could be added: there is an empty slot, or a
        /// slot holding the same item has room left to stack.</summary>
        public bool CanFit(ItemSlot slot)
        {
                if (List == null) return false;
                foreach (ItemSlot owned in List)
                {
                        if (owned == null) continue;
                        if (owned.isEmpty() || owned.isSame(slot) && !owned.isFull()) return true;
                }
                return false;
        }

        /// <summary>Hold the best carried tool for the given operation. False when the bag
        /// has no tool for it.</summary>
        public bool SetTool(OperationType operation)
        {
                return EquipBest(item => item.ProjectileInfo.OperationType == operation
                        ? item.ProjectileInfo.Breaking
                        : null);
        }

        /// <summary>Hold the best carried weapon, so an ally that was working with a tool can
        /// fight. A real weapon (2+ damage) wins, but any damaging item beats an empty hand —
        /// gathering tools deal 1 damage, so an ally holding only a pickaxe still fights.
        /// Items that can't land a hit (no ammo, 0-damage hammer) never qualify.</summary>
        public bool SetWeapon()
        {
                return EquipBest(item => CanUseAsWeapon(item.ID, item.ProjectileInfo)
                        ? item.ProjectileInfo.Damage
                        : null);
        }

        /// <summary>Whether this item can actually land a hit: it deals damage and, if it
        /// needs ammo, the owner is carrying some.</summary>
        private bool CanUseAsWeapon(ID itemID, ProjectileInfo projectile)
        {
                if (projectile.Damage < 1) return false;
                if (AmmoRegistry.IsGun(itemID)) return AmmoRegistry.PickFor(itemID, this) != global::ID.Null;
                return projectile.Ammo == global::ID.Null || GetAmount(projectile.Ammo) > 0;
        }

        /// <summary>Hold the carried item with the highest score. A null score means the item
        /// is not eligible; a score of 0 or less never beats an empty hand.</summary>
        private bool EquipBest(Func<Item, int?> score)
        {
                int targetKey = -1;
                int targetScore = 0;
                for (int i = 0; i < List.Count; i++)
                {
                        Item item = Item.GetItem(List[i].ID);
                        if (item?.ProjectileInfo == null) continue;
                        int? value = score(item);
                        if (value == null || value <= targetScore) continue;
                        targetScore = value.Value;
                        targetKey = i;
                }

                if (targetKey == -1) return false;
                Key = targetKey;
                ((MobInfo)info).SetEquipment(List[Key]);
                return true;
        }

        /// <summary>Drop everything in this storage on the ground around
        /// <paramref name="position"/>, emptying it. One ground item per slot.</summary>
        public void DropAll(Vector3 position, bool stackOnSpawn = true)
        {
                if (List == null) return;
                foreach (ItemSlot itemSlot in List)
                {
                        if (itemSlot is not { Stack: > 0 }) continue;
                        Entity.SpawnItem(itemSlot, position, stackOnSpawn);
                }
                List.Clear();
        }

        public void Explode(Vector3 position)
        {
                if (!Helper.IsHost()) return;
                DropAll(position);
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

        public int Count(ID itemID)
        {
                int count = 0;
                if (List == null) return 0;
                foreach (ItemSlot slot in List)
                        if (slot.ID == itemID)
                                count += slot.Stack;
                return count;
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
                                if (Helper.IsHost())
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
