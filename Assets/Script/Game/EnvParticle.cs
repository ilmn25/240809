using System;
using System.Collections.Generic;
using UnityEngine;
public enum EnvParticles {Null, Rain, Leaf, Snow}

public static class EnvParticle
{
    private static readonly Dictionary<EnvParticles, GameObject> List = new ();

    public static void Initialize()
    {
        foreach (EnvParticles target in Enum.GetValues(typeof(EnvParticles)))
        {
            if (target == EnvParticles.Null) continue;
            List.Add(target, Main.ViewPortObject.transform.Find(target.ToString()).gameObject); 
        }
    }

    public static bool Set(EnvParticles target)
    {
        if (target == EnvParticles.Null)
        {
            return false;
        }

        GameObject particleObject = List[target];
        bool currentlyActive = particleObject.activeSelf;
        particleObject.SetActive(!currentlyActive);
        return !currentlyActive;
    }
}