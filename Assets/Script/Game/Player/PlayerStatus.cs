using UnityEngine;

public class PlayerStatus
{
    public static float Health;
    public static float Mana;
    public static float Sanity;
    public static float Hunger;
    public static float Stamina;
    public static float Speed;
 
    public static void Initialize()
    {
        Health = PlayerData.playerData.health;
        Mana = PlayerData.playerData.mana;
        Sanity = PlayerData.playerData.sanity;
        Hunger = PlayerData.playerData.hunger;
        Stamina = PlayerData.playerData.stamina;
        Speed = PlayerData.playerData.speed;
    }

    public static void Update()
    {
        if (Hunger > 0) Hunger -= 0.01f;
    }

    public static void UpdateHealth(int amount)
    {
        Health += amount;
        if (Health > PlayerData.playerData.health) Health = PlayerData.playerData.health;
        else if (Health < 0) Health = 0;
        Debug.Log("Current Health: " + Health);
    }

    // void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 25;
    //     style.normal.textColor = Color.white;
    //     style.alignment = TextAnchor.UpperRight;
    //
    //     Rect rect = new Rect(Screen.width - 100, 10, 90, 100);
    //     GUI.Label(rect, 
    //         $"Health: {_health}\n" +
    //         $"Mana: {_mana}\n" +
    //         $"Sanity: {_sanity}\n" +
    //         $"Hunger: {_hunger}\n" +
    //         $"Stamina: {_stamina}\n" +
    //         $"Speed: {_speed}", 
    //         style);
    // }
}