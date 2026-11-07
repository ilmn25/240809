/// <summary>How much surface scatter the generator places: trees, plants, mushrooms,
/// rock piles, loose items and the rare structures that ride the same roll.</summary>
public enum WorldDensity { Normal = 0, Sparse = 1, Lush = 2 }

/// <summary>How long a full in-game day takes in real time.</summary>
public enum DayLength { Normal = 0, Short = 1, Long = 2 }

/// <summary>Choices made on the New Game screen that tweak generation and pacing.
/// Both enums keep <c>Normal</c> as the zero value so saves written before the
/// settings existed keep generating and ticking exactly as they used to.</summary>
public static class WorldSettings
{
    public static float Scale(this WorldDensity density) => density switch
    {
        WorldDensity.Sparse => 0.5f,
        WorldDensity.Lush => 2f,
        _ => 1f,
    };

    public static float Scale(this DayLength length) => length switch
    {
        DayLength.Short => 0.6f,
        DayLength.Long => 1.75f,
        _ => 1f,
    };

    /// <summary>Uniform roll stretched by the world's density. Scaling the roll (instead of
    /// every chance constant) makes all surface scatter denser or sparser together while
    /// keeping their relative weights and the order of the generation chains.</summary>
    public static double DensityRoll(System.Random rng)
    {
        float scale = Save.Inst != null ? Save.Inst.density.Scale() : 1f;
        return rng.NextDouble() / scale;
    }

    public static string Label<T>(this T value) where T : struct, System.Enum
        => value.ToString().ToLowerInvariant();
}
