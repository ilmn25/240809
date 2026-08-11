using UnityEngine;

/// <summary>A grave marker found in graveyard clusters. Purely decorative;
/// can be mined away like other stone structures.</summary>
public class HeadstoneMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 20,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
        };
    }
}
