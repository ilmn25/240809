using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EnvironmentType
{
    Null, Black, Sunrise, Rapture, Day, DaySnow, Sunset, NightRainy, NightBright, Dim, Backrooms, DryDay
}
public class Environment
{
    public const int NightStartTime = 60 * 19;
    public const float SpawnLightRadius = 10f;
    private const float NormalSpotLightIntensity = 4000f;
    private const float BaseSpotLightIntensity = 10000f;
    private const float SpotLightFadeSpeed = 4000f;
    public static Dictionary<EnvironmentType, Environment> Environments = new ();
    public static event Action<int, int> HourlyTriggered;
    public static event Action<EnvironmentType> WeatherChanged;
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public bool Rain;
    public bool Snow;
    public bool Lightning;

    static Environment()
    {
        Environments.Add(EnvironmentType.Black, new Environment
        {
            AmbientLight = Color.black,
            FogColor = Color.black,
            SpotLight = Color.black,
            DirectionalLight = Color.black
        });
        Environments.Add(EnvironmentType.Dim, new Environment
        {
            AmbientLight = Helper.GetColor(16, 15, 28),
            FogColor = Helper.GetColor(12, 11, 20),
            SpotLight = Helper.GetColor(26, 25, 34),
            DirectionalLight = Helper.GetColor(22, 20, 46)
        });
        // The world lies underground, but the fog is what swallows the distance, so the
        // daytime weathers can stay properly lit — only the night weathers actually go
        // dark. The lit hours get a lighter haze (~20%), the nights stay almost black.
        Environments.Add(EnvironmentType.Rapture, new Environment
        {
            AmbientLight = Helper.GetColor(150, 100, 100),
            FogColor = Helper.GetColor(52, 19, 16),
            SpotLight = Helper.GetColor(190, 90, 90),
            DirectionalLight = Helper.GetColor(230, 70, 50)
        });
        Environments.Add(EnvironmentType.Day, new Environment
        {
            AmbientLight = Helper.GetColor(224, 224, 224),
            FogColor = Helper.GetColor(50, 50, 50),
            SpotLight = Helper.GetColor(198, 192, 180),
            DirectionalLight = Helper.GetColor(130, 130, 130)
        });
        // A dry day: the same light level, but warm and dusty.
        Environments.Add(EnvironmentType.DryDay, new Environment
        {
            AmbientLight = Helper.GetColor(225, 225, 225),
            FogColor = Helper.GetColor(52, 51, 47),
            SpotLight = Helper.GetColor(197, 142, 88),
            DirectionalLight = Helper.GetColor(135, 124, 121)
        });
        Environments.Add(EnvironmentType.DaySnow, new Environment
        {
            AmbientLight = Helper.GetColor(178, 178, 184),
            FogColor = Helper.GetColor(50, 50, 53),
            SpotLight = Helper.GetColor(150, 150, 158),
            DirectionalLight = Helper.GetColor(118, 114, 150),
            Snow = true
        });
        Environments.Add(EnvironmentType.Sunset, new Environment
        {
            AmbientLight = Helper.GetColor(156, 124, 112),
            FogColor = Helper.GetColor(54, 37, 28),
            SpotLight = Helper.GetColor(196, 144, 104),
            DirectionalLight = Helper.GetColor(232, 140, 80)
        });
        Environments.Add(EnvironmentType.NightRainy, new Environment
        {
            AmbientLight = Helper.GetColor(22, 22, 30),
            FogColor = Helper.GetColor(3, 3, 5),
            SpotLight = Helper.GetColor(88, 88, 102),
            DirectionalLight = Helper.GetColor(56, 44, 132),
            Rain = true,
            Lightning = true
        });
        Environments.Add(EnvironmentType.NightBright, new Environment
        {
            AmbientLight = Helper.GetColor(42, 42, 56),
            FogColor = Helper.GetColor(9, 9, 14),
            SpotLight = Helper.GetColor(148, 148, 172),
            DirectionalLight = Helper.GetColor(82, 70, 156)
        });
        Environments.Add(EnvironmentType.Sunrise, new Environment
        {
            AmbientLight = Helper.GetColor(126, 120, 132),
            FogColor = Helper.GetColor(22, 21, 28),
            SpotLight = Helper.GetColor(176, 166, 196),
            DirectionalLight = Helper.GetColor(168, 132, 196)
        });
        Environments.Add(EnvironmentType.Backrooms, new Environment
        {
            AmbientLight = Helper.GetColor(215, 212, 195),
            FogColor = Helper.GetColor(112, 108, 95),
            SpotLight = Helper.GetColor(190, 172, 120),
            DirectionalLight = Helper.GetColor(128, 124, 108)
        });
        _ = new CoroutineTask(Clock());
    }       
     
    public const int Length = 60 * 24;
    public const float SecondsPerGameMinute = 0.24f;
    private const int TransitionLength = 200;
    private static int _currentTransitionTime;  
    private static EnvironmentType _previous = EnvironmentType.Black;
    private static EnvironmentType _current = EnvironmentType.Black; 
    private static EnvironmentType _lastWeather = EnvironmentType.Null;
    public static EnvironmentType Target = EnvironmentType.Null;
    private static int Time
    {
        get => Save.Inst.time;
        set => Save.Inst.time = value;
    }
    private static EnvironmentType Weather
    {
        get => Save.Inst.weather;
        set
        {
            if (Save.Inst.weather == value) return;
            Save.Inst.weather = value;
            _lastWeather = value;
            WeatherChanged?.Invoke(value);
        }
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
        Set(env.AmbientLight, env.FogColor, env.SpotLight, env.DirectionalLight);
    }

    private static void SetTarget(EnvironmentType target)
    {
        if (target == _current) return;
        _previous = _current;
        _current = target;
        _currentTransitionTime = 0;
    }

    /// <summary>
    /// Environment forced by the dimension the player is standing in, overriding the
    /// time-of-day weather. The backrooms are lit by a permanent half-bright glow;
    /// every other dimension follows the weather (Null = no override).
    /// </summary>
    private static EnvironmentType DimensionEnvironment()
        => Save.Inst != null && Save.Inst.current == GenType.Backrooms
            ? EnvironmentType.Backrooms
            : EnvironmentType.Null;

    public static IEnumerator Clock()
    {
        while (true)
        {
            yield return new WaitForSeconds(SecondsPerGameMinute);
            if (Helper.IsHost()) MoveTime(1);
        }
    }

    public static void Update() 
    {
        if (Save.Inst == null) return;
        if (_lastWeather != Save.Inst.weather)
        {
            _lastWeather = Save.Inst.weather;
            WeatherChanged?.Invoke(_lastWeather);
        }

        // An explicit Target (menu, intermission) outranks everything; otherwise the
        // dimension's own lighting wins over the weather.
        EnvironmentType dimension = DimensionEnvironment();
        if (Target != EnvironmentType.Null)
            SetTarget(Target);
        else if (dimension != EnvironmentType.Null)
            SetTarget(dimension);
        else
            SetTarget(Weather);

        if (_currentTransitionTime < TransitionLength - 1)
        {
            _currentTransitionTime++;
            float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
            Environment previous = Environments[_previous];
            Environment current = Environments[_current];
            Set(Color.Lerp(previous.AmbientLight, current.AmbientLight, t), 
                Color.Lerp(previous.FogColor, current.FogColor, t),
                Color.Lerp(previous.SpotLight, current.SpotLight, t),
                Color.Lerp(previous.DirectionalLight, current.DirectionalLight, t)); 
        }
    }

    /// <summary>The lit hole at the centre of the map (the spawn) brightens the world
    /// spotlight, scaled by how dark the current weather is: during the lit hours it adds
    /// almost nothing, at night it floods the base.</summary>
    public static void UpdatePlayerLight()
    {
        if (Main.SpotLight == null || Save.Inst == null || Main.PlayerInfo == null || World.Inst == null)
            return;

        float target = NormalSpotLightIntensity;
        if (IsPlayerNearSpawn())
        {
            float darkness = 1f - Environments[_current].AmbientLight.grayscale;
            target = Mathf.Lerp(NormalSpotLightIntensity, BaseSpotLightIntensity, darkness);
        }

        Main.SpotLight.intensity = Mathf.MoveTowards(Main.SpotLight.intensity, target,
            SpotLightFadeSpeed * UnityEngine.Time.deltaTime);
    }

    private static bool IsPlayerNearSpawn()
    {
        Vector3 playerPosition = Main.PlayerInfo.position;
        Vector3 spawnPosition = World.Inst.SpawnPoint;
        playerPosition.y = 0f;
        spawnPosition.y = 0f;
        return (playerPosition - spawnPosition).sqrMagnitude <= SpawnLightRadius * SpawnLightRadius;
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
            // Weather = EnvironmentType.DaySnow;
            // return;
            // Rapture is a rare day-long event (15% chance), but never before
            // day 10 — the early game stays calm.
            if (Save.Inst.day >= 10 && Random.value < 0.15f)
            {
                Weather = EnvironmentType.Rapture;
                Dialogue.ShowEvent("The sky is red...");
            }
            // Dry days (the warm, dusty version of a normal day) turn up now and then.
            else if (Random.value < 0.25f)
                Weather = EnvironmentType.DryDay;
            // Winter (snowy days) only begins after day 15; before that it's a
            // normal day.
            else if (Save.Inst.day >= 15)
                Weather = EnvironmentType.DaySnow;
            else
                Weather = EnvironmentType.Day;
        } 
        else if (Time == 60 * 18)
        {
            Weather = EnvironmentType.Sunset;
            if (Main.PlayerInfo != null &&
                World.Inst != null &&
                !IsPlayerNearSpawn())
            {
                Dialogue.ShowEvent("It will be dark soon. I should stay near base...");
            }
        }
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

    public static void Set(Color ambientLight, Color fogColor, Color spotLight, Color directionalLight)
    {
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.fogColor = fogColor;
        Main.SpotLight.color = spotLight;
        Main.DirectionalLight.color = directionalLight;
        Main.Camera.backgroundColor = fogColor;
    }
 
}