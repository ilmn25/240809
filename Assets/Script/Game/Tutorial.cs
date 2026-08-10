/// <summary>Linear early-game tutorial shown in the HUD. Progresses through a fixed
/// checklist; game systems report events (craft, place, assemble, hit, swap) and
/// the player's storage is read each frame for the "collect" objectives.</summary>
public static class Tutorial
{
    private const int ControlBase = 0;  // index of the first control objective

    public enum TutorialControl { Orbit, Map }

    public static void OnControl(TutorialControl control)
    {
        if (Settings.Inst.TutorialEnabled && _progress == ControlBase + (int)control) _progress++;
    }

    private static int _progress;   // index of the current objective
    private static bool _workbenchCrafted;
    private static bool _workbenchPlaced;
    private static bool _malletCrafted;
    private static bool _workbenchAssembled;

    public static void Reset()
    {
        _progress = 0;
        _workbenchCrafted = _workbenchPlaced = _malletCrafted = _workbenchAssembled = false;
    }

    /// <summary>Advance objectives satisfied by inventory state or combined flags.</summary>
    public static void Update()
    {
        if (!Settings.Inst.TutorialEnabled) return;
        Storage storage = Main.PlayerInfo?.Storage;
        if (storage == null) return;

        switch (_progress)
        {
            case 3: if (storage.Count(ID.Flint) >= 3 && storage.Count(ID.Sticks) >= 2) _progress = 4; break;
            case 7: if (storage.Count(ID.Log) >= 15) _progress = 8; break;
            case 9: if (_workbenchCrafted && _workbenchPlaced) _progress = 10; break;
            case 10: if (_malletCrafted && _workbenchAssembled) _progress = 11; break;
        }
    }

    public static void OnOwlStatueInteracted()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 2) _progress = 3;
    }

    public static void OnCraft(ID id)
    {
        if (!Settings.Inst.TutorialEnabled) return;
        switch (_progress)
        {
            case 4: if (IsHatchet(id)) _progress = 5; break;
            case 9: if (id == ID.Workbench) _workbenchCrafted = true; break;
            case 10: if (id == ID.CrudeMallet) _malletCrafted = true; break;
        }
    }

    public static void OnPlaced(ID structure)
    {
        if (Settings.Inst.TutorialEnabled && _progress == 9 && structure == ID.Workbench) _workbenchPlaced = true;
    }

    public static void OnAssembled(ID structure)
    {
        if (Settings.Inst.TutorialEnabled && _progress == 10 && structure == ID.Workbench) _workbenchAssembled = true;
    }

    public static void OnTreeHit(StructureInfo structure, MobInfo attacker)
    {
        if (Settings.Inst.TutorialEnabled && _progress == 5 &&
            structure.id is ID.PineTree or ID.BirchTree &&
            attacker is PlayerInfo)
            _progress = 6;
    }

    public static void OnSwap()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 6) _progress = 7;
    }

    public static void OnRecall()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 8) _progress = 9;
    }

    public static void OnBlockPlaced()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 11) _progress = 12;
    }

    public static void OnEat()
    {
        if (Settings.Inst.TutorialEnabled && _progress == 12) _progress = 13;
    }

    private static bool IsHatchet(ID id) =>
        id is ID.CrudeHatchet or ID.StoneHatchet or ID.MetalAxe or ID.DiamondAxe;

    /// <summary>Current objective text for the HUD, or "" once the tutorial is complete or disabled.</summary>
    public static string BuildHudText()
    {
        if (!Settings.Inst.TutorialEnabled || _progress >= LabelCount) return "";
        return "\u2192 " + BuildLabel(_progress) + ProgressSuffix();
    }

    private static int LabelCount => 13;

    private static string Key(ControlKey key) => key.Primary.ToString();

    private static string BuildLabel(int i) => i switch
    {
        0 => "press " + Key(Control.Inst.OrbitLeft) + "/" + Key(Control.Inst.OrbitRight) + " to orbit the camera",
        1 => "press " + Key(Control.Inst.Map) + " to open the map",
        2 => "right click on the owl statue",
        3 => "press " + Key(Control.Inst.ActionSecondaryNear) + " to pick up flint and sticks",
        4 => "craft a hatchet",
        5 => "hit a tree",
        6 => "press " + Key(Control.Inst.SwapChar) + " to swap character",
        7 => "collect 15 logs",
        8 => "press " + Key(Control.Inst.Recall) + " to recall your teammates",
        9 => "craft and place a workbench",
        10 => "craft a crude mallet and assemble the workbench",
        11 => "place a block",
        12 => "eat some food",
        _ => "",
    };

    private static string ProgressSuffix()
    {
        Storage storage = Main.PlayerInfo?.Storage;
        return _progress switch
        {
            3 => $" (flint {storage?.Count(ID.Flint) ?? 0}/3, sticks {storage?.Count(ID.Sticks) ?? 0}/2)",
            7 => $" (logs {storage?.Count(ID.Log) ?? 0}/15)",
            _ => "",
        };
    }
}
