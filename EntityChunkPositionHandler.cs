using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityChunkPositionHandler : MonoBehaviour
{
    private Vector3Int _positionPrevious; 
    private Vector3Int _position;
    private EntityHandler _entityHandler;

    void Start()
    {
        _positionPrevious = ChunkSystem.GetChunkCoordinate(transform.position);
        _entityHandler = GetComponent<EntityHandler>();
        SetLocation();
        EntityLoadSystem.UpdateEntityParent += HandleUpdateParent;
    }
    
    void OnDestroy() {
        EntityLoadSystem.UpdateEntityParent -= HandleUpdateParent;
        if (EntityLoadSystem._entityList.ContainsKey(_positionPrevious))
        EntityLoadSystem._entityList[_positionPrevious].Item2.Remove(_entityHandler); // if picked up
    }

    public void HandleUpdateParent()
    {
        _position = ChunkSystem.GetChunkCoordinate(transform.position);
        if (_position != _positionPrevious)
        {
            // Remove self's EntityHandler from the old coordinate key
            EntityLoadSystem._entityList[_positionPrevious].Item2.Remove(_entityHandler);
            EntityLoadSystem._entityList[_position].Item2.Add(_entityHandler);

            _positionPrevious = _position;
        }
    }

    public void SetLocation()
    {  
        EntityLoadSystem._entityList[_positionPrevious].Item2.Add(_entityHandler);
    }
}
