 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCStateMachine : EntityStateMachine
{
    private NPCMovementInst _npcMovementInst;
    private NPCPathFindInst _npcPathFindInst;
    private NPCAnimationInst _npcAnimationInst; 
    private SpriteRenderer _sprite;
    protected override void OnAwake()
    {
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _npcPathFindInst = GetComponent<NPCPathFindInst>();
        _npcAnimationInst = GetComponent<NPCAnimationInst>();
        
        AddState(new NPCIdle(_npcMovementInst, _npcAnimationInst), true);
        AddState(new NPCChase(_npcMovementInst, _npcPathFindInst, _npcAnimationInst, _sprite));
        DialogueData dialogueData = new DialogueData();
        dialogueData.Lines.Add("I'M DELETING YOU, BROTHER!");
        dialogueData.Lines.Add("\u2588\u2588]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]] 10% complete.....");
        dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588]]]]]]]]]]]]]]]]]]]]] 35% complete....");
        dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588]]]]]]]]]]]] 60% complete....");
        dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588] 99% complete.....");
        AddState(new CharTalk(this, dialogueData));
    }

    public void OnEnable()
    {
        SetState<NPCIdle>();
        _npcMovementInst.SetDirection(Vector3.zero);
    }
    protected override void LogicUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState<NPCChase>();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            transform.position = Game.Player.transform.position;
        }
    }

    public void Interact()
    {
        SetState<CharTalk>();
    }
} 