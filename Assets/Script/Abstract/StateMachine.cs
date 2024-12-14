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
    
    public void Update()
    {
        OnUpdate(); 
        State.OnUpdate(); 
    }

    public void Terminate()
    {
        State.OnExitState();
    }
}