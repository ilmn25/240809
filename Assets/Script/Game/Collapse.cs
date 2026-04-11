using UnityEngine;

public static class Collapse
{
    private const int DisplayOffsetMinutes = 300; // Save time starts at 5:00 AM display time
    private const int ShakeStartDisplayMinute = 21 * 60; // 9:00 PM
    private const int ShakePeakDisplayMinute = 25 * 60 + 5;  // 1:00 AM (next day)
    private const int ShakePeakInternalHour = 20; // 1:00 AM display time with +5h offset
    private const float AmbientShakeSpeed = 80f;
    private const float AmbientMaxStrength = 0.3f;

    static Collapse()
    {
        Environment.HourlyTriggered += OnHour;
    }

    public static Vector3 Offset { get; private set; }

    public static void Update()
    {
        if (Save.Inst == null)
        {
            Offset = Vector3.zero;
            return;
        }

        float strength = GetStrength(GetAdjustedDisplayMinutes());
        if (strength <= 0f)
        {
            Offset = Vector3.zero;
            return;
        }

        Offset = ScreenShake.SampleNoiseOffset(AmbientShakeSpeed, strength, 0.59f, 0.97f);
    }

    private static void OnHour(int hour, int day)
    {
        if (hour != ShakePeakInternalHour || Save.Inst == null || Scene.Busy)
            return;

        if (Save.Inst.current != GenType.SkyBlock)
            Scene.SwitchWorld(GenType.SkyBlock);
    }

    private static int GetAdjustedDisplayMinutes()
    {
        int displayMinutes = (Save.Inst.time + DisplayOffsetMinutes) % 1440;
        return displayMinutes < ShakeStartDisplayMinute
            ? displayMinutes + 1440
            : displayMinutes;
    }

    private static float GetStrength(int adjustedMinutes)
    {
        if (adjustedMinutes < ShakeStartDisplayMinute) return 0f;
        if (adjustedMinutes < ShakePeakDisplayMinute)
        {
            float t = Mathf.InverseLerp(ShakeStartDisplayMinute, ShakePeakDisplayMinute, adjustedMinutes);
            return AmbientMaxStrength * SmoothEased(t);
        }

        return adjustedMinutes == ShakePeakDisplayMinute ? AmbientMaxStrength : 0f;
    }

    private static float SmoothEased(float t)
    {
        t = Mathf.Clamp01(t);
        float s = t * t * t * (t * (t * 6f - 15f) + 10f);
        return s * s;
    }
}
