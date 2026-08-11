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
            ScreenFade.FadeOut(0.5f);
            Main.PlayerInfo.SpeedModifier = 0.001f;
            yield return new WaitForSeconds(0.6f);
            Main.Player.transform.position = new Vector3Int(2, 50, 2); 
            ScreenFade.FadeIn(0.5f);
            Main.PlayerInfo.SpeedModifier = 1;
            yield return new WaitForSeconds(0.6f); 
        }
    }
}