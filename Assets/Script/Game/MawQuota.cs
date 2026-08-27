using UnityEngine;

/// <summary>The Maw's daily gold quota. The company demands a quota of gold
/// every day; players feed gold into the pit (<see cref="MawPitMachine"/>) to
/// fill it. Each new day (hooked into <see cref="Environment.MoveTime"/>) the
/// previous day's quota is judged: meet it and the Maw rewards you (+max health)
/// and raises the next quota; fail and it devours a piece of your strength
/// (-max health) and the quota resets.</summary>
public static class MawQuota
{
    public const int BaseQuota = 15;
    public const int QuotaStep = 5;
    public const int MaxQuota = 200;

    /// <summary>Ensures the current quota cycle is initialised for today. Call
    /// before reading/writing quota state (old saves default to 0).</summary>
    private static void Initialize()
    {
        if (Save.Inst == null) return;
        if (Save.Inst.mawQuota <= 0) Save.Inst.mawQuota = BaseQuota;
        if (Save.Inst.mawDay != Save.Inst.day)
        {
            Save.Inst.mawDay = Save.Inst.day;
            Save.Inst.mawPaid = 0;
        }
    }

    private static bool IsFull => Save.Inst != null && Save.Inst.mawPaid >= Save.Inst.mawQuota;

    /// <summary>Called when the pit consumes gold. Returns the amount credited.</summary>
    public static int Deposit(int amount)
    {
        if (Save.Inst == null || amount <= 0) return 0;
        Initialize();

        bool wasFull = IsFull;
        Save.Inst.mawPaid += amount;
        if (!wasFull && IsFull)
            Dialogue.ShowEvent("The Maw's quota is filled. The company will be pleased.");

        return amount;
    }

    /// <summary>Called at the start of each new day to judge the quota that just
    /// ended, then assigns a fresh quota for today.</summary>
    public static void OnDayPassed()
    {
        if (Save.Inst == null) return;
        Initialize();

        Save save = Save.Inst;
        if (save.mawUnlocked)
        {
            if (save.mawPaid >= save.mawQuota)
            {
                save.mawStreak++;
                foreach (PlayerInfo player in save.players)
                    GrantMaxHealth(player, 1);
                Dialogue.ShowEvent("The Maw accepts your tribute. (+1 max health)");
            }
            else
            {
                save.mawStreak = 0;
                foreach (PlayerInfo player in save.players)
                    DevourMaxHealth(player);
                Dialogue.ShowEvent("The Maw hungers... it devours a piece of your strength.");
            }
        }

        save.mawDay = save.day;
        save.mawPaid = 0;
        save.mawQuota = Mathf.Min(MaxQuota, BaseQuota + save.mawStreak * QuotaStep);
    }

    private static void GrantMaxHealth(PlayerInfo player, int amount)
    {
        if (player == null) return;
        player.BaseHealthMax += amount;
        player.HealthMax += amount;
        player.Health += amount;
        GUIBar.Update();
    }

    private static void DevourMaxHealth(PlayerInfo player)
    {
        if (player == null) return;
        player.BaseHealthMax = Mathf.Max(1, player.BaseHealthMax - 1);
        player.HealthMax = Mathf.Max(1, player.HealthMax - 1);
        player.Health = Mathf.Min(player.Health, player.HealthMax);
        GUIBar.Update();
    }
}
