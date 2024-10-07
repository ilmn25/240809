using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EntityStateHandler : MonoBehaviour
{
    // public enum EntityState { Roam, Hone, Chase, Freeze }
    public SpriteRenderer _sprite;
    private int _entityState = 1;
    // private EntityState _entityStatePrevious = 0;
    private EntityMovementHandler _entityMovementHandler;
    private EntityPathFindHandler _entityPathFindHandler;
    private EntityAnimationHandler _entityAnimationHandler; 

    // Start is called before the first frame update
    void Start()
    {
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _entityMovementHandler = GetComponent<EntityMovementHandler>();
        _entityPathFindHandler = GetComponent<EntityPathFindHandler>();
        _entityAnimationHandler = GetComponent<EntityAnimationHandler>();
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
                    _entityMovementHandler.HandleMovementUpdate();
                    _entityAnimationHandler.HandleAnimationUpdate();
                }
                break;
            case 2: 
                if (_sprite.isVisible)
                {
                    _entityPathFindHandler.HandlePathFindActive();
                    // _entityMovementHandler.HandleMovementUpdateTest();
                    _entityMovementHandler.HandleMovementUpdate();
                    _entityAnimationHandler.HandleAnimationUpdate();
                }
                else
                {  
                    _entityPathFindHandler.HandlePathFindPassive(); 
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
