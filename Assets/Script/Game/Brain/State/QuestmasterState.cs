using UnityEngine;

/// <summary>Shows the Questmaster's dialogue. On interact it reports the active
/// quest's objective; if the objective is already done, it hands in the quest,
/// drops the rewards, and the next interact offers a new task.</summary>
public class QuestmasterState : DialogueState
{
    public QuestmasterState() : base(null) { }

    public override void OnEnterState()
    {
        if (Main.GUIDialogue.activeSelf)
        {
            Info.CancelTarget();
            return;
        }

        Quest quest = Questmaster.EnsureQuest();
        bool complete = quest.IsComplete();
        if (complete)
            Questmaster.CompleteQuest(Machine.transform.position); // drops rewards, clears the quest

        Dialogue = complete
            ? CreateCompletionDialogue(quest)
            : CreateObjectiveDialogue(quest);

        base.OnEnterState();
    }

    public override void OnUpdateState()
    {
        if (Main.Player == null)
            Machine.SetState<DefaultState>();
        else
            base.OnUpdateState();
    }

    private static Dialogue CreateObjectiveDialogue(Quest quest)
    {
        return new Dialogue
        {
            Text = "Another task, then. " + quest.Describe(),
            Sprite = Cache.LoadSprite("Sprite/Questmaster"),
        };
    }

    private static Dialogue CreateCompletionDialogue(Quest quest)
    {
        return new Dialogue
        {
            Text = "Done already? " + quest.CompleteText,
            Sprite = Cache.LoadSprite("Sprite/Questmaster"),
        };
    }
}
