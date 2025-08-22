 
[System.Serializable]
public class ResourceInfo : SpriteStructureInfo
{
    public override void Initialize()
    {
        base.Initialize();
        operationType = OperationType.Breaking;
    }
}