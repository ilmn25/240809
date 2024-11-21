using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityChunkPositionInst : MonoBehaviour
{
    private Vector3Int _positionPrevious; 
    private Vector3Int _position;
    private EntityAbstract _entityAbstract;

    void Start()
    {
        _positionPrevious = WorldStatic.GetChunkCoordinate(transform.position);
        _entityAbstract = GetComponent<EntityAbstract>();
        SetLocation();
        EntityLoadStatic.UpdateEntityParent += HandleUpdateParent;
    }
    
    void OnDestroy() {
        EntityLoadStatic.UpdateEntityParent -= HandleUpdateParent;
        if (EntityLoadStatic._entityList.ContainsKey(_positionPrevious))
        EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(_entityAbstract); // if picked up
    }

    public void HandleUpdateParent()
    {
        _position = WorldStatic.GetChunkCoordinate(transform.position);
        if (_position != _positionPrevious)
        {
            // Remove self's EntityHandler from the old coordinate key
            EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(_entityAbstract);
            EntityLoadStatic._entityList[_position].Item2.Add(_entityAbstract);

            _positionPrevious = _position;
        }
    }

    public void SetLocation()
    {  
        EntityLoadStatic._entityList[_positionPrevious].Item2.Add(_entityAbstract);
    }
}
