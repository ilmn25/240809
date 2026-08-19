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

        return tip("I'm the Guide. I can point you toward the way forward.",
            tip("Start by gathering Flint and Sticks from the ground, and from mining dirt and slabs.",
                tip("Craft a Workbench, then make a StoneHatchet and a StonePickaxe.",
                    tip("Chop trees with the StoneHatchet for Log, and mine stone with the StonePickaxe for Gravel and ores.",
                        tip("Build a Campfire to cook meat and turn Log into Charcoal.",
                            tip("Then build a Furnace to smelt Steel — the path to the Sawmill and Stonecutter lies beyond."))))));
    }

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
