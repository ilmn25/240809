using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EnvironmentType
{
    Null, Black, Sunrise, Rapture, Day, DaySnow, Sunset, NightRainy, NightBright, Dim
}
public class Environment
{
    public static Dictionary<EnvironmentType, Environment> Environments = new ();
    public static event Action<int, int> HourlyTriggered;
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public Color BackgroundColor;

    static Environment()
    {
        Environments.Add(EnvironmentType.Black, new Environment
        {
            AmbientLight = Color.black,
            FogColor = Color.black,
            SpotLight = Color.black,
            DirectionalLight = Color.black,
            BackgroundColor = Color.black,
        });
        Environments.Add(EnvironmentType.Dim, new Environment
        {
            AmbientLight = Helper.GetColor(16, 15, 28),
            FogColor = Helper.GetColor(12, 11, 20),
            SpotLight = Helper.GetColor(26, 25, 34),
            DirectionalLight = Helper.GetColor(22, 20, 46),
            BackgroundColor = Helper.GetColor(10, 10, 16)
        });
        Environments.Add(EnvironmentType.Rapture, new Environment
        {
            AmbientLight = Helper.GetColor(39, 38, 64),
            FogColor = Helper.GetColor(97, 39, 39),
            SpotLight = Helper.GetColor(161, 77, 77),
            DirectionalLight = Helper.GetColor(255, 68, 47),
            BackgroundColor = Helper.GetColor(190, 130, 134)
        });
        Environments.Add(EnvironmentType.Day, new Environment
        {
            AmbientLight = Helper.GetColor(225, 225, 225),
            FogColor = Helper.GetColor(117, 110, 138),
            SpotLight = Helper.GetColor(197, 142, 88),
            DirectionalLight = Helper.GetColor(135, 124, 121),
            BackgroundColor = Helper.GetColor(116, 113, 137)
        });
        Environments.Add(EnvironmentType.DaySnow, new Environment
        {
            AmbientLight = Helper.GetColor(38, 37, 63),
            FogColor = Helper.GetColor(146, 146, 146),
            SpotLight = Helper.GetColor(97, 97, 97),
            DirectionalLight = Helper.GetColor(105, 101, 159),
            BackgroundColor = Helper.GetColor(131, 131, 135)
        });
        Environments.Add(EnvironmentType.Sunset, new Environment
        {
            AmbientLight = Helper.GetColor(255, 184, 184),
            FogColor = Helper.GetColor(118, 105, 105),
            SpotLight = Helper.GetColor(103, 70, 66),
            DirectionalLight = Helper.GetColor(255, 184, 56),
            BackgroundColor = Helper.GetColor(188, 111, 77)
        });
        Environments.Add(EnvironmentType.NightRainy, new Environment
        {
            AmbientLight = Helper.GetColor(38, 37, 63),
            FogColor = Color.black,
            SpotLight = Helper.GetColor(97, 97, 97),
            DirectionalLight = Helper.GetColor(91, 56, 255),
            BackgroundColor = Helper.GetColor(45, 50, 63)
        });
        Environments.Add(EnvironmentType.NightBright, new Environment
        {
            AmbientLight = Helper.GetColor(38, 37, 63),
            FogColor = Color.black,
            SpotLight = Helper.GetColor(164, 138, 129),
            DirectionalLight = Helper.GetColor(91, 56, 255),
            BackgroundColor = Helper.GetColor(45, 50, 63) 
        });
        Environments.Add(EnvironmentType.Sunrise, new Environment
        {
            AmbientLight = Helper.GetColor(38, 37, 63),
            FogColor = Color.black,
            SpotLight = Helper.GetColor(154, 90, 69),
            DirectionalLight = Helper.GetColor(254, 57, 90),
            BackgroundColor = Helper.GetColor(75, 59, 55)
        });
        _ = new CoroutineTask(Clock());
    }       
     
    public const int Length = 60 * 24;
    private const float Speed = 0.48f; // seconds per in-game minute — 20% longer days
    private const int TransitionLength = 200;
    private static int _currentTransitionTime;  
    private static EnvironmentType _previous = EnvironmentType.Black;
    private static EnvironmentType _current = EnvironmentType.Black; 
    public static EnvironmentType Target = EnvironmentType.Null;
    private static int Time
    {
        get => Save.Inst.time;
        set => Save.Inst.time = value;
    }
    private static EnvironmentType Weather
    {
        get => Save.Inst.weather;
        set => Save.Inst.weather = value;
    }

    /// <summary>
    /// Sets the initial environment state so _current/_previous track the
    /// actual visual state. Call once at startup (e.g. Main.Start) instead
    /// of Set() to keep the transition system in sync.
    /// </summary>
    public static void SetStartEnvironment(EnvironmentType type)
    {
        _previous = type;
        _current = type;
        Environment env = Environments[type];
        Set(env.AmbientLight, env.FogColor, env.SpotLight, env.DirectionalLight, env.BackgroundColor);
    }

    private static void SetTarget(EnvironmentType target)
    {
        if (target == _current) return;
        _previous = _current;
        _current = target;
        _currentTransitionTime = 0;
    }

    public static IEnumerator Clock()
    {
        while (true)
        {
            yield return new WaitForSeconds(Speed);
            if (Helper.IsHost()) MoveTime(1);
        }
    }

    public static void Update() 
    {
        if (Save.Inst == null) return;

        if (Target == EnvironmentType.Null)
            SetTarget(Weather);
        else
            SetTarget(Target);

        if (_currentTransitionTime < TransitionLength - 1)
        {
            _currentTransitionTime++;
            float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
            Environment previous = Environments[_previous];
            Environment current = Environments[_current];
            Set(Color.Lerp(previous.AmbientLight, current.AmbientLight, t), 
                Color.Lerp(previous.FogColor, current.FogColor, t),
                Color.Lerp(previous.SpotLight, current.SpotLight, t),
                Color.Lerp(previous.DirectionalLight, current.DirectionalLight, t),
                Color.Lerp(previous.BackgroundColor, current.BackgroundColor, t)); 
        }
    }

    public static void MoveTime(int amount)
    {
        if (!Helper.IsHost()) return;
        while (amount != 0)
        {  
            CheckWeather(); 
            Time++;
            amount--;
            if (Time == Length)
            {
                Time = 0;
                Save.Inst.day++;
                // Judge the previous day's Maw quota and set today's.
                MawQuota.OnDayPassed();
                // Auto-save at the start of each new day when enabled.
                if (Settings.Inst != null && Settings.Inst.AutoSaveIndex == 1)
                    Saves.SaveGame();
            }

            if (Time % 60 == 0)
            {
                TriggerHourly(Time / 60, Save.Inst.day);
            }
        }
    }

    private static void TriggerHourly(int hour, int day)
    {
        HourlyTriggered?.Invoke(hour, day);
    }
    
    private static void CheckWeather()
    {
        if (Time == 0)
        { 
            // Rapture is a rare day-long event (15% chance), but never before
            // day 10 — the early game stays calm.
            if (Save.Inst.day >= 10 && Random.value < 0.15f)
            {
                Weather = EnvironmentType.Rapture;
                Dialogue.ShowEvent("The sky is red...");
            }
            // Winter (snowy days) only begins after day 15; before that it's a
            // normal day.
            else if (Save.Inst.day >= 15)
                Weather = EnvironmentType.DaySnow;
            else
                Weather = EnvironmentType.Day;
        } 
        else if (Time == 60 * 18)
            Weather = EnvironmentType.Sunset;
        else if (Time == 60 * 19)
            // The full moon (bright night) only rises after day 10.
            if (Save.Inst.day >= 10 && Random.value < 0.7f)
                Weather = EnvironmentType.NightBright;
            else
            {
                Weather = EnvironmentType.NightRainy;
                if (Save.Inst.day >= 10 && Random.value < 0.3f)
                    Dialogue.ShowEvent("The full moon rises...");
            }
        else if (Time == 60 * 23)
            Weather = EnvironmentType.Sunrise;
    }

    public static void Set(Color ambientLight, Color fogColor, Color spotLight, Color directionalLight,
        Color backgroundColor)
    {
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.fogColor = fogColor;
        Main.SpotLight.color = spotLight;
        Main.DirectionalLight.color = directionalLight;
        Main.Camera.backgroundColor = backgroundColor;
    }
 
}