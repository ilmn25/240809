using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ConverterInfo : ContainerInfo
{
    protected abstract IReadOnlyList<ID> GetOutputs(ID input);
    protected virtual int OutputAmount(ID input) => 1;
    protected virtual int ConvertTime => 90;

    [NonSerialized] private int _counter;

    public bool IsConverting()
    {
        if (Storage == null || Storage.List == null) return false;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            if (GetOutputs(slot.ID) != null) return true;
        }
        return false;
    }

    public override void Update()
    {
        if (!Helper.IsHost()) return;
        if (Storage == null || Storage.List == null) return;
        if (Machine != null && Machine.IsCurrentState<InContainerState>()) return;

        bool changed = false;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            if (GetOutputs(slot.ID) != null) continue;
            Entity.SpawnItem(slot, Machine.transform.position + OutputOffset());
            slot.clear();
            changed = true;
        }
        if (changed)
            Storage.NotifyChanged();

        ItemSlot target = null;
        foreach (ItemSlot slot in Storage.List)
        {
            if (slot.isEmpty()) continue;
            target = slot;
            break;
        }
        if (target == null)
        {
            _counter = 0;
            return;
        }

        if (_counter == ConvertTime)
        {
            foreach (ID outId in GetOutputs(target.ID))
                Entity.SpawnItem(outId, Machine.transform.position + OutputOffset(), OutputAmount(target.ID), stackOnSpawn: false);
            target.clear();
            _counter = 0;
            Storage.NotifyChanged();
        }
        else
        {
            _counter++;
        }
    }
}
