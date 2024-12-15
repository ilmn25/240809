using UnityEngine;

public class DecorMachine : EntityMachine
{ 
    public override void OnInitialize()
    {
        State = new StaticIdle();
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