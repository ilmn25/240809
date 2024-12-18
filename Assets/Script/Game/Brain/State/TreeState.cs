 
using System;
using Unity.VisualScripting;
using UnityEngine; 
 

public class TreeState : State
{ 
    public Transform _spriteObject;
    private int _health;
    private string _item;
    private int _currentHealth;
    public override void OnInitialize()
    {
        ((OakMachine)Machine).Hit += Hit;
    }
    public override void OnExitState()
    {
        ((OakMachine)Machine).Hit -= Hit;
    }


    public override void OnEnterState()
    { 
        _currentHealth = _health;
        _spriteObject = Machine.transform.Find("sprite");
        AddState(new ResourceCollapse(_spriteObject, _item));
        AddState(new StaticIdle(), true);
    }

    public TreeState(string item, int health)
    {
        _item = item;
        _health = health;
    }

    public void Hit()
    {
        Audio.PlaySFX(Game.DigSound);
        if (GUI.Active) return;
        _currentHealth--;
        if (_currentHealth != 0) return;
        SetState<ResourceCollapse>();
        _currentHealth = _health;
    }  
}