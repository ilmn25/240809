using System;
using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    public EntityData _entityData;
    private Vector3Int _positionCurrent;
    private Vector3Int _positionPrevious;

    public EntityData GetEntityData()
    {
        _entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return _entityData;
    }

    private void Awake()
    {
        EntityLoadStatic.UpdateEntityListKey += UpdateEntityListKey;
    }

    private void OnDisable()
    {
        EntityLoadStatic.UpdateEntityListKey -= UpdateEntityListKey;
    }

    private void OnEnable()
    {
        EntityLoadStatic.UpdateEntityListKey += UpdateEntityListKey;
    }

    public void Initialize(EntityData entityData, Vector3Int chunkPosition) { 
        _entityData = entityData;
        _positionPrevious = chunkPosition;
    }

    public void UpdateEntityListKey()
    {
        try
        {   
            if (!isActiveAndEnabled) return;
            _positionCurrent = WorldStatic.GetChunkCoordinate(transform.position);
            if (_positionCurrent != _positionPrevious && EntityLoadStatic._entityList.ContainsKey(_positionCurrent))
            {
                // Remove self's EntityHandler from the old coordinate key
                EntityLoadStatic._entityList[_positionPrevious].Item2.Remove(this);
                EntityLoadStatic._entityList[_positionCurrent].Item2.Add(this);

                _positionPrevious = _positionCurrent;
            }
        }
        catch 
        {
            Lib.Log(_entityData.ID, _entityData.Position, isActiveAndEnabled);
        }
    }
} 