/// <summary>Linear early-game tutorial shown in the HUD. Progresses through a fixed
/// checklist; game systems report events (craft, place, hit, swap) and
/// the player's storage is read each frame for the "collect" objectives.</summary>
public static class Tutorial
{
    private const int ControlBase = 0;  // index of the first control objective

    public enum TutorialControl { Orbit }

    public static void OnControl(TutorialControl control)
    {
        if (Settings.Inst.TutorialEnabled && _progress == ControlBase + (int)control) _progress++;
    }

    private static int _progress;   // index of the current objective
    private static bool _toolbenchCrafted;
    private static bool _toolbenchPlaced;

    public static void Reset()
    {
        _progress = 0;
        _toolbenchCrafted = _toolbenchPlaced = false;
    }

    /// <summary>Advance objectives satisfied by inventory state or combined flags.</summary>
    public static void Update()
    {
        if (!Settings.Inst.TutorialEnabled) return;
        Storage storage = Main.PlayerInfo?.Storage;
        if (storage == null) return;

        switch (_progress)
        {
            case 1: if (storage.Count(ID.Flint) >= 3 && storage.Count(ID.Sticks) >= 2) _progress = 2; break;
            case 6: if (_toolbenchCrafted) _progress = 7; break;
            case 7: if (_toolbenchPlaced) _progress = 8; break;
        }
    }

    public static void OnWorkbenchInteracted()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 8) _progress = 9;
    }

    public static void OnCraft(ID id)
    {
        if (!Settings.Inst.TutorialEnabled) return;
        switch (_progress)
        {
            case 2: if (IsHatchet(id)) _progress = 3; break;
            case 6: if (id == ID.Toolbench) _toolbenchCrafted = true; break;
        }
    }

    public static void OnPlaced(ID structure)
    {
        if (Settings.Inst.TutorialEnabled && _progress == 7 && structure == ID.Toolbench) _toolbenchPlaced = true;
    }

    public static void OnTreeHit(StructureInfo structure, MobInfo attacker)
    {
        if (Settings.Inst.TutorialEnabled && _progress == 3 &&
            structure.id is ID.PineTree or ID.BirchTree &&
            attacker is PlayerInfo)
            _progress = 4;
    }

    public static void OnSwap()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 4) _progress = 5;
    }

    public static void OnTeammateInventory()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 5) _progress = 6;
    }

    public static void OnBlockPlaced()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 9) _progress = 10;
    }

    public static void OnConsume()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 10) _progress = 11;
    }

    private static bool IsHatchet(ID id) =>
        id is ID.CrudeHatchet or ID.StoneHatchet or ID.MetalAxe or ID.DiamondAxe;

    /// <summary>Current objective text for the HUD, or "" once the tutorial is complete or disabled.</summary>
    public static string BuildHudText()
    {
        if (!Settings.Inst.TutorialEnabled || _progress >= LabelCount) return "";
        return "\u2192 " + BuildLabel(_progress) + ProgressSuffix();
    }

    private static int LabelCount => 11;

    private static string Key(ControlKey key) => key.Primary.ToString();

    private static string BuildLabel(int i) => i switch
    {
        0 => "press " + Key(Control.Inst.OrbitLeft) + "/" + Key(Control.Inst.OrbitRight) + " to orbit the camera",
        1 => "press " + Key(Control.Inst.ActionSecondaryNear) + " to pick up flint and sticks",
        2 => "craft a hatchet",
        3 => "hit a tree",
        4 => "press " + Key(Control.Inst.SwapChar) + " to swap character",
        5 => "right click on a teammate to access their inventory",
        6 => "craft a toolbench",
        7 => "place the toolbench",
        8 => "interact with the toolbench with right click",
        9 => "craft and place a mulch block",
        10 => "eat some food",
        _ => "",
    };

    private static string ProgressSuffix()
    {
        Storage storage = Main.PlayerInfo?.Storage;
        return _progress switch
        {
            1 => $" (flint {storage?.Count(ID.Flint) ?? 0}/3, sticks {storage?.Count(ID.Sticks) ?? 0}/2)",
            2 => $" (sticks {storage?.Count(ID.Sticks) ?? 0}/2, flint {storage?.Count(ID.Flint) ?? 0}/2)",
            6 => $" (logs {storage?.Count(ID.Log) ?? 0}/5, flint {storage?.Count(ID.Flint) ?? 0}/3, sticks {storage?.Count(ID.Sticks) ?? 0}/3)",
            _ => "",
        };
    }
}
