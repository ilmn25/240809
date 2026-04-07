public class ConstructionMachine : EntityMachine, IActionPrimaryResource, IActionSecondaryInteract
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

    public void OnActionSecondary(Info info)
    {
        Dialogue.Target = new Dialogue { Text = "I need to assemble it" };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }
}