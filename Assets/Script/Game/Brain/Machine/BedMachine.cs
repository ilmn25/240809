using System.Collections;
using UnityEngine;

public class BedMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }

    public void OnActionSecondary(Info info)
    {
        if (Save.Inst.weather == EnvironmentType.Day) return;
        _ = new CoroutineTask(Sleep());
        return;

        IEnumerator Sleep()
        {
            ScreenFade.FadeOut(0.5f);
            Main.PlayerInfo.SpeedModifier = 0.001f;
            yield return new WaitForSeconds(2.5f);
            Environment.MoveTime(Environment.Length / 2);
            ScreenFade.FadeIn(0.5f);
            yield return new WaitForSeconds(0.6f);
            Main.PlayerInfo.SpeedModifier = 1;
        }
    }
}