using System.Collections;
using UnityEngine;

/// <summary>A dead player's corpse. An idle, non-AI body that holds the deceased
/// player's inventory; right-click opens it for looting.</summary>
public class CorpseMachine : MobMachine, IActionSecondaryInteract
{
    public new CorpseInfo Info => GetModule<CorpseInfo>();

    public static Info CreateInfo()
    {
        return new CorpseInfo
        {
            Health = 1,
            HealthMax = 1,
            CharSprite = ID.Skeleton,
        };
    }

    public override void OnStart()
    {
        AddState(new InContainerState { Storage = Info.Storage });
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InContainerState>();
        else
            SetState<DefaultState>();
    }

    /// <summary>Spawn a corpse at <paramref name="position"/> carrying the player's
    /// inventory, styled with the dead player's sprite.</summary>
    public static CorpseInfo SpawnCorpse(Vector3 position, ID charSprite, Storage inventory)
    {
        CorpseInfo corpse = (CorpseInfo)Entity.Spawn(ID.Corpse, Vector3Int.FloorToInt(position));
        if (corpse == null) return null;

        corpse.CharSprite = charSprite;
        if (inventory?.List != null)
        {
            foreach (ItemSlot slot in inventory.List)
                if (slot is { Stack: > 0 })
                    corpse.Storage.List.Add(slot);
            inventory.List.Clear();
        }
        _ = new CoroutineTask(DelayedSync(corpse));
        return corpse;
    }

    // Clients learn of the corpse via the entity batch; wait a beat before
    // broadcasting its loot so the storage message isn't dropped on arrival.
    private static IEnumerator DelayedSync(CorpseInfo corpse)
    {
        yield return new WaitForSeconds(0.2f);
        if (corpse != null && corpse.Storage != null)
            StorageSync.Send(corpse.uid, corpse.Storage);
    }
}
