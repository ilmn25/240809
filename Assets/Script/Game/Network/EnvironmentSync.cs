using Mirror;
using UnityEngine;

public class EnvironmentSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnDayChanged))] public int day;
    [SyncVar(hook = nameof(OnTimeChanged))] public int time;
    // Default to Sunrise (not Null) — Null (0) is not in Environments dict
    // and would crash Environment.Update(), killing the whole Main.Update().
    [SyncVar(hook = nameof(OnWeatherChanged))] public EnvironmentType weather = EnvironmentType.Sunrise;
    // The Maw's daily quota state, so clients' Save.Inst stays consistent with the
    // host (the host owns the quota logic in Environment.MoveTime / MawQuota).
    [SyncVar(hook = nameof(OnMawQuotaChanged))] public int mawQuota;
    [SyncVar(hook = nameof(OnMawPaidChanged))] public int mawPaid;

    private void Update()
    {
        if (!isServer || Save.Inst == null) return;
        if (Save.Inst.day != day || Save.Inst.time != time || Save.Inst.weather != weather ||
            Save.Inst.mawQuota != mawQuota || Save.Inst.mawPaid != mawPaid)
        {
            CopyFromSave();
        }
    }

    private void CopyFromSave()
    { 
        day = Save.Inst.day;
        time = Save.Inst.time;
        weather = Save.Inst.weather;
        mawQuota = Save.Inst.mawQuota;
        mawPaid = Save.Inst.mawPaid;
    }

    private void ApplyToSave()
    {
        if (isServer || Save.Inst == null) return;
        Save.Inst.day = day;
        Save.Inst.time = time;
        Save.Inst.weather = weather;
        Save.Inst.mawQuota = mawQuota;
        Save.Inst.mawPaid = mawPaid;
    }

    private void OnDayChanged(int _, int newValue) => ApplyToSave();
    private void OnTimeChanged(int _, int newValue) => ApplyToSave();
    private void OnWeatherChanged(EnvironmentType _, EnvironmentType newValue) => ApplyToSave();
    private void OnMawQuotaChanged(int _, int newValue) => ApplyToSave();
    private void OnMawPaidChanged(int _, int newValue) => ApplyToSave();
}
