[System.Serializable]
public class ContainerInfo : StructureInfo
{
    public Storage Storage;
    public override void Initialize()
    { 
        operationType = OperationType.Break;
    }

    public override void OnDestroy(Projectile projectile)
    {
        Storage.Explode(Machine.transform.position);
    }
}