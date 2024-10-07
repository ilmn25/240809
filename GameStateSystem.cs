using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState { FreeRoam, Dialogue, Battle }

public class GameStateSystem : MonoBehaviour
{
    private DialogueSystem _dialogueSystem;
    private PlayerInteractionSystem _playerInteractionSystem;

    [HideInInspector]
    public GameState GAMESTATE = GameState.FreeRoam;

    // 
    void Start()
    {
        _dialogueSystem = GameObject.Find("viewport_system").GetComponent<DialogueSystem>();
        _playerInteractionSystem = GameObject.Find("player").GetComponent<PlayerInteractionSystem>(); 

        // The event handlers for OnShowDialog and OnHideDialog need to be set up when the game object is first created, so that the game can properly track the dialogue state.
        _dialogueSystem.OnShowDialog += () =>
        {
            GAMESTATE = GameState.Dialogue;
        };
        _dialogueSystem.OnHideDialog += () =>
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
                _playerInteractionSystem.HandleInteraction();
                break;
        }
    }
}
