using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EnvironmentType
{
    Null, Black, Sunrise, Rapture, Day, DaySnow, Sunset, NightRainy, NightBright
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
    private const float Speed = 0.4f;
    private const int TransitionLength = 200;
    private static int _currentTransitionTime;  
    private static EnvironmentType _previous;
    private static EnvironmentType _current = EnvironmentType.Black; 
    public static EnvironmentType Target = EnvironmentType.Null;
    private static int Time
    {
        get => SaveData.Inst.time;
        set => SaveData.Inst.time = value;
    }
    private static EnvironmentType Weather
    {
        get => SaveData.Inst.weather;
        set => SaveData.Inst.weather = value;
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
            MoveTime(1);
        } 
    }

    public static void Update() 
    {
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
        while (amount != 0)
        {  
            CheckWeather(); 
            Time++;
            amount--;
            if (Time == Length)
            {
                Time = 0;
                SaveData.Inst.day++;
            }

            if (Time % 60 == 0)
            {
                TriggerHourly(Time / 60, SaveData.Inst.day);
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
            if (Random.value < 0.5f)
                Weather = EnvironmentType.Day;
            else
                Weather = EnvironmentType.DaySnow;
        } 
        else if (Time == 60 * 11)
            if (Random.value < 0.8f)
                Weather = EnvironmentType.Sunset;
            else
                Weather = EnvironmentType.Rapture;
        else if (Time == 60 * 14)
            if (Random.value < 0.7f)
                Weather = EnvironmentType.NightRainy;
            else
                Weather = EnvironmentType.NightBright;
        else if (Time == Length * 23/24)
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