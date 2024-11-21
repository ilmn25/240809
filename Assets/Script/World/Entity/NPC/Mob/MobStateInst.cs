using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MobStateInst : MonoBehaviour
{
    // public enum EntityState { Roam, Hone, Chase, Freeze }
    public SpriteRenderer _sprite;
    private int _entityState = 1;
    // private EntityState _entityStatePrevious = 0;
    private NPCMovementInst _npcMovementInst;
    private NPCPathFindInst _npcPathFindInst;
    private NPCAnimationInst _npcAnimationInst; 

    // Start is called before the first frame update
    void Start()
    {
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _npcPathFindInst = GetComponent<NPCPathFindInst>();
        _npcAnimationInst = GetComponent<NPCAnimationInst>();
    }
 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _entityState = 2;
        }
        
        // HandleStateChange();
        // CustomLibrary.Log(_sprite.isVisible);
        switch (_entityState)
        {
            case 1: 
                if(_sprite.isVisible)
                {
                    _npcMovementInst.HandleMovementUpdate();
                    _npcAnimationInst.HandleAnimationUpdate();
                }
                break;
            case 2: 
                if (_sprite.isVisible)
                {
                    _npcPathFindInst.HandlePathFindActive();
                    // _entityMovementHandler.HandleMovementUpdateTest();
                    _npcMovementInst.HandleMovementUpdate();
                    _npcAnimationInst.HandleAnimationUpdate();
                }
                else
                {  
                    _npcPathFindInst.HandlePathFindPassive(); 
                }
                break; 
        } 
    }
 
    // void HandleStateChange()
    // {
    //     if (_entityState != _entityStatePrevious)
    //     {
    //         switch (_entityState)
    //         {  
    //             case 2:
    //                 break;
    //             default:
    //                 break;
    //         }
    //         _entityStatePrevious = _entityState;
    //     }
    // }
}
