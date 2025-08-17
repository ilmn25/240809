
[System.Serializable]
public class DestructableInfo : StructureInfo
{ 
    public override void OnHit(Projectile projectile)
    {
        PlayerInfo info = (PlayerInfo)projectile.SourceInfo;
        info.Target = Machine.transform;
        info.Action = (IAction)Machine;
        info.ActionType = IActionType.Hit;
    }
    public override void OnDestroy(Projectile projectile)
    {
        ((PlayerInfo)projectile.SourceInfo).Target = null;
    }
}

[System.Serializable]
public class ResourceInfo : DestructableInfo
{
    public override void Initialize()
    {
        operationType = OperationType.Break;
    }
}