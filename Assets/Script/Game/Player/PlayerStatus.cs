using UnityEngine;

public class PlayerStatus
{
    public static float Health;
    public static float Mana;
    public static float Sanity;
    public static float Hunger;
    public static float Stamina;
    public static float Speed;
    private static PlayerMachine _playerMachine;
    public static void Initialize()
    {
        _playerMachine = Game.Player.transform.GetComponent<PlayerMachine>();
        Health = PlayerData.Inst.health;
        Mana = PlayerData.Inst.mana;
        Sanity = PlayerData.Inst.sanity;
        Hunger = PlayerData.Inst.hunger;
        Stamina = PlayerData.Inst.stamina;
        Speed = PlayerData.Inst.speed;
    }

    public static void Update()
    {
        if (Hunger > 0) Hunger -= 0.01f;
        if (Health == 0)
        {
            Audio.PlaySFX("player_die",0.5f);
            Health = 100;
            Game.Player.transform.position = new Vector3(100, 200, 100);
        }
    }

    public static void UpdateHealth(int amount)
    {
        Health += amount;
        if (Health > PlayerData.Inst.health) Health = PlayerData.Inst.health;
        else if (Health < 0) Health = 0;
        Debug.Log("Current Health: " + Health);
    }

    public static void hit(int dmg, int knockback, Vector3 position)
    {
        UpdateHealth(-dmg);
        _playerMachine.GetModule<PlayerMovementModule>().KnockBack(position, knockback, true);
        Audio.PlaySFX("player_hurt",0.4f);
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