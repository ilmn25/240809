using System.Collections.Generic;
using UnityEngine;

public class GuideMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public const int FlowerCost = 2;
    public const float CompanionDuration = 45f;

    /// <summary>Every meadow flower the Guide asks for, two of each.</summary>
    private static readonly ID[] FlowerTypes = { ID.Orchids, ID.Tulip, ID.Daisies };

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

    public void OnActionSecondary(Info info)
    {
        if (Info.Target != null) return;

        PlayerInfo payer = info as PlayerInfo;
        if (payer == null) payer = Main.PlayerInfo;
        if (payer?.Storage == null)
        {
            Dialogue.ShowEvent("The guide looks at you expectantly.");
            return;
        }

        ShowMenu(payer);
    }

    public override void OnUpdate()
    {
        UpdateFlee();
    }

    /// <summary>The flower types the payer is short of, FlowerCost being the mark.</summary>
    private static List<ID> MissingFlowers(PlayerInfo payer)
    {
        List<ID> missing = new List<ID>();
        foreach (ID flower in FlowerTypes)
            if (payer.Storage.Count(flower) < FlowerCost)
                missing.Add(flower);
        return missing;
    }

    /// <summary>Offers to hire the Guide like the mercenary, or to hear his tips.</summary>
    private void ShowMenu(PlayerInfo payer)
    {
        Dictionary<string, Dialogue> options = new Dictionary<string, Dialogue>();

        List<ID> missing = MissingFlowers(payer);
        if (missing.Count == 0)
        {
            options[$"Recruit for {FlowerCost} of each flower"] = new Dialogue
            {
                Text = $"\"I'll walk with you a while.\" The Guide takes the flowers and joins you for {CompanionDuration:0} seconds.",
                Sprite = Cache.LoadSprite("Sprite/Guide"),
                OnOpen = () => CompleteRecruit(payer),
            };
        }
        else
        {
            options["Recruit"] = new Dialogue
            {
                Text = $"\"I need {FlowerCost} of each flower — still missing {string.Join(", ", missing)}.\"",
                Sprite = Cache.LoadSprite("Sprite/Guide"),
            };
        }

        options["Ask for advice"] = CreateGuideDialogue();
        options["Leave"] = new Dialogue { Text = "\"Stay safe out there.\"" };

        Dialogue.Target = new Dialogue
        {
            Text = $"\"Need a hand? {FlowerCost} of each flower buys you a short escort.\"",
            Sprite = Cache.LoadSprite("Sprite/Guide"),
            Next = options,
        };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    /// <summary>Pays the flowers and spawns a temporary Guide companion, like a hired mercenary.</summary>
    private void CompleteRecruit(PlayerInfo payer)
    {
        if (Save.Inst == null || payer?.Storage == null) return;
        if (MissingFlowers(payer).Count > 0) return;

        foreach (ID flower in FlowerTypes)
            payer.Storage.RemoveItem(flower, FlowerCost);

        PlayerInfo companion = (PlayerInfo)Entity.CreateInfo(ID.Delver, transform.position);
        companion.CharSprite = ID.Guide;
        companion.Storage.CreateAndAddItem(ID.SteelSword);
        if (Entity.SpawnFromInfo(companion, false) is DelverMachine delver)
            delver.ExpireAt = Time.time + CompanionDuration;

        Info.Destroy();
        Unload();
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
