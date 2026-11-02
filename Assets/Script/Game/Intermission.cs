using System.Collections;
using UnityEngine;

/// <summary>Start-of-game intermission: dims the world, shows the intro dialogue
/// while generating worlds over frames, then loads the map in and reveals the
/// game. Blocks game input and hides the HUD while active.</summary>
public static class Intermission
{
    public static bool Active;

    private static readonly string[] NewGameIntro =
    {
        "You wake in the Abyss — a place with no name and no memory of you, though it knows your footsteps.",
        "Beneath the dark, something breathes in time with your heartbeat... and it has been waiting a very long time."
    };

    public static void Start(bool playIntro = false)
    {
        new CoroutineTask(Play(playIntro));
    }

    private static IEnumerator Play(bool playIntro)
    {
        Active = playIntro;

        if (playIntro)
        {
            Environment.Target = EnvironmentType.Dim;
            ScreenFade.FadeIn(1f, 0.3f);
            Dialogue.ShowEventChain(Save.Inst.id == null ? NewGameIntro : new[] { "The horror continues..." });
        }

        GenerateWorlds();

        if (playIntro)
            while (Dialogue.Showing) yield return null;

        yield return new WaitForSeconds(1f);
        
        Main.GUIMenu.gameObject.SetActive(false);
        World.Inst.PopulateNavMap();
        Vector3 spawnPosition = World.Inst.SpawnPoint;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player.Machine == null) Entity.SpawnFromInfo(player, false);
            player.Machine.transform.position = spawnPosition;
            // Safe at the destination spawn — drop the world-switch protection so
            // the player lands and resumes control instead of hovering mid-air.
            player.EndWorldSwitchProtection();
        }
        PlayerSync.HostClaimPlayer(Save.Inst.players[0].uid);

        Environment.Target = EnvironmentType.Null;
        ScreenFade.FadeOut(0.3f);
        yield return new WaitForSeconds(2.4f); 
        ScreenFade.FadeIn(1f, 0.5f);
        Active = false;
    }

    private static void GenerateWorlds()
    {
        foreach (var kv in Save.Inst.worlds)
        {
            Gen.GenerateAllFor(kv.Value);
            kv.Value.Map?.BuildMarkers(kv.Value);
        }
    }
}
