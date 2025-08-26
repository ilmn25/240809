using UnityEngine;

public class BedMachine: StructureMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new Info();
    }
    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
        {
            // SetState<InCraftingState>();
        } 
        else 
            SetState<DefaultState>();
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
        Audio.PlaySFX(SfxID.Notification);
        Game.Player.transform.position = new Vector3Int(2, 50, 2);
        if (IsCurrentState<DefaultState>())
        {
            // SetState<InCraftingState>();
        } 
        else 
            SetState<DefaultState>();
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