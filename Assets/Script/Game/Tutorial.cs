/// <summary>Linear early-game tutorial shown in the HUD. Progresses through a fixed
/// checklist; game systems report events (craft, place, assemble, cut) and the
/// player's storage is read each frame for the "collect" objectives.</summary>
public static class Tutorial
{
    private static readonly string[] Labels =
    {
        "collect flint",
        "collect sticks",
        "craft a hatchet",
        "cut down a tree",
        "craft and place a workbench",
        "craft a crude mallet and assemble the workbench",
    };

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
            case 0: if (storage.Count(ID.Flint) >= 3) _progress = 1; break;
            case 1: if (storage.Count(ID.Sticks) >= 2) _progress = 2; break;
            case 4: if (_workbenchCrafted && _workbenchPlaced) _progress = 5; break;
            case 5: if (_malletCrafted && _workbenchAssembled) _progress = 6; break;
        }
    }

    public static void OnCraft(ID id)
    {
        switch (_progress)
        {
            case 2: if (IsHatchet(id)) _progress = 3; break;
            case 4: if (id == ID.Workbench) _workbenchCrafted = true; break;
            case 5: if (id == ID.CrudeMallet) _malletCrafted = true; break;
        }
    }

    public static void OnPlaced(ID structure)
    {
        if (_progress == 4 && structure == ID.Workbench) _workbenchPlaced = true;
    }

    public static void OnAssembled(ID structure)
    {
        if (_progress == 5 && structure == ID.Workbench) _workbenchAssembled = true;
    }

    public static void OnStructureDestroyed(StructureInfo structure, MobInfo attacker)
    {
        if (_progress == 3 &&
            structure.id is ID.PineTree or ID.BirchTree &&
            attacker is PlayerInfo)
            _progress = 4;
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
            0 => $" ({storage?.Count(ID.Flint) ?? 0}/3)",
            1 => $" ({storage?.Count(ID.Sticks) ?? 0}/2)",
            _ => "",
        };
    }
}
