using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class PlanterMachine : StructureMachine, IActionSecondaryInteract
{
    protected new PlanterInfo Info => GetModule<PlanterInfo>();

    private readonly struct Crop
    {
        public readonly ID Harvest;
        public readonly int Min;
        public readonly int Max;

        public Crop(ID harvest, int min, int max)
        {
            Harvest = harvest;
            Min = min;
            Max = max;
        }
    }

    private static readonly Dictionary<ID, Crop> Crops = new()
    {
        { ID.Acorn, new Crop(ID.Log, 1, 1) },
        { ID.CornSeed, new Crop(ID.Corn, 2, 4) },
        { ID.PumpkinSeed, new Crop(ID.Pumpkin, 1, 1) },
    };

    private int _visualStage = -1;
    private readonly Dialogue _messageDialogue = new();

    public override void OnStart()
    {
        base.OnStart();
        AddState(new MessageState(_messageDialogue));
        Environment.HourlyTriggered += OnHour;
        EnsureGrowthSchedule();
        TryGrowForTime(Save.Inst.time / 60, Save.Inst.day);
        RefreshSprite(force: true);
    }

    private void OnDestroy()
    {
        Environment.HourlyTriggered -= OnHour;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        RefreshSprite();
    }

    public void OnActionSecondary(Info info)
    {
        if (info is not PlayerInfo playerInfo || playerInfo.Storage == null)
            return;

        if (Info.IsGrown)
        {
            Harvest();
            return;
        }

        if (Info.IsPlanted)
        {
            if (!Info.IsWatered)
                ShowPlanterMessage("This needs water to grow");
            else
                ShowPlanterMessage($"I should come back in {GetHoursLeftToGrow()} hours");
            return;
        }

        if (!TryConsumeSelectedSeed(playerInfo))
        {
            ShowPlanterMessage("I can plant a seed in this");
            return;
        }

        Info.IsPlanted = true;
        Info.IsGrown = false;
        Info.IsWatered = false;
        Info.GrowAtDay = -1;
        Info.GrowAtHour = -1;
        RefreshSprite(force: true);
    }

    /// <summary>Whether this planter can currently be watered (planted, not grown, dry).</summary>
    public bool CanWater()
    {
        return Info.IsPlanted && !Info.IsGrown && !Info.IsWatered;
    }

    /// <summary>Water this planter, scheduling growth one day out. True if it was watered.</summary>
    public bool Water()
    {
        if (!CanWater()) return false;
        Info.IsWatered = true;
        Info.GrowAtDay = Save.Inst.day + 1;
        Info.GrowAtHour = Save.Inst.time / 60;
        RefreshSprite(force: true);
        return true;
    }

    private int GetHoursLeftToGrow()
    {
        int currentHour = Save.Inst.time / 60;
        int remainingHours = (Info.GrowAtDay - Save.Inst.day) * 24 + (Info.GrowAtHour - currentHour);
        return Mathf.Max(1, remainingHours);
    }

    private void ShowPlanterMessage(string message)
    {
        _messageDialogue.Text = message;
        if (IsCurrentState<DefaultState>())
            SetState<MessageState>();
        else
            SetState<DefaultState>();
    }

    protected virtual bool TryConsumeSelectedSeed(PlayerInfo actor)
    {
        if (actor.Storage.List == null || actor.Storage.List.Count == 0)
            return false;

        ItemSlot selectedSlot = actor.Storage.GetSelected();

        if (selectedSlot.Stack <= 0 || !Crops.TryGetValue(selectedSlot.ID, out Crop crop))
            return false;

        selectedSlot.Stack--;
        if (selectedSlot.Stack <= 0)
            selectedSlot.clear();

        Info.HarvestID = crop.Harvest;
        Info.HarvestMin = crop.Min;
        Info.HarvestMax = crop.Max;

        actor.Storage.NotifyChanged();
        return true;
    }

    protected virtual void Harvest()
    {
        Vector3 offset = new Vector3(
            Random.value > 0.5f ? 0.65f : -0.65f,
            1.8f,
            Random.value > 0.5f ? 0.65f : -0.65f);

        int amount = Random.Range(Info.HarvestMin, Info.HarvestMax + 1);
        Entity.SpawnItem(Info.HarvestID, transform.position + offset, amount: amount, stackOnSpawn: false);

        Info.IsPlanted = false;
        Info.IsGrown = false;
        Info.IsWatered = false;
        Info.GrowAtDay = -1;
        Info.GrowAtHour = -1;
        RefreshSprite(force: true);
    }

    private void OnHour(int hour, int day)
    {
        TryGrowForTime(hour, day);
    }

    private void EnsureGrowthSchedule()
    {
        if (!Info.IsPlanted || Info.IsGrown || !Info.IsWatered)
            return;

        if (Info.GrowAtDay > 0 && Info.GrowAtHour >= 0)
            return;

        Info.GrowAtDay = Save.Inst.day + 1;
        Info.GrowAtHour = Save.Inst.time / 60;
    }

    private void TryGrowForTime(int hour, int day)
    {
        if (!Info.IsPlanted || Info.IsGrown || !Info.IsWatered)
            return;

        if (Info.GrowAtDay <= 0 || Info.GrowAtHour < 0)
            return;

        if (day < Info.GrowAtDay)
            return;

        if (day == Info.GrowAtDay && hour < Info.GrowAtHour)
            return;

        Info.IsGrown = true;
        RefreshSprite(force: true);
    }

    private void RefreshSprite(bool force = false)
    {
        int stage = GetStage();
        if (!force && stage == _visualStage)
            return;

        _visualStage = stage;
        switch (stage)
        {
            case 2:
                SetAttachment(Cache.LoadSprite("Sprite/" + Info.HarvestID));
                break;
            case 1:
                SetAttachment(Cache.LoadSprite("Sprite/Seedling"));
                break;
            default:
                SetAttachment(null, false);
                break;
        }
    }

    private int GetStage()
    {
        if (Info.IsGrown)
            return 2;
        if (Info.IsPlanted)
            return 1;
        return 0;
    }
}

public class ImprovisedPlanterMachine : PlanterMachine
{
    public static Info CreateInfo()
    {
        return new PlanterInfo()
        {
            Health = 250,
            Loot = ID.ImprovisedPlanter,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Cutting,
            threshold = 1,
        };
    }
}
