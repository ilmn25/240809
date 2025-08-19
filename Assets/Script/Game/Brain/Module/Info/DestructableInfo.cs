
[System.Serializable]
public class DestructableInfo : StructureInfo
{ 
    public override void OnHit(Projectile projectile)
    {
        PlayerInfo info = (PlayerInfo)projectile.SourceInfo;
        info.Target = this;
        info.ActionType = IActionType.Hit;
    } 
}

[System.Serializable]
public class ResourceInfo : DestructableInfo
{
    public override void Initialize()
    {
        base.Initialize();
        operationType = OperationType.Break;
    }
}