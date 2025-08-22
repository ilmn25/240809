public class ConstructionMachine : EntityMachine, IActionPrimaryResource
{
    public new ConstructionInfo Info => GetModule<ConstructionInfo>();
    public static Info CreateInfo()
    {
        return new ConstructionInfo();
    }
    public override void OnStart()
    {
        Info.SpriteRenderer.sprite = Cache.LoadSprite("Sprite/Construction");
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    }
}