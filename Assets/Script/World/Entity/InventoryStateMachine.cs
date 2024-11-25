using Unity.VisualScripting;
using UnityEngine;

public class InventoryStateMachine : StateMachine
{
        public static InventoryStateMachine Instance { get; private set; }  
        public override void OnAwake()
        {
                Instance = this;
                AddState(new ItemEmpty(), true);
                AddState(new ItemBlock());
                AddState(new ItemTool());
        }
        public void HandleItemUpdate()
        { 
                if (PlayerInventoryStatic.CurrentItem == null)
                {
                        SetState<ItemEmpty>();
                        return;
                }
                switch (ItemLoadStatic.GetItem(PlayerInventoryStatic.CurrentItem.StringID).Type)
                {
                        case ItemType.Block:
                                SetState<ItemBlock>();
                                break;
                        case ItemType.Tool:
                                SetState<ItemTool>();
                                break;
                }
        }
}

public class ItemEmpty : State
{ 
}

public class ItemBlock : State
{
        public override void StateUpdate()
        {  
                PlayerChunkEditStatic.Instance._blockStringID = PlayerInventoryStatic.CurrentItem.StringID;
        }

        public override void OnExitState()
        {
                PlayerChunkEditStatic.Instance._blockStringID = null;
        }
}

public class ItemTool : State
{
        private bool isSwinging = false; 
        private float swingTimer = 0f;
        private float swingDuration = 0.15f; // Duration of the swing in seconds
        private float initialZRotation = 0f;
        private float targetZRotation = -90f;
        private GameObject playerSprite;
        private GameObject toolSprite;
 
        public override void OnEnterState()
        {
                playerSprite = Game.Player.transform.Find("sprite").gameObject;
                toolSprite = Game.Player.transform.Find("tool").gameObject;
                toolSprite.SetActive(true);
        }

        public override void OnExitState()
        {
                toolSprite.SetActive(false);
        }
        public override void StateUpdate()
        {
                if (!isSwinging)
                {
                        if (Input.GetMouseButtonDown(0)) // Assuming left mouse button triggers the attack
                        {
                                isSwinging = true;
                                swingTimer = 0f;
                        }
                }
                else
                {
                        swingTimer += Time.deltaTime;
                        float swingProgress = swingTimer / swingDuration;

                        if (swingProgress < 0.5f)
                        {
                                // Swinging from 0 to -70 degrees
                                float zRotation = Mathf.Lerp(initialZRotation, targetZRotation, swingProgress * 2); // Multiply by 2 to speed up the first half
                                toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, zRotation);
                        }
                        else if (swingProgress < 1f)
                        {
                                // Swinging back from -70 to 0 degrees
                                float zRotation = Mathf.Lerp(targetZRotation, initialZRotation, (swingProgress - 0.5f) * 2); // Multiply by 2 to speed up the second half
                                toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, zRotation);
                        }
                        else
                        {
                                // Swing complete
                                toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, initialZRotation);
                                isSwinging = false;
                        }
                }
        }
}
 