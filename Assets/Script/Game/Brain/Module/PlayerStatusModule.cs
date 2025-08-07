using UnityEngine;

public enum PlayerStatus {
    Active, 
    Dead,
    Freeze
}
public class PlayerStatusModule : StatusModule
{ 
    public float Mana;
    public float Sanity;
    public float Hunger;
    public float Stamina; 
    public bool IsBusy = false;
    public bool Invincibility = false;
    public PlayerStatus PlayerStatus = PlayerStatus.Freeze;
    

    public override void Initialize()
    { 
        base.Initialize();
        Iframes = 45;
    }

    protected override void OnUpdate()
    {
        TargetScreenDir = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
         
        
        if (Hunger > 0) Hunger -= 0.01f; 
    }

    protected override void OnDeath()
    { 
        Game.Player.transform.position = Utility.AddToVector(Game.Player.transform.position, 0,7, 0);
        PlayerStatus = PlayerStatus.Freeze;
    }
 
    // for later passive effects boosts
    public static float GetRange()
    {
        return 1 * Inventory.CurrentItemData.Range;
    }
    public static float GetSpeed()
    {
        return 1 * Inventory.CurrentItemData.Speed;
    } 
}