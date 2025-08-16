public class DestructableInfo : StructureInfo
{
    public override void OnHit(Projectile projectile)
    {
        PlayerInfo info = (PlayerInfo)projectile.SourceInfo;
        info.Target = Machine.transform;
        info.Action = (IAction)Machine;
        info.ActionTarget = IActionTarget.Hit;
    }
    public override void OnDestroy(Projectile projectile)
    {
        ((PlayerInfo)projectile.SourceInfo).Target = null;
    }
}