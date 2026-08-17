/// <summary>Base for furniture: static, unbreakable structures placed directly
/// (no build phase) and picked back up with a hammer. See <see cref="FurnitureInfo"/>.</summary>
public class FurnitureMachine : StructureMachine
{
    protected static FurnitureInfo CreateFurnitureInfo(ID loot, bool glowOn = false)
    {
        return new FurnitureInfo
        {
            Loot = loot,
            SfxHit = SfxID.HitMetal,
            SfxDestroy = SfxID.HitMetal,
            GlowOn = glowOn,
        };
    }
}
