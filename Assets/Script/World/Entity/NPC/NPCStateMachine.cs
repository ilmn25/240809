 
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
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _movementModule = GetComponent<MovementModule>();
        _pathFindModule = GetComponent<PathFindModule>();
        _animationModule = GetComponent<AnimationModule>();
        
        AddState(new NPCIdle(_movementModule, _animationModule), true);
        AddState(new NPCChase(_movementModule, _pathFindModule, _animationModule, _sprite));
        AddState(new NPCRoam(_movementModule, _pathFindModule, _animationModule, _sprite));
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("Dicks are so cute omg UwU when you hold one in your hand and it starts twitching its like its nuzzling you(/ω＼) or when they perk up and look at you like\" owo nya? :3” hehe ~ penis-kun is happy to see me!!");
        AddState(new CharTalk(this, dialogueData));
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
        }else if (Input.GetKeyDown(KeyCode.T))
        {
            SetState<NPCRoam>();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            transform.position = Game.Player.transform.position;
        }  
    }

    public void Interact()
    {
        SetState<CharTalk>();
    }
} 