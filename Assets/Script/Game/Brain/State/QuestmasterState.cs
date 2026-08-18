using UnityEngine;

/// <summary>Shows the Questmaster's dialogue. On interact it reports the active
/// quest's objective; if the objective is already done, it hands in the quest,
/// drops the rewards, and the next interact offers a new task.</summary>
public class QuestmasterState : MobState
{
    private Dialogue _dialogue;

    public override void OnEnterState()
    {
        if (Main.GUIDialogue.activeSelf)
        {
            Info.CancelTarget();
            return;
        }

        Audio.PlaySFX(SfxID.Notification);
        Info.PathingStatus = PathingStatus.Reached;
        Info.Direction = Vector3.zero;

        Quest quest = Questmaster.EnsureQuest();
        bool complete = quest.IsComplete();
        if (complete)
            Questmaster.CompleteQuest(Machine.transform.position); // drops rewards, clears the quest

        _dialogue = complete
            ? CreateCompletionDialogue(quest)
            : CreateObjectiveDialogue(quest);

        Dialogue.Target = _dialogue;
        Dialogue.Show(true);
    }

    public override void OnUpdateState()
    {
        if (Main.Player == null || !Dialogue.Showing ||
            Helper.SquaredDistance(Main.Player.transform.position, Machine.transform.position) > 5 * 5)
            Machine.SetState<DefaultState>();
    }

    public override void OnExitState()
    {
        Dialogue.Show(false);
        _dialogue = null;
    }

    private static Dialogue CreateObjectiveDialogue(Quest quest)
    {
        return new Dialogue
        {
            Text = "Another task, then. " + quest.Describe(),
            Sprite = Cache.LoadSprite("Sprite/Guide"),
        };
    }

    private static Dialogue CreateCompletionDialogue(Quest quest)
    {
        return new Dialogue
        {
            Text = "Done already? " + quest.CompleteText,
            Sprite = Cache.LoadSprite("Sprite/Guide"),
        };
    }
}
