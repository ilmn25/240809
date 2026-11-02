using System.Collections.Generic;
using UnityEngine;

public class GuideMachine : PassiveNPCMachine, IActionSecondaryInteract
{
    public const int FlowerCost = 2;
    public const float CompanionDuration = 45f;

    /// <summary>Every meadow flower the Guide asks for, two of each.</summary>
    private static readonly ID[] FlowerTypes = { ID.Orchids, ID.Tulip, ID.Daisies };

    public static Info CreateInfo() => PassiveInfo.CreateNpc();

    public void OnActionSecondary(Info info)
    {
        BeginInteraction();

        ShowMenu((PlayerInfo)info);
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

    /// <summary>Offers to hire the Guide like the mercenary.</summary>
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

        options["Leave"] = new Dialogue { Text = "\"Stay safe out there.\"" };

        ShowDialogue(new Dialogue
        {
            Text = $"\"Need a hand? {FlowerCost} of each flower buys you a short escort.\"",
            Sprite = Cache.LoadSprite("Sprite/Guide"),
            Next = options,
        });
    }

    /// <summary>Pays the flowers and spawns a temporary Guide companion, like a hired mercenary.</summary>
    private void CompleteRecruit(PlayerInfo payer)
    {
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

    public void OnDrawGizmos()
    {
        if (Camera.current == Camera.main)
            GetModule<GroundPathingModule>().DrawGizmos();
    }
}
