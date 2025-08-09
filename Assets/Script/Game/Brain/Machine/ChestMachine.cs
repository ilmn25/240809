public class ChestMachine : StructureMachine, IActionSecondary
{
    private Storage _storage;
    public override void OnStart()
    {
        _storage = new Storage(27);
        Loot.Gettable("slab").AddToContainer(_storage);
        AddModule(new ContainerInfo()
        {
            Health = 500,
            Loot = "tree",
            SfxHit = "dig_stone",
            SfxDestroy = "dig_stone",
            Storage = _storage
        });
    }

    public void OnActionSecondary()
    {
        Audio.PlaySFX("text", 0.5f);
        GUIMain.Storage.Storage = _storage.List;
        GUIMain.RefreshStorage();
    }
}