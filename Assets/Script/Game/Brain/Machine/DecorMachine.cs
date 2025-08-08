using UnityEngine;

public class DecorMachine : EntityMachine
{ 
    public override void OnStart()
    {
        AddState(new StaticIdle(),true);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }
    //
    // private void OnMouseDown()
    // {
    //     WipeEntity();
    // } 
}