using System;

/// <summary>A dead player's body. Holds the deceased player's inventory for looting.</summary>
[Serializable]
public class CorpseInfo : MobInfo
{
    public Storage Storage;

    public override void Initialize()
    {
        base.Initialize();
        if (Storage == null) Storage = new Storage(9);
        Storage.info = this;
    }
}
