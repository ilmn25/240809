public class RockStateMachine : EntityStateMachine
{ 
    private int _health;
    private string _item;
    protected override void OnAwake()
    {
        Initialize(ref _item, ref _health);
    
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
        _health = 2;
        SetState<Idle>();
    }

    private void OnMouseDown()
    {
        _health--;
        if (_health != 0) return;
        SetState<ResourceCollapse>();
    } 
}