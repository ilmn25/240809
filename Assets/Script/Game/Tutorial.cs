/// <summary>Linear early-game tutorial shown in the HUD. Progresses through a fixed
/// checklist; game systems report events (craft, place, assemble, hit, swap) and
/// the player's storage is read each frame for the "collect" objectives.</summary>
public static class Tutorial
{
    private static readonly string[] Labels =
    {
        "press I to open inventory",
        "press M to open the map",
        "press Q/E to orbit the camera",
        "right click on the owl statue",
        "press F to pick up flint and sticks",
        "craft a hatchet",
        "hit a tree",
        "press Tab to swap character",
        "collect 15 logs",
        "press H to recall your allies",
        "craft and place a workbench",
        "craft a crude mallet and assemble the workbench",
        "place a block",
        "eat some food",
    };

    private const int ControlBase = 0;  // index of the first control objective

    public enum TutorialControl { Inventory, Map, Orbit }

    public static void OnControl(TutorialControl control)
    {
        if (_progress == ControlBase + (int)control) _progress++;
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
        Storage storage = Main.PlayerInfo?.Storage;
        if (storage == null) return;

        switch (_progress)
        {
            case 4: if (storage.Count(ID.Flint) >= 3 && storage.Count(ID.Sticks) >= 2) _progress = 5; break;
            case 8: if (storage.Count(ID.Log) >= 15) _progress = 9; break;
            case 10: if (_workbenchCrafted && _workbenchPlaced) _progress = 11; break;
            case 11: if (_malletCrafted && _workbenchAssembled) _progress = 12; break;
        }
    }

    public static void OnOwlStatueInteracted()
    {
        if (_progress == 3) _progress = 4;
    }

    public static void OnCraft(ID id)
    {
        switch (_progress)
        {
            case 5: if (IsHatchet(id)) _progress = 6; break;
            case 10: if (id == ID.Workbench) _workbenchCrafted = true; break;
            case 11: if (id == ID.CrudeMallet) _malletCrafted = true; break;
        }
    }

    public static void OnPlaced(ID structure)
    {
        if (_progress == 10 && structure == ID.Workbench) _workbenchPlaced = true;
    }

    public static void OnAssembled(ID structure)
    {
        if (_progress == 11 && structure == ID.Workbench) _workbenchAssembled = true;
    }

    public static void OnTreeHit(StructureInfo structure, MobInfo attacker)
    {
        if (_progress == 6 &&
            structure.id is ID.PineTree or ID.BirchTree &&
            attacker is PlayerInfo)
            _progress = 7;
    }

    public static void OnSwap()
    {
        if (_progress == 7) _progress = 8;
    }

    public static void OnRecall()
    {
        if (_progress == 9) _progress = 10;
    }

    public static void OnBlockPlaced()
    {
        if (_progress == 12) _progress = 13;
    }

    public static void OnEat()
    {
        if (_progress == 13) _progress = 14;
    }

    private static bool IsHatchet(ID id) =>
        id is ID.CrudeHatchet or ID.StoneHatchet or ID.MetalAxe or ID.DiamondAxe;

    /// <summary>Current objective text for the HUD, or "" once the tutorial is complete.</summary>
    public static string BuildHudText()
    {
        if (_progress >= Labels.Length) return "";
        return "\u2192 " + Labels[_progress] + ProgressSuffix();
    }

    private static string ProgressSuffix()
    {
        Storage storage = Main.PlayerInfo?.Storage;
        return _progress switch
        {
            4 => $" (flint {storage?.Count(ID.Flint) ?? 0}/3, sticks {storage?.Count(ID.Sticks) ?? 0}/2)",
            8 => $" (logs {storage?.Count(ID.Log) ?? 0}/15)",
            _ => "",
        };
    }
}
