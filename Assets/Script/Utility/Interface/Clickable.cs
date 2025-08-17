
public interface IAction { } 
public interface IActionPrimary : IAction { }
public interface IActionPrimaryAttack : IActionPrimary { }
public interface IActionPrimaryResource : IActionPrimary { } 

public interface IActionSecondary : IAction { }

public interface IActionSecondaryPickUp : IActionSecondary { }

public interface IActionSecondaryInteract : IActionSecondary
{
    void OnActionSecondary(Info info);  
}

public interface IHover : IAction
{
    void OnHover();
} 
 
public interface IInfoProvider
{ 
}
