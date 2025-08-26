using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Environment
{
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public Color BackgroundColor;
    public EnvParticles EnvParticles = EnvParticles.Null;
    
    public static Environment Black = new Environment()
    {
        AmbientLight = Color.black,
        FogColor = Color.black,
        SpotLight = Color.black,
        DirectionalLight = Color.black,
        BackgroundColor = Color.black,
    };
    public static Environment Rapture = new Environment()
    {
        AmbientLight = Helper.GetColor(39, 38, 64),
        FogColor = Helper.GetColor(97, 39, 39),
        SpotLight = Helper.GetColor(161, 77, 77),
        DirectionalLight = Helper.GetColor(255, 68, 47),
        BackgroundColor = Helper.GetColor(190, 130, 134),
        EnvParticles = EnvParticles.Leaf
    };
    public static Environment Day = new Environment()
    {
        AmbientLight = Helper.GetColor(225, 225, 225),
        FogColor = Helper.GetColor(117, 110, 138),
        SpotLight = Helper.GetColor(197, 142, 88),
        DirectionalLight = Helper.GetColor(135, 124, 121),
        BackgroundColor = Helper.GetColor(116, 113, 137),
        EnvParticles = EnvParticles.Leaf
    };
    public static Environment DayFog = new Environment()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Helper.GetColor(146, 146, 146),
        SpotLight = Helper.GetColor(97, 97, 97),
        DirectionalLight = Helper.GetColor(105, 101, 159),
        BackgroundColor = Helper.GetColor(131, 131, 135),
        EnvParticles = EnvParticles.Snow
    };
    
    public static Environment Noon = new Environment()
    {
        AmbientLight = Helper.GetColor(255, 184, 184),
        FogColor = Helper.GetColor(118, 105, 105),
        SpotLight = Helper.GetColor(103, 70, 66),
        DirectionalLight = Helper.GetColor(255, 184, 56),
        BackgroundColor = Helper.GetColor(188, 111, 77),
        EnvParticles = EnvParticles.Leaf
    };

    public static Environment Night = new Environment()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(97, 97, 97),
        DirectionalLight = Helper.GetColor(91, 56, 255),
        BackgroundColor = Helper.GetColor(45, 50, 63),
        EnvParticles = EnvParticles.Rain
    };
    public static Environment NightFull = new Environment()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(164, 138, 129),
        DirectionalLight = Helper.GetColor(91, 56, 255),
        BackgroundColor = Helper.GetColor(45, 50, 63) 
    };
    public static Environment Sunrise = new Environment()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(154, 90, 69),
        DirectionalLight = Helper.GetColor(254, 57, 90),
        BackgroundColor = Helper.GetColor(75, 59, 55),
        EnvParticles = EnvParticles.Leaf
    };
     
    public static readonly int Length = 30000;
    public static int TickSpeed = 1;
    private const int TransitionLength = 200;
    private static int _currentTransitionTime;  
    private static Environment _previous = Night;
    private static Environment _current = Black; 
    public static Environment Target = null;
    private static int Time
    {
        get => World.Inst.time;
        set => World.Inst.time = value;
    }
    private static Environment Weather
    {
        get => World.Inst.weather;
        set => World.Inst.weather = value;
    }

    private static void SetTarget(Environment target)
    {
        if (target == _current) return;
        _previous = _current;
        _current = target;
        _currentTransitionTime = 0;
        EnvParticle.Set(target.EnvParticles);
    }
    public static void Update()
    {
        MoveTime(TickSpeed);

        if (Target == null)
        {
            SetTarget(Weather);
        }
        else
        {
            SetTarget(Target);
        }
         
        if (_currentTransitionTime < TransitionLength - 1)
        {
            _currentTransitionTime++;
            float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
            Set(Color.Lerp(_previous.AmbientLight, _current.AmbientLight, t), 
                Color.Lerp(_previous.FogColor, _current.FogColor, t),
                Color.Lerp(_previous.SpotLight, _current.SpotLight, t),
                Color.Lerp(_previous.DirectionalLight, _current.DirectionalLight, t),
                Color.Lerp(_previous.BackgroundColor, _current.BackgroundColor, t)); 
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
                World.Inst.day++;
            } 
        }
    }
    
    private static void CheckWeather()
    {
        if (Time == 0)
        { 
            if (Random.value < 0.5f)
                Weather = Day;
            else
                Weather = DayFog;
        } 
        else if (Time == Length * 12/24)
            if (Random.value < 0.8f)
                Weather = Noon;
            else
                Weather = Rapture;
        else if (Time == Length * 17/24)
            if (Random.value < 0.7f)
                Weather = Night;
            else
                Weather = NightFull;
        else if (Time == Length * 23/24)
            Weather = Sunrise;
    }

    public static void Set(Color ambientLight, Color fogColor, Color spotLight, Color directionalLight,
        Color backgroundColor)
    {
        RenderSettings.ambientLight = ambientLight;
        RenderSettings.fogColor = fogColor;
        Game.SpotLight.color = spotLight;
        Game.DirectionalLight.color = directionalLight;
        Game.Camera.backgroundColor = backgroundColor;
    }

    public static (int day, int time) CalculateTime(int amount)
    {
        int target = Time + amount;
        int day = target / Length + World.Inst.day;
        int time = target % Length;
        return (day, time);
    }
}