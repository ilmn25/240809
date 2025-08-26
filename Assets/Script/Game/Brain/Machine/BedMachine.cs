using System.Collections;
using UnityEngine;

public class BedMachine: StructureMachine, IActionSecondaryInteract
{
    private (int, int) _target = (-1, -1);
    public static Info CreateInfo()
    {
        return new Info();
    }
    public void OnActionSecondary(Info info)
    { 
        if (_target.Item1 != -1) return; 
        Game.PlayerInfo.spawnPoint = transform.position;
        Game.PlayerInfo.SpeedModifier = 0.001f;
        _target = Environment.CalculateTime(Environment.Length / 2);
        Environment.Tick = 10;
    }

    public override void OnUpdate()
    {
        if (_target.Item1 == -1) return;
        if (_target.Item1 <= World.Inst.day && _target.Item2 <= World.Inst.time)
        {  
            Environment.Target = null;
            _target.Item1 = -1;
            Environment.Tick = 1;
            Game.PlayerInfo.SpeedModifier = 1;
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
    }

    private IEnumerator Portal()
    {
        Environment.Target = Environment.Black; 
        Game.PlayerInfo.SpeedModifier = 0.001f;
        yield return new WaitForSeconds(2);
        Game.Player.transform.position = new Vector3Int(2, 50, 2); 
        yield return new WaitForSeconds(3);
        Game.PlayerInfo.SpeedModifier = 1;
        Environment.Target = null; 
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
        Dialogue dialogue = new Dialogue(); 
        dialogue.Lines.Add("illu was here\n25/08/26"); 
        
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