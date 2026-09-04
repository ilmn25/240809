using UnityEngine;

/// <summary>A placeable owl statue that acts as the Guide's home, like a pig house.
/// NPCs aren't saved, so the statue brings the Guide back each new day (or the next
/// time the statue loads) whenever it dies. Right-clicking it shows the controls.</summary>
public class OwlStatueMachine : SpawnerStructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new StructureInfo
        {
            Health = 100,
            Loot = ID.OwlStatue,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
        };
    }

    public override void OnStart()
    {
        base.OnStart(); // restores the Guide
        AddModule(new NightGlowModule());
    }

    /// <summary>Spawns a new guide beside the statue so it drops down and stands next to it.</summary>
    protected override Info SpawnUnit(int index)
    {
        Vector3Int spawnPos = Vector3Int.FloorToInt(transform.position) + new Vector3Int(1, 2, 0);
        return Entity.Spawn(ID.Guide, spawnPos);
    }

    public void OnActionSecondary(Info info)
    {
        Dialogue.Target = BuildControlsDialogue();
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Text);
    }

    private static Dialogue BuildControlsDialogue()
    {
        return new Dialogue { Text = "this is the center of the map" };
    }

    // Glows at night and turns off during the day. Runs in Everyone mode so it
    // updates on every client too (Save.Inst.weather is synced via EnvironmentSync).
    private class NightGlowModule : Module
    {
        public NightGlowModule() { updateMode = UpdateMode.Everyone; }

        public override void Update()
        {
            bool night = Save.Inst.weather == EnvironmentType.NightRainy ||
                         Save.Inst.weather == EnvironmentType.NightBright;
            ((StructureMachine)Machine).SetGlow(night);
        }
    }
}
