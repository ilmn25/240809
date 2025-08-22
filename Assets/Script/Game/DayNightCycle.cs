using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class DayNightCycle
{
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public Color BackgroundColor;
 
    public static DayNightCycle Rapture = new DayNightCycle()
    {
        AmbientLight = GetColor(39, 38, 64),
        FogColor = GetColor(97, 39, 39),
        SpotLight = GetColor(161, 77, 77),
        DirectionalLight = GetColor(255, 68, 47),
        BackgroundColor = GetColor(190, 130, 134)
    };
    public static DayNightCycle Day = new DayNightCycle()
    {
        AmbientLight = GetColor(225, 225, 225),
        FogColor = GetColor(117, 110, 138),
        SpotLight = GetColor(197, 142, 88),
        DirectionalLight = GetColor(135, 124, 121),
        BackgroundColor = GetColor(116, 113, 137)
    };
    public static DayNightCycle DayFog = new DayNightCycle()
    {
        AmbientLight = GetColor(38, 37, 63),
        FogColor = GetColor(146, 146, 146),
        SpotLight = GetColor(97, 97, 97),
        DirectionalLight = GetColor(105, 101, 159),
        BackgroundColor = GetColor(131, 131, 135)
    };
    
    public static DayNightCycle Noon = new DayNightCycle()
    {
        AmbientLight = GetColor(255, 184, 184),
        FogColor = GetColor(118, 105, 105),
        SpotLight = GetColor(103, 70, 66),
        DirectionalLight = GetColor(255, 184, 56),
        BackgroundColor = GetColor(188, 111, 77)
    };

    public static DayNightCycle Night = new DayNightCycle()
    {
        AmbientLight = GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = GetColor(97, 97, 97),
        DirectionalLight = GetColor(91, 56, 255),
        BackgroundColor = GetColor(45, 50, 63)
    };
    public static DayNightCycle NightFull = new DayNightCycle()
    {
        AmbientLight = GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = GetColor(164, 138, 129),
        DirectionalLight = GetColor(91, 56, 255),
        BackgroundColor = GetColor(45, 50, 63)
    };
    public static DayNightCycle Sunrise = new DayNightCycle()
    {
        AmbientLight = GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = GetColor(164, 138, 129),
        DirectionalLight = GetColor(254, 57, 90),
        BackgroundColor = GetColor(45, 50, 63)
    };
    
    public static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
    
    private const int Length = 20000;
    private const int TransitionLength = 200;
    private static int _currentTransitionTime; 
    private static int _currentTime;
    private static DayNightCycle _previous = Night;
    private static DayNightCycle _current = Day;

    private static void SetTarget(DayNightCycle target)
    {
        _previous = _current;
        _current = target;
        _currentTransitionTime = 0;
    }
    public static void Update()
    {  
        if (_currentTime == 0)
            if (Random.value < 0.8f)
                SetTarget(Day);
            else
                SetTarget(DayFog);
        else if (_currentTime == Length * 12/24)
            if (Random.value < 0.9f)
                SetTarget(Noon);
            else
                SetTarget(Rapture);
        else if (_currentTime == Length * 19/24)
            if (Random.value < 0.7f)
                SetTarget(Night);
            else
                SetTarget(NightFull);
        
        _currentTime++; 
        if (_currentTime == Length) _currentTime = 0;
        if (_currentTransitionTime < TransitionLength - 1) _currentTransitionTime++;
        
        float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
        RenderSettings.ambientLight = Color.Lerp(_previous.AmbientLight, _current.AmbientLight, t);
        RenderSettings.fogColor = Color.Lerp(_previous.FogColor, _current.FogColor, t);
        Game.SpotLight.color = Color.Lerp(_previous.SpotLight, _current.SpotLight, t);
        Game.DirectionalLight.color = Color.Lerp(_previous.DirectionalLight, _current.DirectionalLight, t);
        Game.Camera.backgroundColor = Color.Lerp(_previous.BackgroundColor, _current.BackgroundColor, t);
    }   
}