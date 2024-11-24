 
using UnityEngine;

public class TreeStateMachine : EntityStateMachine
{ 
    public Transform _spriteObject;
    
    protected override void OnAwake()
    {
        _spriteObject = transform.Find("sprite");
        AddState(new TreeBreak(_spriteObject));
        AddState(new PropIdle(), true);
    }

    public override void OnEnable()
    { 
        SetState<PropIdle>();
    }

    protected override void LogicUpdate()
    { 
        if (Input.GetKeyDown(KeyCode.G)) SetState<TreeBreak>();
    }
}
 
class TreeBreak : EntityState
{
    private Transform _spriteObject;
    private float _rotationProgress = 0;
    public TreeBreak(Transform spriteObject)
    {
        _spriteObject = spriteObject;
    }

    public override void OnEnterState(){}

    public override void StateUpdate() 
    { 
        _rotationProgress += Time.deltaTime * 0.4f;
        _spriteObject.rotation = Quaternion.Lerp(CreateRotation(0), CreateRotation(90), _rotationProgress);
        if (_spriteObject.rotation.eulerAngles.x > 89) 
        {  
            _rotationProgress = 0;
            _spriteObject.parent.GetComponent<EntityHandler>().WipeEntity();
        }
    }

    private Quaternion CreateRotation(float tilt)
    {
        return Quaternion.Euler(tilt, _spriteObject.rotation.eulerAngles.y, 0);
    }

    public override void OnExitState() {}
}

class PropIdle : EntityState
{
    public override void OnEnterState() { }

    public override void StateUpdate() { }

    public override void OnExitState() { }
}
