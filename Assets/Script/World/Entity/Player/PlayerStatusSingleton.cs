    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusSingleton : MonoBehaviour
{
    public static PlayerStatusSingleton Instance { get; private set; }
    public static float _health;
    public static float _mana;
    public static float _sanity;
    public static float _hunger;
    public static float _stamina;
    public static float _speed;

    void Start()
    {
        Instance = this;
        _health = PlayerDataSingleton._playerData.health;
        _mana = PlayerDataSingleton._playerData.mana;
        _sanity = PlayerDataSingleton._playerData.sanity;
        _hunger = PlayerDataSingleton._playerData.hunger;
        _stamina = PlayerDataSingleton._playerData.stamina;
        _speed = PlayerDataSingleton._playerData.speed;
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

    public void HandleStatusUpdate()
    {
        if (_hunger > 0) _hunger -= 0.01f;
    }

    public void UpdateHealth(int amount)
    {
        _health += amount;
        if (_health > PlayerDataSingleton._playerData.health) _health = PlayerDataSingleton._playerData.health;
        else if (_health < 0) _health = 0;
        Debug.Log("Current Health: " + _health);
    }
}