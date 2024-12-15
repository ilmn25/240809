using UnityEngine;

public class RockMachine : EntityMachine, ILeftClick
{
    public override void OnInitialize()
    {
        State = new RockState();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public void OnLeftClick()
    {
        ((RockState)State).Hit();
    } 
}