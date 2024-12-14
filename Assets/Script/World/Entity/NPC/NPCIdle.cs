class NPCIdle : State {
    NPCMovementModule _npcMovementModule;
    NPCAnimationModule _npcAnimationModule; 

    public NPCIdle(NPCMovementModule npcMovementModule, NPCAnimationModule npcAnimationModule)
    {
        _npcMovementModule = npcMovementModule;
        _npcAnimationModule = npcAnimationModule;
    }
 
    public override void StateUpdate() {
        _npcMovementModule.HandleMovementUpdate();
        _npcAnimationModule.HandleAnimationUpdate();
    }
}