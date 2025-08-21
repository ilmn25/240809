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

public class DayNightCycle
{
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public Color BackgroundColor;
 
    public static DayNightCycle Day = new DayNightCycle()
    {
        AmbientLight = GetColor(225, 225, 225),
        FogColor = GetColor(117, 110, 138),
        SpotLight = GetColor(197, 142, 88),
        DirectionalLight = GetColor(135, 124, 121),
        BackgroundColor = GetColor(116, 113, 137)
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
    
    public static Color GetColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
    
    private const int Length = 1000;
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
        _currentTime++; 
        if (_currentTime == Length) _currentTime = 0;
        if (_currentTransitionTime < TransitionLength - 1) _currentTransitionTime++; 
        
        if (_currentTime == 0)
            SetTarget(Day);
        else if (_currentTime == Length / 3)
            SetTarget(Noon);
        else if (_currentTime == Length / 2)
            SetTarget(Night);  
        
        float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
        RenderSettings.ambientLight = Color.Lerp(_previous.AmbientLight, _current.AmbientLight, t);
        RenderSettings.fogColor = Color.Lerp(_previous.FogColor, _current.FogColor, t);
        Game.SpotLight.color = Color.Lerp(_previous.SpotLight, _current.SpotLight, t);
        Game.DirectionalLight.color = Color.Lerp(_previous.DirectionalLight, _current.DirectionalLight, t);
        Game.Camera.backgroundColor = Color.Lerp(_previous.BackgroundColor, _current.BackgroundColor, t);
    }   
}