using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType { None, Kill, Collect }

/// <summary>Base class for a quest handed out by the Questmaster. Subclasses define
/// what counts as progress (kills, collected items, etc.) and how it is verified.</summary>
public abstract class Quest
{
    /// <summary>Describes the task, shown while the quest is active.</summary>
    public string Objective;
    /// <summary>Spoken when the player hands the finished quest in.</summary>
    public string CompleteText;
    /// <summary>Items dropped on the ground when the quest is completed.</summary>
    public Dictionary<ID, int> Rewards;

    public abstract QuestType Type { get; }

    protected Quest(Dictionary<ID, int> rewards)
    {
        Rewards = rewards ?? new Dictionary<ID, int>();
    }

    public abstract bool IsComplete();

    /// <summary>Writes this quest's data into the serializable snapshot.</summary>
    public virtual void CaptureState(QuestState state)
    {
        state.type = Type;
        state.rewards = Rewards;
    }

    /// <summary>Called when the quest becomes the active one (subscribe to events).</summary>
    public virtual void OnBecameActive() { }

    /// <summary>Stops tracking without consuming requirements — used when a quest
    /// is abandoned (world/session teardown) so kill quests unsubscribe.</summary>
    public virtual void OnDeactivate() { }

    /// <summary>Called on hand-in (unsubscribe, consume collected items).</summary>
    public virtual void OnComplete() { }

    /// <summary>Objective text with live progress, shown in the dialogue.</summary>
    public virtual string Describe() => Objective;
}

/// <summary>A quest to defeat a set number of a specific enemy.</summary>
public class KillQuest : Quest
{
    public readonly ID TargetMob;
    public readonly int Required;
    public int Kills { get; private set; }

    public KillQuest(ID targetMob, int required, Dictionary<ID, int> rewards, string completeText = null)
        : base(rewards)
    {
        TargetMob = targetMob;
        Required = required;
        Objective = $"Hunt the {Helper.ToDisplayName(targetMob)}. Defeat {required} of them.";
        CompleteText = completeText ?? $"The {Helper.ToDisplayName(targetMob)} won't bother us again.";
    }

    public override QuestType Type => QuestType.Kill;

    public override void OnBecameActive() => EnemyInfo.Killed += OnKilled;

    private void OnKilled(ID id)
    {
        if (id == TargetMob) Kills++;
    }

    public override bool IsComplete() => Kills >= Required;

    public override void OnComplete() => OnDeactivate();

    public override void OnDeactivate() => EnemyInfo.Killed -= OnKilled;

    public override string Describe() => $"{Objective} ({Kills}/{Required})";

    public override void CaptureState(QuestState state)
    {
        base.CaptureState(state);
        state.target = TargetMob;
        state.required = Required;
        state.progress = Kills;
    }

    public void SetProgress(int kills) => Kills = kills;
}

/// <summary>A quest to hand in a set number of a specific item.</summary>
public class CollectQuest : Quest
{
    public readonly ID TargetItem;
    public readonly int Required;

    public CollectQuest(ID targetItem, int required, Dictionary<ID, int> rewards, string completeText = null)
        : base(rewards)
    {
        TargetItem = targetItem;
        Required = required;
        Objective = $"Bring me {required} {Helper.ToDisplayName(targetItem)}.";
        CompleteText = completeText ?? "A fair trade. Here's your payment.";
    }

    public override QuestType Type => QuestType.Collect;

    public override bool IsComplete()
    {
        return Main.PlayerInfo != null && Main.PlayerInfo.Storage.GetAmount(TargetItem) >= Required;
    }

    public override void OnComplete()
    {
        if (Main.PlayerInfo != null)
            Main.PlayerInfo.Storage.RemoveItem(TargetItem, Required);
    }

    public override string Describe()
    {
        int have = Main.PlayerInfo != null ? Main.PlayerInfo.Storage.GetAmount(TargetItem) : 0;
        return $"{Objective} ({have}/{Required})";
    }

    public override void CaptureState(QuestState state)
    {
        base.CaptureState(state);
        state.target = TargetItem;
        state.required = Required;
        state.progress = 0;
    }
}

/// <summary>Serializable snapshot of the active quest, stored on Save so progress
/// survives save/load. NPCs aren't saved, so the Questmaster rebuilds its quest
/// from this after a reload.</summary>
[Serializable]
public class QuestState
{
    public QuestType type;
    public ID target;
    public int required;
    public int progress;
    public int completed;
    public Dictionary<ID, int> rewards = new();
}

/// <summary>Owns the single active quest and issues new ones. Completing a quest
/// drops its rewards and hands out the next task, escalating difficulty over time.</summary>
public static class Questmaster
{
    public static Quest Current { get; private set; }

    private static int _completed;

    public static Quest EnsureQuest()
    {
        if (Current == null)
            Current = LoadOrCreate();
        return Current;
    }

    /// <summary>Completes the active quest: consumes its requirements, drops the
    /// rewards at the given position, and clears it so the next interaction issues
    /// a fresh quest.</summary>
    public static void CompleteQuest(Vector3 dropPos)
    {
        if (Current == null) return;

        Quest done = Current;
        done.OnComplete();
        DropRewards(done.Rewards, dropPos);
        _completed++;
        Current = null;
        WriteState(null); // clear the active quest in the save
    }

    /// <summary>Stops the active quest from listening and persists its progress —
    /// called on world/session teardown so the save captures it and kill quests
    /// unsubscribe. The quest is restored from the save on the next load.</summary>
    public static void Reset()
    {
        if (Current == null) return;

        Current.OnDeactivate();
        WriteState(Current);
        Current = null;
    }

    private static Quest LoadOrCreate()
    {
        QuestState state = Save.Inst != null ? Save.Inst.quest : null;
        if (state != null && state.type != QuestType.None)
        {
            _completed = state.completed;
            Quest restored = RestoreQuest(state);
            if (restored != null)
            {
                restored.OnBecameActive();
                return restored;
            }
        }
        return CreateQuest();
    }

    private static Quest RestoreQuest(QuestState state)
    {
        switch (state.type)
        {
            case QuestType.Kill:
                var kill = new KillQuest(state.target, state.required, state.rewards);
                kill.SetProgress(state.progress);
                return kill;
            case QuestType.Collect:
                return new CollectQuest(state.target, state.required, state.rewards);
            default:
                return null;
        }
    }

    private static void WriteState(Quest quest)
    {
        if (Save.Inst == null || Save.Inst.quest == null) return;

        if (quest == null)
        {
            Save.Inst.quest.type = QuestType.None;
            Save.Inst.quest.completed = _completed;
            return;
        }
        quest.CaptureState(Save.Inst.quest);
        Save.Inst.quest.completed = _completed;
    }

    private static void DropRewards(Dictionary<ID, int> rewards, Vector3 pos)
    {
        foreach (var kv in rewards)
            Entity.SpawnItem(kv.Key, pos, amount: kv.Value, stackOnSpawn: false);
    }

    private static int Tier => _completed + (Save.Inst != null ? Save.Inst.day / 4 : 0);

    private static Quest CreateQuest()
    {
        // Alternate between kill and collect tasks, scaling with how far along the player is.
        Quest q = Tier % 2 == 0 ? CreateKillQuest() : CreateCollectQuest();
        q.OnBecameActive();
        WriteState(q); // persist the freshly issued quest
        return q;
    }

    private static readonly ID[] KillPool = { ID.SnareFlea, ID.Slime, ID.Harpy, ID.Raider, ID.Lich };

    private static Quest CreateKillQuest()
    {
        ID mob = KillPool[Mathf.Clamp(Tier / 2, 0, KillPool.Length - 1)];
        int required = 2 + Tier;
        return new KillQuest(mob, required, new Dictionary<ID, int>
        {
            { ID.Steel, 2 + Tier },
            { ID.Charcoal, 3 },
        });
    }

    private static readonly ID[] CollectPool = { ID.Gravel, ID.Log, ID.StoneBlock, ID.Plank, ID.Steel };

    private static Quest CreateCollectQuest()
    {
        ID item = CollectPool[Mathf.Clamp(Tier / 2, 0, CollectPool.Length - 1)];
        int required = 5 + Tier * 3;
        return new CollectQuest(item, required, new Dictionary<ID, int>
        {
            { ID.Copper, 2 + Tier },
        });
    }
}
