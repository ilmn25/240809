public class RockState : State 
{ 
    private int _health;
    private string _item;
    private int _currentHealth;
    
    public override void OnInitialize()
    {
        ((RockMachine)Machine).Hit += Hit;
    }
    public override void OnExitState()
    {
        ((RockMachine)Machine).Hit -= Hit;
    }

    public override void OnEnterState()
    {
        _item = "stone";
        _health = 2;
        _currentHealth = _health;
        AddState(new ResourceCollapse(Machine.transform, _item));
        AddState(new StaticIdle(), true); 
    }
 
    public void Hit()
    {
        Audio.PlaySFX("dig_metal");
        _currentHealth--;
        if (_currentHealth != 0) return;
        SetState<ResourceCollapse>(); 
    } 
}