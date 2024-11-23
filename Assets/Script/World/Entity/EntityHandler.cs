using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    public EntityData _entityData;
    private Vector3Int _positionCurrent;
    private Vector3Int _positionPrevious;  

    public EntityData GetUpdatedEntity()
    {
        _entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return _entityData;
    }
     
    void Start()
    {
        EntityLoadStatic.UpdateEntityListKey += UpdateEntityListKey;
        
        _positionPrevious = WorldStatic.GetChunkCoordinate(transform.position);
        EntityLoadStatic._entityList[_positionPrevious].Item2.Add(this); 
    }
     
    public void OnDestroy() {
        EntityLoadStatic.UpdateEntityListKey -= UpdateEntityListKey;
        
        if (EntityLoadStatic._entityList.ContainsKey(_positionPrevious)) 
            EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(this); // if picked up
    }

    public void UpdateEntityListKey()
    {
        _positionCurrent = WorldStatic.GetChunkCoordinate(transform.position);
        if (_positionCurrent != _positionPrevious && EntityLoadStatic._entityList.ContainsKey(_positionCurrent))
        {
            // Remove self's EntityHandler from the old coordinate key
            EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(this);
            EntityLoadStatic._entityList[_positionCurrent].Item2.Add(this);

            _positionPrevious = _positionCurrent;
        }
    }
} 