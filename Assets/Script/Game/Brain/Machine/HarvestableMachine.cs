using UnityEngine;

public class HarvestableMachine : StructureMachine
{
    public static Info CreateInfo()
    {
        return new HarvestableInfo();
    }

    public override void OnStart()
    {
        base.OnStart();
        AddState(new StaticIdle(), true);
    }

    public override void OnUpdate()
    {
        // Tick the regrow cooldown so a picked bush can be harvested again later.
        if (Info is HarvestableInfo h && h.RegrowTimer > 0f)
        {
            h.RegrowTimer -= Time.deltaTime;
            if (h.RegrowTimer < 0f) h.RegrowTimer = 0f;
        }
    }
}
