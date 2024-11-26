 
using Unity.VisualScripting;
using UnityEngine;

public class TreeStateMachine : EntityStateMachine
{ 
    public Transform _spriteObject;
    private int _health;
    private string _item;
    private int _currentHealth;
    protected override void OnAwake()
    {
        Initialize(ref _item, ref _health);
        _currentHealth = _health;
        _spriteObject = transform.Find("sprite");
        AddState(new ResourceCollapse(_spriteObject, _item));
        AddState(new Idle(), true);
    }

    protected virtual void Initialize(ref string item, ref int health ) {}
    
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
 
class ResourceCollapse : EntityState
{
    private Transform _spriteObject;
    private float _rotationProgress = 0;
    private string _item;
    public ResourceCollapse(Transform spriteObject, string item)
    {
        _item = item;
        _spriteObject = spriteObject;
    }

    public override void StateUpdate() 
    { 
        _rotationProgress += Time.deltaTime * 0.8f;
        _spriteObject.rotation = Quaternion.Lerp(CreateRotation(0), CreateRotation(90), _rotationProgress);
        if (_spriteObject.rotation.eulerAngles.x > 89) 
        {  
            _rotationProgress = 0;
            Entity.SpawnItem(_item, StateMachine.transform.position + new Vector3(0, 0.2f, 0));
            StateMachine.WipeEntity();
        }
    }

    private Quaternion CreateRotation(float tilt)
    {
        return Quaternion.Euler(tilt, _spriteObject.rotation.eulerAngles.y, 0);
    }
} 
