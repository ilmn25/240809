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
        if (World.Inst.weather == EnvironmentType.Day) return;
        _ = new CoroutineTask(Sleep());
        return;

        IEnumerator Sleep()
        {
            Environment.Target = EnvironmentType.Black;
            Game.PlayerInfo.SpeedModifier = 0.001f;
            yield return new WaitForSeconds(3);
            Environment.MoveTime(Environment.Length / 2);
            yield return new WaitForSeconds(3);
            Game.PlayerInfo.SpeedModifier = 1;
            Environment.Target = EnvironmentType.Null;
        }
    }
}

public class PortalMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }
     
    public void OnActionSecondary(Info info)
    {
        _ = new CoroutineTask(Portal());
        return;
        IEnumerator Portal()
        {
            Environment.Target = EnvironmentType.Black; 
            Game.PlayerInfo.SpeedModifier = 0.001f;
            yield return new WaitForSeconds(2);
            Game.Player.transform.position = new Vector3Int(2, 50, 2); 
            yield return new WaitForSeconds(3);
            Game.PlayerInfo.SpeedModifier = 1;
            Environment.Target = EnvironmentType.Null; 
        }
    }
}
public class SignMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }

    public override void OnStart()
    {
        base.OnStart();
        
        Dialogue dialogue = new Dialogue
        {
            Text = "illu was here\n25/08/26", 
        };
        AddState(new MessageState(dialogue)); 
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
        {
            SetState<MessageState>();
        } 
        else 
            SetState<DefaultState>();
    }
}