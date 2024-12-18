using UnityEngine;

class ResourceCollapse : State
{
    private Transform _spriteObject;
    private float _rotationProgress = 0;
    private string _item;
    public ResourceCollapse(Transform spriteObject, string item)
    {
        _item = item;
        _spriteObject = spriteObject;
    }

    public override void OnUpdateState() 
    { 
        _rotationProgress += Time.deltaTime * 0.8f;
        _spriteObject.rotation = Quaternion.Lerp(CreateRotation(0), CreateRotation(90), _rotationProgress);
        if (_spriteObject.rotation.eulerAngles.x > 89)
        {
            _spriteObject.rotation = CreateRotation(0);
            _rotationProgress = 0;
            Entity.SpawnItem(_item, Vector3Int.FloorToInt(Machine.transform.position)); 
            Entity.SpawnItem(_item, Vector3Int.FloorToInt(Machine.transform.position)); 
            Entity.SpawnItem(_item, Vector3Int.FloorToInt(Machine.transform.position)); 
            Entity.SpawnItem(_item, Vector3Int.FloorToInt(Machine.transform.position)); 
            Entity.SpawnItem(_item, Vector3Int.FloorToInt(Machine.transform.position)); 
            SetState<StaticIdle>();
            ((EntityMachine)Machine).Delete();
        }
    }

    private Quaternion CreateRotation(float tilt)
    {
        return Quaternion.Euler(tilt, _spriteObject.rotation.eulerAngles.y, 0);
    }
}