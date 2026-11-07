using System.Collections.Generic;

/// <summary>Tooltip text for a hovered party member, laid out like an item tooltip: a title
/// line, then one stat per line in a fixed order, plus a secondary block for status effects.</summary>
public static class PlayerTooltip
{
    /// <summary>Name on top, then stats in a fixed order, then what the player is doing.</summary>
    public static string Text(PlayerInfo player)
    {
        var lines = new List<string>
        {
            $"{player.Health}/{player.HealthMax} health",
            $"Hunger {player.Hunger}/{player.HungerMax}",
            $"Stamina {player.Stamina:0}",
            $"Mana {player.Mana:0}",
            $"Sanity {player.Sanity:0}",
        };

        if (player.Equipment is { Stack: > 0 } held)
            lines.Add("Holding " + held.Title);

        lines.Add(Activity(player));
        return Name(player) + "\n" + string.Join("\n", lines);
    }

    /// <summary>Secondary block: the status effects currently on the hovered player.</summary>
    public static string Details(PlayerInfo player)
    {
        string effects = player.Machine?.GetModule<StatusEffectModule>()?.ActiveEffectsText() ?? "";
        return effects.Length > 0 ? "Effects: " + effects : "";
    }

    private static string Name(PlayerInfo player) =>
        Helper.ToDisplayName(player.CharSprite != ID.Null ? player.CharSprite : player.id);

    /// <summary>What the player is busy with, worded like the HUD's own target line
    /// (e.g. "Cutting: Oak Tree | HP 40").</summary>
    private static string Activity(PlayerInfo player)
    {
        if (player.PlayerStatus == PlayerStatus.Incapacitated) return "Incapacitated";
        if (player.Resting) return "Resting";

        Info target = player.Target;
        if (target == null) return "Idle";
        if (target is StructureInfo structure) return structure.ToString();
        if (target is ItemInfo item) return "Picking up: " + Helper.ToDisplayName(item.id);
        if (target is MobInfo mob)
            return player.ActionType == IActionType.Follow
                ? "Following"
                : "Fighting: " + Helper.ToDisplayName(mob.id);
        return target.ToString();
    }
}
