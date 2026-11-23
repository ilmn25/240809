[System.Serializable]
public class ContainerInfo : SpriteStructureInfo
{
    public Storage Storage;
    public override void Initialize()
    { 
        base.Initialize();
        Storage.info = this;
        operationType = OperationType.Chopping;
    }

    public override void OnDestroy(MobInfo info)
    {
        Storage.Explode(Machine.transform.position);
    }
}