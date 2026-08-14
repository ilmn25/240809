using System;

/// <summary>Info for the gnome. It carries a storage of stolen items. On death it
/// drops everything it stole back onto the ground.</summary>
[System.Serializable]
public class GnomeInfo : EnemyInfo
{
    /// <summary>Items the gnome has stolen from the ground.</summary>
    public Storage Stolen = new Storage(8);

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (Health <= 0)
            DropStolen();
    }

    /// <summary>Drop all stolen items back onto the ground.</summary>
    public void DropStolen()
    {
        if (Stolen == null || Stolen.List == null) return;
        foreach (ItemSlot slot in Stolen.List)
        {
            if (slot == null || slot.isEmpty()) continue;
            Entity.SpawnItem(slot, Machine.transform.position, stackOnSpawn: false);
        }
        Stolen = null;
    }
}
