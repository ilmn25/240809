using UnityEngine;
 
public abstract class StateMachine : MonoBehaviour
{  
    protected State State;
    public virtual void OnAwake() {}
    public virtual void OnUpdate() {}
 
    private void Awake()
    {
        OnAwake();
        State.Root = this; 
        State.OnEnterState();
    }
    
    private void Update()
    {
        OnUpdate(); 
        State.OnUpdateInternal(); 
    }

    public void Terminate()
    {
        State.OnExitState();
    }
}