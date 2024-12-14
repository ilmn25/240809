class NPCIdle : State {
    NPCMovementModule _npcMovementModule;
    NPCAnimationModule _npcAnimationModule; 

    public NPCIdle(NPCMovementModule npcMovementModule, NPCAnimationModule npcAnimationModule)
    {
        _npcMovementModule = npcMovementModule;
        _npcAnimationModule = npcAnimationModule;
    }
 
    public override void OnUpdateState() {
        _npcMovementModule.HandleMovementUpdate();
        _npcAnimationModule.HandleAnimationUpdate();
    }
}