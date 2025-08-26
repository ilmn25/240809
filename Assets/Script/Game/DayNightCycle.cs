using UnityEngine;
using Random = UnityEngine.Random;

public class DayNightCycle
{
    public Color AmbientLight;
    public Color FogColor;
    public Color SpotLight;
    public Color DirectionalLight;
    public Color BackgroundColor;
    public EnvParticles EnvParticles = EnvParticles.Null;
    
    public static DayNightCycle Rapture = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(39, 38, 64),
        FogColor = Helper.GetColor(97, 39, 39),
        SpotLight = Helper.GetColor(161, 77, 77),
        DirectionalLight = Helper.GetColor(255, 68, 47),
        BackgroundColor = Helper.GetColor(190, 130, 134),
        EnvParticles = EnvParticles.Leaf
    };
    public static DayNightCycle Day = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(225, 225, 225),
        FogColor = Helper.GetColor(117, 110, 138),
        SpotLight = Helper.GetColor(197, 142, 88),
        DirectionalLight = Helper.GetColor(135, 124, 121),
        BackgroundColor = Helper.GetColor(116, 113, 137),
        EnvParticles = EnvParticles.Leaf
    };
    public static DayNightCycle DayFog = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Helper.GetColor(146, 146, 146),
        SpotLight = Helper.GetColor(97, 97, 97),
        DirectionalLight = Helper.GetColor(105, 101, 159),
        BackgroundColor = Helper.GetColor(131, 131, 135),
        EnvParticles = EnvParticles.Snow
    };
    
    public static DayNightCycle Noon = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(255, 184, 184),
        FogColor = Helper.GetColor(118, 105, 105),
        SpotLight = Helper.GetColor(103, 70, 66),
        DirectionalLight = Helper.GetColor(255, 184, 56),
        BackgroundColor = Helper.GetColor(188, 111, 77),
        EnvParticles = EnvParticles.Leaf
    };

    public static DayNightCycle Night = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(97, 97, 97),
        DirectionalLight = Helper.GetColor(91, 56, 255),
        BackgroundColor = Helper.GetColor(45, 50, 63),
        EnvParticles = EnvParticles.Rain
    };
    public static DayNightCycle NightFull = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(164, 138, 129),
        DirectionalLight = Helper.GetColor(91, 56, 255),
        BackgroundColor = Helper.GetColor(45, 50, 63) 
    };
    public static DayNightCycle Sunrise = new DayNightCycle()
    {
        AmbientLight = Helper.GetColor(38, 37, 63),
        FogColor = Color.black,
        SpotLight = Helper.GetColor(154, 90, 69),
        DirectionalLight = Helper.GetColor(254, 57, 90),
        BackgroundColor = Helper.GetColor(75, 59, 55),
        EnvParticles = EnvParticles.Leaf
    };
     
    private const int Length = 10000;
    private const int TransitionLength = 200;
    private static int _currentTransitionTime; 
    private static int Time
    {
        get => World.Inst.time;
        set => World.Inst.time = value;
    }
    private static DayNightCycle _previous = Night;
    private static DayNightCycle _current = Sunrise;

    private static void SetTarget(DayNightCycle target)
    {
        _previous = _current;
        _current = target;
        _currentTransitionTime = 0;
        EnvParticle.Set(target.EnvParticles);
    }
    public static void Update()
    {
        if (Time == 0)
        {
            World.Inst.day++;
            if (Random.value < 0.5f)
                SetTarget(Day);
            else
                SetTarget(DayFog);
        } 
        else if (Time == Length * 12/24)
            if (Random.value < 0.7f)
                SetTarget(Noon);
            else
                SetTarget(Rapture);
        else if (Time == Length * 17/24)
            if (Random.value < 0.7f)
                SetTarget(Night);
            else
                SetTarget(NightFull);
        else if (Time == Length * 23/24)
            SetTarget(Sunrise);
        
        Time++; 
        if (Time == Length) Time = 0;
        if (_currentTransitionTime < TransitionLength - 1) _currentTransitionTime++;
        
        float t = Mathf.InverseLerp(0, TransitionLength, _currentTransitionTime % TransitionLength);
        Set(Color.Lerp(_previous.AmbientLight, _current.AmbientLight, t), 
            Color.Lerp(_previous.FogColor, _current.FogColor, t),
            Color.Lerp(_previous.SpotLight, _current.SpotLight, t),
            Color.Lerp(_previous.DirectionalLight, _current.DirectionalLight, t),
            Color.Lerp(_previous.BackgroundColor, _current.BackgroundColor, t)); 
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
}