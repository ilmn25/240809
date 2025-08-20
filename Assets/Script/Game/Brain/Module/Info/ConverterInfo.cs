using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class ConverterInfo : ContainerInfo
{
    public readonly List<ID> Pending = new List<ID>();
    public int Max = 10;
    public SfxID Sfx;
    [NonSerialized] private int _counter; 
    public override void Update()
    {
        if (Pending.Count == 0) return;
        if (Sfx != SfxID.Null) Audio.PlaySFX(Sfx);
        if (_counter == ItemRecipe.Dictionary[Pending[0]].Time)
        {
            Vector3 offset = new Vector3(
                Random.value > 0.5f ? 0.65f : -0.65f,
                0.8f, 
                Random.value > 0.5f ? 0.65f : -0.65f);
            Entity.SpawnItem(Pending[0], Machine.transform.position + offset);
            // Helper.Log(Pending[0], Pending.Count);
            Pending.RemoveAt(0);
            _counter = 0;
        }
        else
        {
            _counter++;
        } 
    }

    public bool isFull()
    {
        return Pending.Count == Max;
    }
}