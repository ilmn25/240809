
public interface IAction { }
public interface IActionPrimary : IAction
{ 
    void OnActionPrimary(EntityMachine entityMachine); 
}
public interface IActionSecondary : IAction
{
    void OnActionSecondary(EntityMachine entityMachine);  
}

public interface IHover : IAction
{
    void OnHover();
} 

public interface IHitBox : IAction { }
public interface IHitBoxAttack : IHitBox { }
public interface IHitBoxResource : IHitBox { }