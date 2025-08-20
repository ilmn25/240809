using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ControlKey
{
    private KeyCode _primary;
    private KeyCode _secondary;
    private readonly KeyCode[] _modifier;

    public ControlKey(KeyCode primary = KeyCode.None, KeyCode secondary = KeyCode.None, KeyCode[] modifier = null)
    {
        this._primary = primary;
        _secondary = secondary;
        _modifier = modifier ?? Array.Empty<KeyCode>();
    }

    public bool KeyDown()
    {
        return ModifierActive() && !Console.IsTyping &&
               (Input.GetKeyDown(_primary) || Input.GetKeyDown(_secondary));
    }

    public bool Key()
    {
        return ModifierActive() && !Console.IsTyping &&
               (Input.GetKey(_primary) || Input.GetKey(_secondary));
    }

    public bool KeyUp()
    {
        return ModifierActive() && !Console.IsTyping && 
               (Input.GetKeyUp(_primary) || Input.GetKeyUp(_secondary));
    }

    public bool ModifierActive()
    {
        foreach (KeyCode keyCode in _modifier)
        {
            if (!Input.GetKey(keyCode)) return false;
        }
        return true;
    }
}