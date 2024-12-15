using UnityEngine;

public class OakMachine : EntityMachine, ILeftClick
{
    public override void OnInitialize()
    {
        State = new TreeState("wood", 5);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        AddModule(new SpriteCullModule(spriteRenderer)); 
        AddModule(new SpriteOrbitModule(spriteRenderer)); 
    }

    public void OnLeftClick()
    {
        ((TreeState)State).Hit();
    }
}