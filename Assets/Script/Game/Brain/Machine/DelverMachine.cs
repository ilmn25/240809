using UnityEngine;

/// <summary>A delver — a player-like entity that is strictly AI-controlled and
/// can never be taken over by a human. It shares every player mechanic (inventory,
/// hunger, stats, spawn protection, party-follower AI) and is persisted in the
/// save's player list (<see cref="PlayerInfo.IsDelver"/>), so it survives save/load
/// exactly like a party member — but it is excluded from control switching, client
/// claims, and the death handoff.</summary>
public class DelverMachine : PlayerMachine
{
    private new PlayerInfo Info => GetModule<PlayerInfo>();

    /// <summary>Optional contract expiry (in <see cref="Time.time"/> seconds).
    /// 0 = permanent (console/bound-NPC delvers). Hired mercenaries set this so
    /// they leave once their contract runs out.</summary>
    public float ExpireAt;

    public static new Info CreateInfo()
    {
        // Start from the exact player stat block, then mark it AI-only.
        PlayerInfo player = (PlayerInfo)PlayerMachine.CreateInfo();
        player.IsDelver = true;
        return player;
    }

    /// <summary>Delvers are always AI — never read human input. Runs the same
    /// party-follower brain as non-controlled allies (fight nearby hostiles,
    /// work marked structures, otherwise trail the leader).</summary>
    public override void OnUpdate()
    {
        Info.position = transform.position;

        // Timed companions (hired mercenaries) leave when their time runs out.
        if (ExpireAt > 0f && Time.time >= ExpireAt)
        {
            Info.Destroy();
            Unload();
            return;
        }

        UpdateAllyBrain();
    }
}
