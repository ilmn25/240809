using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EntityAbstract : MonoBehaviour
{
    public EntityData entityData;

    public EntityData GetUpdatedEntity()
    {
        entityData.Position = new SerializableVector3(WorldStatic.GetBlockCoordinate(transform.position));
        return entityData;
    }
}
