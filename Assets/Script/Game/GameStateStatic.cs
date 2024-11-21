using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState { FreeRoam, Dialogue, Battle }

public class GameStateStatic : MonoBehaviour
{
    private GUIDialogueStatic _guiDialogueStatic;
    private PlayerInteractStatic _playerInteractStatic;

    [HideInInspector]
    public GameState GAMESTATE = GameState.FreeRoam;

    // 
    void Start()
    {
        _guiDialogueStatic = GameObject.Find("viewport_system").GetComponent<GUIDialogueStatic>();
        _playerInteractStatic = GameObject.Find("player").GetComponent<PlayerInteractStatic>(); 

        // The event handlers for OnShowDialog and OnHideDialog need to be set up when the game object is first created, so that the game can properly track the dialogue state.
        _guiDialogueStatic.OnShowDialog += () =>
        {
            GAMESTATE = GameState.Dialogue;
        };
        _guiDialogueStatic.OnHideDialog += () =>
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
                _playerInteractStatic.HandleInteraction();
                break;
        }
    }
}
