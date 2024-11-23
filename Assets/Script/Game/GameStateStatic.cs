using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState { FreeRoam, Dialogue, Battle }

public class GameStateStatic : MonoBehaviour
{
    public static GameStateStatic Instance { get; private set; }  
    

    [HideInInspector]
    public GameState GAMESTATE = GameState.FreeRoam;

    // 
    void Start()
    {
        Instance = this;  
        

        // The event handlers for OnShowDialog and OnHideDialog need to be set up when the game object is first created, so that the game can properly track the dialogue state.
        GUIDialogueStatic.Instance.OnShowDialog += () =>
        {
            GAMESTATE = GameState.Dialogue;
        };
        GUIDialogueStatic.Instance.OnHideDialog += () =>
        {
            GAMESTATE = GameState.FreeRoam;
        };
    }

    // Update is called once per frame
    void Update()
    {
        switch (GAMESTATE)
        {
            case GameState.Dialogue:
                // DialogueSystem.Instance.HandleUpdate(); 
                break;
            case GameState.FreeRoam:
                break;
        }
    }
}
