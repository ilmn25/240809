 
[System.Serializable]
public class ResourceInfo : StructureInfo
{
    public override void Initialize()
    {
        base.Initialize();
        operationType = OperationType.Breaking;
    }
}