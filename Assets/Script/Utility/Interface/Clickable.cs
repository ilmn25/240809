
public interface IAction { }
public interface IActionPrimary : IAction
{ 
    void OnActionPrimary(); 
}
public interface IActionSecondary : IAction
{
    void OnActionSecondary();  
}

public interface IHover : IAction
{
    void OnHover();
} 

public interface IHitBox : IAction
{
    void OnHit();  
}