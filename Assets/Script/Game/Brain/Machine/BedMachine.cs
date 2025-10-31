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
        if (SaveData.Inst.weather == EnvironmentType.Day) return;
        _ = new CoroutineTask(Sleep());
        return;

        IEnumerator Sleep()
        {
            Environment.Target = EnvironmentType.Black;
            Main.PlayerInfo.SpeedModifier = 0.001f;
            yield return new WaitForSeconds(3);
            Environment.MoveTime(Environment.Length / 2);
            yield return new WaitForSeconds(3);
            Main.PlayerInfo.SpeedModifier = 1;
            Environment.Target = EnvironmentType.Null;
        }
    }
}