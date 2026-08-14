using System;

/// <summary>Info for the bug. When it latches onto a player it applies a Slow
/// status effect. The bug only releases when the player is hit by something else
/// (a non-bug projectile), at which point it goes into a strafe panic.</summary>
[System.Serializable]
public class BugInfo : EnemyInfo
{
    /// <summary>The player this bug is currently latched onto, or null.</summary>
    [NonSerialized] public PlayerInfo LatchedPlayer;

    /// <summary>Slow effect applied to the latched player.</summary>
    private static readonly StatusEffect Slow = new StatusEffect(
        ID.SnareFlea, EffectType.Slow, duration: 999f, tickInterval: 1f, slowAmount: 0.5f, name: "Bugged");

    /// <summary>Latch onto the given player: attach, apply slow, and remember them.</summary>
    public void Latch(PlayerInfo player)
    {
        if (LatchedPlayer == player) return;
        LatchedPlayer = player;
        player.Machine?.GetModule<StatusEffectModule>()?.Apply(Slow);
    }

    /// <summary>Release the current latch: remove the slow and clear the player.</summary>
    public void Release()
    {
        if (LatchedPlayer != null)
            LatchedPlayer.Machine?.GetModule<StatusEffectModule>()?.Remove(ID.SnareFlea);
        LatchedPlayer = null;
    }

    /// <summary>Called when the player this bug is latched onto gets hit by
    /// something else — release the bug so it panics away.</summary>
    public void OnLatchedPlayerHit()
    {
        Release();
        if (Machine is BugMachine bug)
            bug.Panic();
    }
}
