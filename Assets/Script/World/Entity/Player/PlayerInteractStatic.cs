using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractStatic : MonoBehaviour
{
    public static PlayerInteractStatic Instance { get; private set; }  
    
    private LayerMask _interactLayer;
    private Animator _animator;

    void Awake()
    {
        Instance = this;
        _interactLayer = LayerMask.GetMask("Interact");
        _animator = transform.Find("sprite").GetComponent<Animator>();
    }
    
    public void HandleInteractionUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {   
            var facingDir = new Vector3(_animator.GetFloat("PosX"), 0, _animator.GetFloat("PosY"));
            var interactPos = transform.position + facingDir / 2;

            if (Physics.OverlapSphere(interactPos, 1, _interactLayer).Length > 0)
            {       
                // dialogue system starts here
                Collider[] hitColliders = Physics.OverlapSphere(interactPos, 1, _interactLayer);
                foreach (var hitCollider in hitColliders)
                { 
                    NPCStateMachine NPCStateMachine = hitCollider.GetComponent<NPCStateMachine>();
                    if (NPCStateMachine != null)
                    {
                        NPCStateMachine.interact();
                        break;  
                    }
                }
            }
        }
    }

}
