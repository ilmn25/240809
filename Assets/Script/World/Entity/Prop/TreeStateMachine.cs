 
using UnityEngine;

public class TreeStateMachine : EntityStateMachine
{ 
    public Transform _spriteObject;
    
    protected override void OnAwake()
    {
        // gameObject.AddComponent<EntityClickInst>();
        _spriteObject = transform.Find("sprite");
        AddState(new TreeBreak(_spriteObject));
        AddState(new Idle(), true);
    }

    public void OnEnable()
    { 
        SetState<Idle>();
    }

    private void OnMouseDown()
    {
        SetState<TreeBreak>();
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

    public override void StateUpdate() 
    { 
        _rotationProgress += Time.deltaTime * 0.4f;
        _spriteObject.rotation = Quaternion.Lerp(CreateRotation(0), CreateRotation(90), _rotationProgress);
        if (_spriteObject.rotation.eulerAngles.x > 89) 
        {  
            _rotationProgress = 0;
            ItemLoadStatic.Instance.SpawnItem("brick", StateMachine.transform.position);
            StateMachine.WipeEntity();
        }
    }

    private Quaternion CreateRotation(float tilt)
    {
        return Quaternion.Euler(tilt, _spriteObject.rotation.eulerAngles.y, 0);
    }

}

class Idle : EntityState
{
}
