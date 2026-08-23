using System.Collections.Generic;
using UnityEngine;

public class GuideMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public static Info CreateInfo()
    {
        return new PassiveInfo()
        {
            HealthMax = 50,
            SpeedGround = 5,
            SpeedAir = 6,
            DistRoam = 3,
            IsNPC = true,
        };
    }

    public override void OnStart()
    {
        base.OnStart();

        AddState(new DialogueState(CreateGuideDialogue()));
    }

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;
        SetState<DialogueState>();
    }

    public override void OnUpdate()
    {
        UpdateFlee();
    }

    // Steps the player through the early-game progression. Pressing the interact
    // key again advances to the next tip.
    private static Dialogue CreateGuideDialogue()
    {
        Dialogue tip(string text, Dialogue next = null)
        {
            return new Dialogue
            {
                Text = text,
                Sprite = Cache.LoadSprite("Sprite/Guide"),
                Next = next == null ? null : new Dictionary<string, Dialogue> { [""] = next },
            };
        }

        return tip("Are you also a delver?, welcome to the Abyss!",
            tip("The Abyss is a dangerous place.",
                tip("You can craft items using the resources you find.",
                    tip("You can also build structures to protect yourself.",
                        tip("Good luck!")))));
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
