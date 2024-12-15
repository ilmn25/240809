class NPCIdle : State {
    NPCMovementModule _npcMovementModule;
    NPCAnimationModule _npcAnimationModule;  
    public override void OnInitialize()
    {
        _npcMovementModule = Machine.GetModule<NPCMovementModule>();
        _npcAnimationModule = Machine.GetModule<NPCAnimationModule>();
    }

    public override void OnUpdateState() {
        _npcMovementModule.HandleMovementUpdate();
        _npcAnimationModule.HandleAnimationUpdate();
    }
}