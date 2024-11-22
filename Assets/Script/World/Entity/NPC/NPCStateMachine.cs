 
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCStateMachine : EntityStateMachine
{
    private NPCMovementInst _npcMovementInst;
    private NPCPathFindInst _npcPathFindInst;
    private NPCAnimationInst _npcAnimationInst; 
    public SpriteRenderer _sprite;
    private List<EntityState> states; 
    void Awake()
    {
        _sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        _npcMovementInst = GetComponent<NPCMovementInst>();
        _npcPathFindInst = GetComponent<NPCPathFindInst>();
        _npcAnimationInst = GetComponent<NPCAnimationInst>();
        
        states = new List<EntityState>();
        states.Add(new NPCIdle(_npcMovementInst, _npcAnimationInst));
        states.Add(new NPCChase(_npcMovementInst, _npcPathFindInst, _npcAnimationInst, _sprite));
        DialogueData _dialogueData = new DialogueData();
        _dialogueData.Lines.Add("I'M DELETING YOU, BROTHER!");
        _dialogueData.Lines.Add("\u2588\u2588]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]] 10% complete.....");
        _dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588]]]]]]]]]]]]]]]]]]]]] 35% complete....");
        _dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588]]]]]]]]]]]] 60% complete....");
        _dialogueData.Lines.Add("\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588] 99% complete.....");
        states.Add(new CharTalk(this, _dialogueData));
        
        Initialize<NPCIdle>(states);    
    }

    protected override void LogicUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState(GetState<NPCChase>());
        }
    }

    public void interact()
    {
        SetState(GetState<CharTalk>());
    }
} 