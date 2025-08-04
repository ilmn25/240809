using UnityEngine;

public class NPCState : State
{
    private readonly ProjectileInfo _projectileInfo = 
        new SwingProjectileInfo(10, 10, 3, 1.1f, 1.5f);
    public override void OnEnterState()
    {
        string status = ((NPCCED)((EntityMachine)Machine).entityData).npcStatus;
        AddState(new NPCIdle(), status == "idle");
        AddState(new NPCChase(), status == "chase");
        AddState(new NPCRoam(),status == "roam");
        Dialogue dialogue = new Dialogue();
        dialogue.Lines.Add("when i was in primary school");
        dialogue.Lines.Add("i used to piss out the bathroom window off the building for fun");
        dialogue.Lines.Add("but one time my mom caught me because she saw the piss stream from the kitchen window");
        dialogue.Lines.Add("after that");
        dialogue.Lines.Add("i pissed out the window again, but i locked the bathroom door so she couldnt stop me");
        AddState(new CharTalk(dialogue));
    }
 
    
    public override void OnUpdateState()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            SetState<NPCChase>();
        else if (Input.GetKeyDown(KeyCode.T))
            SetState<NPCRoam>();
        else if (Input.GetKeyDown(KeyCode.U))
            Machine.transform.position = Game.Player.transform.position;

        if (Vector3.Distance(Machine.transform.position, Game.Player.transform.position) < 0.7f)
        {
            Projectile.Spawn(Machine.transform.position,Game.Player.transform.position,
                _projectileInfo, HitboxType.Friendly);
            // PlayerStatusModule.hit(10, 20, Machine.transform.position);
        }
    }
}