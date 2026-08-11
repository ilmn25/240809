using System;
using UnityEngine;

/// <summary>Info for the tree mimic. It looks like a harmless tree and follows the
/// player from a distance at night. When struck it "realizes" — drops a little loot
/// and flees — and drops more loot if actually killed.</summary>
[System.Serializable]
public class TreeMimicInfo : PassiveInfo
{
    /// <summary>Set once when the mimic is first hit, so it only drops the
    /// "startled" loot a single time before fleeing.</summary>
    [NonSerialized] public bool Startled;

    protected override void OnHit(Projectile projectile)
    {
        if (!Startled)
        {
            Startled = true;
            Loot.Gettable(((EntityMachine)Machine).Info.id).SpawnOneRandom(Machine.transform.position);
        }

        Target = projectile.SourceInfo;
        if (Machine is TreeMimicMachine mimic)
            mimic.StartFlee();
        else
            Machine.SetState<MobEscape>();
    }
}
