using UnityEngine;
class NPCIdle : EntityState {
    MovementModule _movementModule;
    AnimationModule _animationModule; 

    public NPCIdle(MovementModule movementModule, AnimationModule animationModule)
    {
        _movementModule = movementModule;
        _animationModule = animationModule;
    }
 
    public override void StateUpdate() {
        _movementModule.HandleMovementUpdate();
        _animationModule.HandleAnimationUpdate();
    }
}

class NPCRoam : EntityState {
    MovementModule _movementModule;
    PathFindModule _pathFindModule;
    AnimationModule _animationModule; 
    SpriteRenderer _sprite;

    public NPCRoam(MovementModule movementModule, PathFindModule pathFindModule, 
        AnimationModule animationModule, SpriteRenderer sprite)
    {
        _movementModule = movementModule;
        _animationModule = animationModule;
        _pathFindModule = pathFindModule;
        _sprite = sprite; 
    }

    public override void OnEnterState()
    {
        _pathFindModule.SetTarget(null);
    }
    
    public override void StateUpdate() {
        if (_sprite.isVisible)
        {
            _movementModule.SetDirection(_pathFindModule.HandlePathFindRandom(_movementModule.IsGrounded()));
            _movementModule.HandleMovementUpdate(true);
            _animationModule.HandleAnimationUpdate();
        }
        else
        {  
            _pathFindModule.HandlePathFindPassive(_movementModule.SPEED_WALK); 
        }
    }
}

class NPCChase : EntityState {
    MovementModule _movementModule;
    PathFindModule _pathFindModule;
    AnimationModule _animationModule; 
    SpriteRenderer _sprite;

    public NPCChase(MovementModule movementModule, PathFindModule pathFindModule, 
        AnimationModule animationModule, SpriteRenderer sprite)
    {
        _movementModule = movementModule;
        _animationModule = animationModule;
        _pathFindModule = pathFindModule;
        _sprite = sprite; 
    }

    public override void OnEnterState()
    {
        _pathFindModule.SetTarget(Game.Player.transform);
    }
    
    public override void StateUpdate() {
        if (_sprite.isVisible)
        {
            _movementModule.SetDirection(_pathFindModule.HandlePathFindActive(_movementModule.IsGrounded()));
            _movementModule.HandleMovementUpdate();
            _animationModule.HandleAnimationUpdate();
        }
        else
        {  
            _pathFindModule.HandlePathFindPassive(_movementModule.SPEED_WALK); 
        }
    }
}