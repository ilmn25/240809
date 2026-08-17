using UnityEngine;

public partial class GUIMenu
{
    private string RenderSettings()
    {
        Settings s = Settings.Inst;
        Vector2Int res = Settings.Resolutions[s.ResolutionIndex];
        return
            "1 > Fullscreen: " + (s.Fullscreen ? "On" : "Off") + "\n" +
            "2 > Resolution: " + res.x + "x" + res.y + "\n" +
            "3 > BGM Volume: " + Mathf.RoundToInt(s.BgmVolume * 100) + "\n" +
            "4 > Ambience Volume: " + Mathf.RoundToInt(s.AmbienceVolume * 100) + "\n" +
            "5 > SFX Volume: " + Mathf.RoundToInt(s.SfxVolume * 100) + "\n" +
            "6 > Text Speed: " + (Settings.ScrollSpeeds[s.ScrollSpeedIndex] * 100) + "%" + "\n" +
            "7 > Tutorial: " + (s.TutorialEnabled ? "On" : "Off") + "\n" +
            "8 > Max FPS: " + Settings.FpsLimits[s.FpsIndex] + "\n" +
            "9 > Max FOV: " + Settings.MaxFOVs[s.MaxFOVIndex] + "\n" +
            "0 > Keybinds";
    }

    private void CycleSetting(int index)
    {
        Settings s = Settings.Inst;
        switch (index)
        {
            case 0: // fullscreen
                s.Fullscreen = !s.Fullscreen;
                break;
            case 1: // resolution
                s.ResolutionIndex = (s.ResolutionIndex + 1) % Settings.Resolutions.Length;
                break;
            case 2: // bgm volume
                s.BgmVolume = CycleVolume(s.BgmVolume);
                break;
            case 3: // ambience volume
                s.AmbienceVolume = CycleVolume(s.AmbienceVolume);
                break;
            case 4: // sfx volume
                s.SfxVolume = CycleVolume(s.SfxVolume);
                break;
            case 5: // text speed
                s.ScrollSpeedIndex = (s.ScrollSpeedIndex + 1) % Settings.ScrollSpeeds.Length;
                break;
            case 6: // tutorial
                s.TutorialEnabled = !s.TutorialEnabled;
                break;
            case 7: // max fps
                s.FpsIndex = (s.FpsIndex + 1) % Settings.FpsLimits.Length;
                break;
            case 8: // max fov
                s.MaxFOVIndex = (s.MaxFOVIndex + 1) % Settings.MaxFOVs.Length;
                break;
        }
        Settings.Apply();
        Settings.Save();
        RenderSettingsText();
    }

    private void RenderSettingsText()
    {
        _scrollTask?.Stop();
        Main.GUIMenu.text = Header("Settings") + RenderSettings() + "\n\nESC > Back";
    }

    private static float CycleVolume(float v)
    {
        int percent = Mathf.RoundToInt(v * 100);
        percent = (percent + 10) % 110; // 0,10,...,100 -> 0
        return Mathf.Clamp(percent, 0, 100) / 100f;
    }
}
