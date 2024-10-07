using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHandler : MonoBehaviour
{
    public Entity _entity;
    // void Start()
    // { 
    // }

    public Entity GetUpdatedEntity()
    {
        _entity.Position = new SerializableVector3(ChunkSystem.GetBlockCoordinate(transform.position));
        return _entity;
    }
}
