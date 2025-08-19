public class ConstructionMachine : EntityMachine, IActionPrimaryResource
{
    public new ConstructionInfo Info => GetModule<ConstructionInfo>();
    public static Info CreateInfo()
    {
        return new ConstructionInfo();
    }
    public override void OnStart()
    {
        Info.SpriteRenderer.sprite = Cache.LoadSprite("sprite/construction");
        AddModule(new StructureSpriteCullModule()); 
        AddModule(new SpriteOrbitModule()); 
    }
}