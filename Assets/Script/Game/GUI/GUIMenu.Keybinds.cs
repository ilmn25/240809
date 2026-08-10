using System;
using System.Text;
using UnityEngine;

public partial class GUIMenu
{
    private static readonly (string Label, Func<ControlKey> GetKey)[] KeybindList = BuildKeybindList();
    private int _keybindPage;
    private int _keybindIndex = -1;
    private bool _rebinding;

    private static (string Label, Func<ControlKey> GetKey)[] BuildKeybindList() => new (string, Func<ControlKey>)[]
    {
        ("Inventory", () => Control.Inst.Inv),
        ("Map", () => Control.Inst.Map),
        ("Pause", () => Control.Inst.Pause),
        ("Swap Character", () => Control.Inst.SwapChar),
        ("Recall Allies", () => Control.Inst.Recall),
        ("Fullscreen", () => Control.Inst.FullScreen),
        ("Reveal Map", () => Control.Inst.RevealMap),
        ("Use Near", () => Control.Inst.ActionPrimaryNear),
        ("Pick Up", () => Control.Inst.ActionSecondaryNear),
        ("Orbit Left", () => Control.Inst.OrbitLeft),
        ("Orbit Right", () => Control.Inst.OrbitRight),
        ("Jump", () => Control.Inst.Jump),
        ("Sprint", () => Control.Inst.Sprint),
        ("Drop", () => Control.Inst.Drop),
        ("Up", () => Control.Inst.Up),
        ("Down", () => Control.Inst.Down),
        ("Left", () => Control.Inst.Left),
        ("Right", () => Control.Inst.Right),
    };

    private string RenderKeybinds()
    {
        var sb = new StringBuilder();
        int start = _keybindPage * 8;
        for (int i = 0; i < 8; i++)
        {
            int index = start + i;
            if (index >= KeybindList.Length) break;
            var (label, getKey) = KeybindList[index];
            sb.Append($"{i + 1} > {label}: {getKey().Primary}\n");
        }
        if (start > 0)
            sb.Append("\n9 > prev");
        if (start + 8 < KeybindList.Length)
            sb.Append("\n0 > next");
        return sb.ToString();
    }

    private string RenderKeybind()
    {
        var (label, getKey) = KeybindList[_keybindIndex];
        return label + ": " + getKey().Primary + "\npress a new key...";
    }

    private void HandleKeybindSelect(int n)
    {
        if (n == 9 && _keybindPage > 0)
        {
            Audio.PlaySFX(SfxID.Text);
            _keybindPage--;
            RenderNoScroll();
            return;
        }
        if (n == 0 && _keybindPage * 8 + 8 < KeybindList.Length)
        {
            Audio.PlaySFX(SfxID.Text);
            _keybindPage++;
            RenderNoScroll();
            return;
        }
        int index = _keybindPage * 8 + (n - 1);
        if (n >= 1 && n <= 8 && index < KeybindList.Length)
        {
            Audio.PlaySFX(SfxID.Text);
            _keybindIndex = index;
            _rebinding = true;
            _screen = MenuScreen.Keybind;
            RenderNoScroll();
        }
    }

    private void ApplyKeybind(KeyCode key)
    {
        if (_keybindIndex >= 0 && _keybindIndex < KeybindList.Length)
        {
            KeybindList[_keybindIndex].GetKey().Primary = key;
            Control.Save();
        }
    }

    private static bool TryGetPressedKey(out KeyCode key)
    {
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
        {
            if (k != KeyCode.None && Input.GetKeyDown(k))
            {
                key = k;
                return true;
            }
        }
        key = KeyCode.None;
        return false;
    }
}
