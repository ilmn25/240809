 
using System;
using Unity.VisualScripting;
using UnityEngine; 
 

public class TreeState : State
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

    public TreeState(string item, int health)
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