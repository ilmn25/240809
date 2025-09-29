using System.Collections;
using UnityEngine;

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