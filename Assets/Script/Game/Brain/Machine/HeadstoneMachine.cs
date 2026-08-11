using UnityEngine;

/// <summary>A grave marker found in graveyard clusters. Right-clicking it shows a
/// random epitaph; it can be mined away like other stone structures.</summary>
public class HeadstoneMachine : StructureMachine, IActionSecondaryInteract
{
    private static readonly string[] Epitaphs =
    {
        "Here lies Xx_Slayer420_xX\nGone but not forgotten",
        "Here lies Steve\nHe dug too deep",
        "Here lies the last person\nwho looked at this",
        "Here lies a brave warrior\nDied to a sheep",
        "Here lies Grog\nDied doing what he loved",
        "Here lies Bob\nWe hardly knew ye",
        "Here lies an adventurer\nNever reached the end",
        "Rest in peace\nA true legend",
        "Here lies a farmer\nReaped what he sowed",
        "Here lies a miner\nStruck a little too hard",
    };

    private MessageState _messageState;

    public static Info CreateInfo()
    {
        return new SpriteStructureInfo()
        {
            Health = 20,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            operationType = OperationType.Mining,
        };
    }

    public override void OnStart()
    {
        base.OnStart();
        _messageState = new MessageState(new Dialogue { Text = Epitaphs[Random.Range(0, Epitaphs.Length)] });
        AddState(_messageState);
    }

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<MessageState>();
    }
}
