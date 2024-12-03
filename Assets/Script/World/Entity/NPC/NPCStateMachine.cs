 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCStateMachine : EntityStateMachine
{
    private MovementModule _movementModule;
    private PathFindModule _pathFindModule;
    private AnimationModule _animationModule; 
    private SpriteRenderer _sprite;
    protected override void OnAwake()
    {
        GUIDialogueSingleton.DialogueAction += DialogueAction;
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _movementModule = GetComponent<MovementModule>();
        _pathFindModule = GetComponent<PathFindModule>();
        _animationModule = GetComponent<AnimationModule>();
        
        AddState(new NPCIdle(_movementModule, _animationModule), true);
        AddState(new NPCChase(_movementModule, _pathFindModule, _animationModule, _sprite));
        AddState(new NPCRoam(_movementModule, _pathFindModule, _animationModule, _sprite));
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("help");
        dialogueData.Lines.Add("I cant fix my raycast ");
        dialogueData.Lines.Add("im about to kms rahhhhhhh");
        AddState(new CharTalk(this, dialogueData));
    }

    private void DialogueAction()
    {
        if (Vector3.Distance(transform.position, Game.Player.transform.position) < 1.4f)
        {
            SetState<CharTalk>();
        }
    }

    public void OnEnable()
    {
        SetState<NPCIdle>();
        _movementModule.SetDirection(Vector3.zero);
    }
    
    protected override void LogicUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SetState<NPCChase>();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            SetState<NPCRoam>();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            transform.position = Game.Player.transform.position;
        }
 
    }
} 