using UnityEngine;

/// <summary>A quest-giver NPC. Interact to hear the current quest; when the task
/// is done it accepts the hand-in, drops the reward, and offers the next quest.</summary>
public class QuestmasterMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.DistEscape = 17;
        info.CharSprite = ID.Questmaster;
        return info;
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new QuestmasterState());
    }

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();
        SetState<QuestmasterState>();
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
