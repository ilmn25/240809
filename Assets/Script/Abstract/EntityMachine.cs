using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMachine : StateMachine
{ 
    public void WipeEntity()
    {
        GetComponent<EntityHandler>().WipeEntity();
    }
    public ChunkEntityData GetEntityData()
    {
        return GetComponent<EntityHandler>()._entityData;
    }
}

