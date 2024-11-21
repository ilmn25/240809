using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EntityAbstract : MonoBehaviour
{
    public EntityData _entityData;

    public EntityData GetUpdatedEntity()
    {
        _entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return _entityData;
    }
}
