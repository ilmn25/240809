public class RockMachine : EntityMachine, ILeftClick
{
    public override void OnAwake()
    {
        State = new RockStateMachine();
    }

    public void OnLeftClick()
    {
        ((RockStateMachine)State).Hit();
    } 
}
public class RockStateMachine : State 
{ 
    private int _health;
    private string _item;
    private int _currentHealth;
    public override void OnEnterState()
    {
        _item = "stone";
        _health = 2;
        _currentHealth = _health;
        AddState(new ResourceCollapse(Root.transform, _item));
        AddState(new Idle(), true); 
    }
 
 

    public void Hit()
    {
        AudioSingleton.PlaySFX(Game.DigSound);
        _currentHealth--;
        if (_currentHealth != 0) return;
        SetState<ResourceCollapse>(); 
    } 
}