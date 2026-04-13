using Mirror;
using UnityEngine;

public class WorldSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDayChanged))] public int day;
    [SyncVar(hook = nameof(OnTimeChanged))] public int time;
    [SyncVar(hook = nameof(OnWeatherChanged))] public EnvironmentType weather;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void Update()
    {
        if (!isServer || Save.Inst == null) return;
        if (Save.Inst.day != day || Save.Inst.time != time || Save.Inst.weather != weather)
        {
            CopyFromSave();
        }
    }

    private void CopyFromSave()
    { 
        day = Save.Inst.day;
        time = Save.Inst.time;
        weather = Save.Inst.weather;
    }

    private void ApplyToSave()
    {
        if (isServer || Save.Inst == null) return;
        Save.Inst.day = day;
        Save.Inst.time = time;
        Save.Inst.weather = weather;
    }

    private void OnDayChanged(int _, int newValue) => ApplyToSave();
    private void OnTimeChanged(int _, int newValue) => ApplyToSave();
    private void OnWeatherChanged(EnvironmentType _, EnvironmentType newValue) => ApplyToSave();
}
