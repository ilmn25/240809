 
using System;
using Unity.VisualScripting;
using UnityEngine; 
 

public class TreeStateMachine : State
{ 
    public Transform _spriteObject;
    private int _health;
    private string _item;
    private int _currentHealth;
    public override void OnEnterState()
    { 
        _currentHealth = _health;
        _spriteObject = Root.transform.Find("sprite");
        AddState(new ResourceCollapse(_spriteObject, _item));
        AddState(new Idle(), true);
    }

    public TreeStateMachine(string item, int health)
    {
        _item = item;
        _health = health;
    }

    public void Hit()
    {
        AudioSingleton.PlaySFX(Game.DigSound);
        if (Game.GUIBusy) return;
        _currentHealth--;
        if (_currentHealth != 0) return;
        SetState<ResourceCollapse>();
        _currentHealth = _health;
    }  
}
 
class ResourceCollapse : State
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
            EntitySingleton.SpawnItem(_item, Vector3Int.FloorToInt(Root.transform.position)); 
            SetState<Idle>();
            ((EntityMachine)Root).WipeEntity();
        }
    }

    private Quaternion CreateRotation(float tilt)
    {
        return Quaternion.Euler(tilt, _spriteObject.rotation.eulerAngles.y, 0);
    }
} 
