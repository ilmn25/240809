using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A bed that also acts as the Merchant's home, like a pig house. Each bed hosts
/// exactly one merchant while it sits within a couple of blocks of a lamp AND an old radio.
/// NPCs aren't saved, so the bed respawns its merchant whenever it dies or despawns.</summary>
public class BedMachine : StructureMachine, IActionSecondaryInteract
{
    private const int CheckInterval = 200;       // frames between checks (~3.3s at 60 fps)
    private const int RespawnDelay = 900;        // frames before a lost merchant respawns (~15s)
    private const float RequirementRadius = 2.5f; // lamp & radio within ~2 blocks
    private const float MerchantSearchRadius = 40f;

    private static readonly Collider[] ScanBuffer = new Collider[16];

    private Info _merchantInfo;
    private int _timer;
    private int _respawnTimer;

    public static Info CreateInfo()
    {
        return new Info();
    }

    public override void OnStart()
    {
        base.OnStart();
        _timer = Random.Range(0, CheckInterval); // stagger beds so they don't all fire at once
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (++_timer < CheckInterval) return;
        _timer = 0;

        if (MerchantAlive()) return;

        // Only hosts a merchant while a lamp and an old radio are nearby.
        if (!HasLampAndRadio()) return;

        if (_respawnTimer > 0)
        {
            _respawnTimer--;
            return;
        }

        // Spawn a new merchant next to the bed.
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(2, 0, 2);
        _merchantInfo = Entity.Spawn(ID.Merchant, spawnPos);
        Console.Print("someone is approaching your outpost...");
        _respawnTimer = RespawnDelay;
    }

    private bool HasLampAndRadio()
    {
        bool hasLamp = false;
        bool hasRadio = false;
        int count = Physics.OverlapSphereNonAlloc(transform.position, RequirementRadius, ScanBuffer, Main.MaskEntity);
        for (int i = 0; i < count; i++)
        {
            if (ScanBuffer[i].TryGetComponent(out LampMachine _)) hasLamp = true;
            else if (ScanBuffer[i].TryGetComponent(out OldRadioMachine _)) hasRadio = true;
            if (hasLamp && hasRadio) return true;
        }
        return false;
    }

    // True while this bed's merchant is still alive. Falls back to adopting any merchant
    // already standing nearby (e.g. after a world reload), so each bed keeps exactly one.
    private bool MerchantAlive()
    {
        if (_merchantInfo != null && !_merchantInfo.Destroyed && _merchantInfo.Machine != null)
            return true;

        int count = Physics.OverlapSphereNonAlloc(transform.position, MerchantSearchRadius, ScanBuffer, Main.MaskEntity);
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

    public void OnActionSecondary(Info info)
    {
        // Sleep disabled for now.
        // if (Save.Inst.weather == EnvironmentType.Day) return;
        // _ = new CoroutineTask(Sleep());
        // return;
        //
        // IEnumerator Sleep()
        // {
        //     ScreenFade.FadeOut(0.5f);
        //     Main.PlayerInfo.SpeedModifier = 0.001f;
        //     yield return new WaitForSeconds(2.5f);
        //     Environment.MoveTime(Environment.Length / 2);
        //     ScreenFade.FadeIn(0.5f);
        //     yield return new WaitForSeconds(0.6f);
        //     Main.PlayerInfo.SpeedModifier = 1;
        // }
    }
}