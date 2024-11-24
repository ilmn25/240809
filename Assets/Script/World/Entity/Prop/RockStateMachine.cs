public class RockStateMachine : EntityStateMachine
{ 
    private int _health;
    private string _item;
    private int _currentHealth;
    protected override void OnAwake()
    {
        Initialize(ref _item, ref _health);
        _currentHealth = _health;
        AddState(new ResourceCollapse(this.transform, _item));
        AddState(new Idle(), true);
    }

    protected virtual void Initialize(ref string item, ref int health)
    {
        item = "stone";
        health = 2;
    }

    public void OnEnable()
    { 
        _currentHealth = _health;
        SetState<Idle>();
    }

    private void OnMouseDown()
    {
        _currentHealth--;
        if (_currentHealth != 0) return;
        SetState<ResourceCollapse>();
    } 
}