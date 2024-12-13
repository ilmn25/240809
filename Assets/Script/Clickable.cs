public interface LeftClickable
{ 
    void OnLeftClick(); 
}

public interface Hoverable
{
    void OnHover();
}
public interface RightClickable
{
    void OnRightClick();  
}
//
// public interface Clickable : LeftClickable, RightClickable { }