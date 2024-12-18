using System;
using UnityEngine;

public class ControlKey
{
    public KeyCode Primary;
    public KeyCode Secondary;
    public readonly KeyCode[] Modifier;

    public ControlKey(KeyCode primary = KeyCode.None, KeyCode secondary = KeyCode.None, KeyCode[] modifier = null)
    {
        Primary = primary;
        Secondary = secondary;
        Modifier = modifier ?? Array.Empty<KeyCode>();
    }

    public bool KeyDown()
    {
        return ModifierActive() && 
               (Input.GetKeyDown(Primary) || Input.GetKeyDown(Secondary));
    }

    public bool Key()
    {
        return ModifierActive() && 
               (Input.GetKey(Primary) || Input.GetKey(Secondary));
    }

    public bool KeyUp()
    {
        return ModifierActive() && 
               (Input.GetKeyUp(Primary) || Input.GetKeyUp(Secondary));
    }

    public bool ModifierActive()
    {
        foreach (KeyCode keyCode in Modifier)
        {
            if (!Input.GetKey(keyCode)) return false;
        }
        return true;
    }
}