using UnityEngine;
using Random = UnityEngine.Random;

public abstract class PlanterMachine : StructureMachine, IActionSecondaryInteract
{
    protected new PlanterInfo Info => GetModule<PlanterInfo>();

    private int _visualStage = -1;

    public override void OnStart()
    {
        base.OnStart();
        Environment.HourlyTriggered += OnHour;
        EnsureGrowthSchedule();
        TryGrowForTime(SaveData.Inst.time / 60, SaveData.Inst.day);
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
            ShowPlanterMessage($"I should come back in {GetHoursLeftToGrow()} hours");
            return;
        }

        if (!TryConsumeSelectedSeed(playerInfo))
        {
            ShowPlanterMessage("I can plant acorns in this");
            return;
        }

        Info.IsPlanted = true;
        Info.IsGrown = false;
        Info.GrowAtDay = SaveData.Inst.day + 1;
        Info.GrowAtHour = SaveData.Inst.time / 60;
        RefreshSprite(force: true);
    }

    private int GetHoursLeftToGrow()
    {
        int currentHour = SaveData.Inst.time / 60;
        int remainingHours = (Info.GrowAtDay - SaveData.Inst.day) * 24 + (Info.GrowAtHour - currentHour);
        return Mathf.Max(1, remainingHours);
    }

    private static void ShowPlanterMessage(string message)
    {
        Dialogue.Target = new Dialogue { Text = message };
        Dialogue.Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    protected virtual bool TryConsumeSelectedSeed(PlayerInfo actor)
    {
        if (actor.Storage.List == null || actor.Storage.List.Count == 0)
            return false;

        int key = Mathf.Clamp(actor.Storage.Key, 0, actor.Storage.List.Count - 1);
        ItemSlot selectedSlot = actor.Storage.List[key];

        if (selectedSlot.Stack <= 0 || selectedSlot.ID != Info.SeedID)
            return false;

        selectedSlot.Stack--;
        if (selectedSlot.Stack <= 0)
            selectedSlot.clear();

        actor.Storage.NotifyChanged();
        return true;
    }

    protected virtual void Harvest()
    {
        Vector3 offset = new Vector3(
            Random.value > 0.5f ? 0.65f : -0.65f,
            1.8f,
            Random.value > 0.5f ? 0.65f : -0.65f);

        Entity.SpawnItem(Info.HarvestID, transform.position + offset, amount: Info.HarvestAmount, stackOnSpawn: false);

        Info.IsPlanted = false;
        Info.IsGrown = false;
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
        if (!Info.IsPlanted || Info.IsGrown)
            return;

        if (Info.GrowAtDay > 0 && Info.GrowAtHour >= 0)
            return;

        Info.GrowAtDay = SaveData.Inst.day + 1;
        Info.GrowAtHour = SaveData.Inst.time / 60;
    }

    private void TryGrowForTime(int hour, int day)
    {
        if (!Info.IsPlanted || Info.IsGrown)
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
                SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.id + "2");
                break;
            case 1:
                SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.id + "1");
                break;
            default:
                SpriteRenderer.sprite = Cache.LoadSprite("Sprite/" + Info.id);
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
            SeedID = ID.Acorn,
            HarvestID = ID.Log,
            HarvestAmount = 1,
        };
    }
}
