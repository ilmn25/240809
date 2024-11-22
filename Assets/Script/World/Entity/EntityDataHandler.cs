using UnityEngine;

public class EntityDataHandler : MonoBehaviour
{
    public EntityData _entityData;
    private Vector3Int _position;
    private Vector3Int _positionPrevious;  

    public EntityData GetUpdatedEntity()
    {
        _entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return _entityData;
    }
     
    void Start()
    {
        EntityLoadStatic.UpdateEntityParent += HandleUpdateParent;
        _positionPrevious = WorldStatic.GetChunkCoordinate(transform.position);
        EntityLoadStatic._entityList[_positionPrevious].Item2.Add(this); 
    }
    
    void OnDestroy() {
        EntityLoadStatic.UpdateEntityParent -= HandleUpdateParent;
        if (EntityLoadStatic._entityList.ContainsKey(_positionPrevious)) 
            EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(this); // if picked up
    }

    public void HandleUpdateParent()
    {
        _position = WorldStatic.GetChunkCoordinate(transform.position);
        if (_position != _positionPrevious && EntityLoadStatic._entityList.ContainsKey(_position))
        {
            // Remove self's EntityHandler from the old coordinate key
            EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(this);
            EntityLoadStatic._entityList[_position].Item2.Add(this);

            _positionPrevious = _position;
        }
    }
} 