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

    public static void Set(EnvParticles target)
    {
        foreach (GameObject gameObject in List.Values)
        {
            gameObject.SetActive(false);
        }
        if (target == EnvParticles.Null) return;
        List[target].SetActive(true);
    }
}