using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>The Maw's collection pit — an invisible 2x2 area. Any gold dropped
/// into the area is devoured and counted toward the daily quota (see MawQuota).</summary>
public class MawPitMachine : StructureMachine
{
    private const int ProcessInterval = 30; // frames between gold processing (~0.5s at 60fps)
    private int _timer;

    private static readonly Collider[] CollisionArray = new Collider[40];

    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 300,
            Loot = ID.MawPit,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public override void OnSetup()
    {
        base.OnSetup();
        // No sprite — the pit is an invisible 2x2 collection area.
        SpriteRenderer.enabled = false;

        // The transform sits at the anchor block's centre (Floor SpawnOffset adds
        // 0.5), but the 2x2 collider should cover blocks bx..bx+1 / bz..bz+1 —
        // nudge it +0.5 in x/z (and +0.5 in y to sit on the floor) so it isn't
        // offset by half a block.
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
            box.center = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, ProcessInterval); // stagger so pits don't all fire together
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Helper.IsHost()) return;
        if (++_timer < ProcessInterval) return;
        _timer = 0;

        CountGoldInArea();
    }

    /// <summary>Counts (and devours) gold items resting inside the pit's 2x2 area,
    /// crediting each to the daily quota.</summary>
    private void CountGoldInArea()
    {
        // Same +0.5 offset as the collider so the box covers the 2x2 block area
        // (blocks bx..bx+1 / bz..bz+1) instead of being shifted half a block.
        Vector3 center = transform.position + new Vector3(0.5f, 0.5f, 0.5f);
        int count = Physics.OverlapBoxNonAlloc(
            center,
            new Vector3(1f, 0.75f, 1f), // 2x2 area, tall enough to catch resting items
            CollisionArray,
            Quaternion.identity,
            Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            Collider col = CollisionArray[i];
            if (col.gameObject == gameObject) continue;
            if (col.gameObject.name != "ItemPrefab") continue;

            ItemMachine itemMachine = col.GetComponent<ItemMachine>();
            if (itemMachine == null || itemMachine.Info is not ItemInfo item) continue;
            if (item.item == null || item.item.ID != ID.Gold) continue;

            MawQuota.Deposit(item.item.Stack);
            item.Destroy();
        }
    }
}
