using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>The merchant-housing system for a bed (a "pig house"). Each bed hosts exactly
/// one merchant while its requirements are met, and respawns it whenever it dies or
/// despawns (NPCs aren't saved). The bed drives this via OnStart/OnUpdate.</summary>
public class MerchantHouse
{
    private const int CheckInterval = 200;       // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 900;        // frames before a lost merchant respawns (~15s)
    private const float RequirementRadius = 2.5f; // lamp within ~2 blocks
    private const float MerchantSearchRadius = 40f;
    private const float BedSpacingRadius = 12f;  // too close to another occupied bed
    private const int ShelterScanHeight = 4;     // solid block overhead counts as shelter

    private static readonly Collider[] ScanBuffer = new Collider[16];

    private readonly BedMachine _bed;
    private Info _merchantInfo;
    private int _timer;
    private int _respawnTimer;

    public MerchantHouse(BedMachine bed)
    {
        _bed = bed;
    }

    public void OnStart()
    {
        _timer = Random.Range(0, CheckInterval); // stagger beds so they don't all fire at once
    }

    public void OnUpdate()
    {
        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (MerchantAlive()) return;

        // Only hosts a merchant while its requirements are met.
        if (!CanHostMerchant()) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a new merchant next to the bed.
        Vector3Int spawnPos = Vector3Int.FloorToInt(_bed.transform.position) + new Vector3Int(2, 0, 2);
        _merchantInfo = Entity.Spawn(ID.Merchant, spawnPos);
        Console.Print("someone is approaching your outpost...");
        _respawnTimer = RespawnDelay;
    }

    /// <summary>All requirements needed before the bed will host a merchant.</summary>
    public bool CanHostMerchant()
    {
        return HasLamp && IsSheltered && !IsTooCloseToOccupiedBed;
    }

    /// <summary>Whether a lamp sits within ~2 blocks of this bed.</summary>
    public bool HasLamp
    {
        get
        {
            int count = Physics.OverlapSphereNonAlloc(_bed.transform.position, RequirementRadius, ScanBuffer, Main.MaskEntity);
            for (int i = 0; i < count; i++)
                if (ScanBuffer[i].TryGetComponent(out LampMachine _)) return true;
            return false;
        }
    }

    /// <summary>Whether a solid block sits overhead of the bed and its 8 surrounding
    /// cells (sheltered from the environment).</summary>
    public bool IsSheltered
    {
        get
        {
            Vector3Int bed = Vector3Int.FloorToInt(_bed.transform.position);
            // The bed cell plus its 8 neighbours all need a ceiling overhead.
            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                    if (!HasCeiling(bed + new Vector3Int(dx, 0, dz)))
                        return false;
            return true;
        }
    }

    // True if any solid block sits within ShelterScanHeight above the given cell.
    private bool HasCeiling(Vector3Int cell)
    {
        for (int y = 1; y <= ShelterScanHeight; y++)
            if (World.GetBlock(cell + new Vector3Int(0, y, 0)) != 0)
                return true;
        return false;
    }

    /// <summary>Whether another bed that already hosts a merchant is too close.</summary>
    public bool IsTooCloseToOccupiedBed
    {
        get
        {
            int count = Physics.OverlapSphereNonAlloc(_bed.transform.position, BedSpacingRadius, ScanBuffer, Main.MaskEntity);
            for (int i = 0; i < count; i++)
            {
                if (ScanBuffer[i].TryGetComponent(out BedMachine other) && other != _bed && other.HasMerchant)
                    return true;
            }
            return false;
        }
    }

    /// <summary>Whether this bed currently has a live merchant.</summary>
    public bool HasMerchant => MerchantAlive();

    // True while this bed's merchant is still alive. Falls back to adopting any merchant
    // already standing nearby (e.g. after a world reload), so each bed keeps exactly one.
    private bool MerchantAlive()
    {
        if (_merchantInfo != null && !_merchantInfo.Destroyed && _merchantInfo.Machine != null)
            return true;

        int count = Physics.OverlapSphereNonAlloc(_bed.transform.position, MerchantSearchRadius, ScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (ScanBuffer[i].TryGetComponent(out MerchantMachine merchant) && !merchant.Info.Destroyed)
            {
                _merchantInfo = merchant.Info;
                return true;
            }
        }
        return false;
    }

    /// <summary>Reports every requirement in full sentences so the player can remember
    /// what a bed needs when building the next one.</summary>
    public string Diagnose()
    {
        return
            $"There {(HasLamp ? "is" : "is no")} suitable light source nearby.\n" +
            $"The bed {(IsSheltered ? "is" : "is not")} sheltered from the environment.\n" +
            $"The bed {(IsTooCloseToOccupiedBed ? "is" : "is not")} too close to another occupied bed.";
    }
}
