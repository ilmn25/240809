using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusStatic : MonoBehaviour
{
    public static PlayerStatusStatic Instance { get; private set; }
    
    private int maxHealth = 100;
    [HideInInspector]   public int currentHealth;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }
 
    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Current Health: " + currentHealth);
    }
}
