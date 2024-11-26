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
    private  float TOOL_DELAY = 0.05f; // Constant delay for tool swing

    private bool isSwinging = false;
    private float swingTimer = 0f;
    private float windUpDuration = 0.1f; // Duration of the wind-up in seconds
    private float toolSwingDuration = 0.2f; // Duration of the tool swing in seconds
    private float playerSwingDuration = 0.2f; // Duration of the player swing in seconds
    private float initialZRotation = 0f;
    private float windUpZRotation = 20f; // Slight backward rotation for wind-up
    private float toolTargetZRotation = -90f;
    private float playerTargetZRotation = -35f; // Rotation for player's swing
    private Transform playerSprite;
    private Transform toolSprite;

    public override void OnEnterState()
    { 
        playerSprite = Game.Player.transform.Find("sprite").transform.Find("char");
        toolSprite = Game.Player.transform.Find("sprite").transform.Find("tool"); 
        toolSprite.gameObject.SetActive(true);
        PlayerChunkEditStatic.Instance._toolData = ItemLoadStatic.GetItem(PlayerInventoryStatic.CurrentItem.StringID);
    }

    public override void OnExitState()
    {
        PlayerChunkEditStatic.Instance._toolData = null;
        toolSprite.gameObject.SetActive(false);
    }

    public override void StateUpdate()
    {
        TOOL_DELAY = Game.Instance.tooldelay;
        windUpDuration = Game.Instance.windUpDuration; // Duration of the wind-up in seconds
        toolSwingDuration = Game.Instance.toolSwingDuration; // Duration of the tool swing in seconds
        playerSwingDuration = Game.Instance.playerSwingDuration; // Duration of the player swing in seconds
        initialZRotation = Game.Instance.initialZRotation;
        windUpZRotation = Game.Instance.windUpZRotation; // Slight backward rotation for wind-up
        toolTargetZRotation = Game.Instance.toolTargetZRotation;
        playerTargetZRotation = Game.Instance.playerTargetZRotation; // Rotation for player's swing

        if (!isSwinging)
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(0))
            {
                isSwinging = true;
                swingTimer = 0f;
            }
        }
        else
        {
            swingTimer += Time.deltaTime;
            float windUpProgress = Mathf.Clamp01(swingTimer / windUpDuration);
            float toolSwingProgress = Mathf.Clamp01((swingTimer - windUpDuration - TOOL_DELAY) / toolSwingDuration);
            float playerSwingProgress = Mathf.Clamp01((swingTimer - windUpDuration) / playerSwingDuration);

            if (swingTimer < windUpDuration)
            {
                // Wind-up phase: rotate player slightly in the opposite direction
                float playerZRotation = Mathf.Lerp(initialZRotation, windUpZRotation, windUpProgress);
                playerSprite.transform.rotation = Quaternion.Euler(0, playerSprite.transform.rotation.eulerAngles.y, playerZRotation);
                
                // Tool also performs a wind-up
                float toolZRotation = Mathf.Lerp(initialZRotation, windUpZRotation, windUpProgress);
                toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, toolZRotation);
            }
            else
            {
                // Swing phase for player
                if (playerSwingProgress < 0.5f)
                {
                    float playerZRotation = Mathf.Lerp(windUpZRotation, playerTargetZRotation, playerSwingProgress * 2);
                    playerSprite.transform.rotation = Quaternion.Euler(0, playerSprite.transform.rotation.eulerAngles.y, playerZRotation);
                }
                else if (playerSwingProgress < 1f)
                {
                    float playerZRotation = Mathf.Lerp(playerTargetZRotation, initialZRotation, (playerSwingProgress - 0.5f) * 2);
                    playerSprite.transform.rotation = Quaternion.Euler(0, playerSprite.transform.rotation.eulerAngles.y, playerZRotation);
                }

                // Swing phase for tool, starts and ends later
                if (swingTimer >= windUpDuration + TOOL_DELAY)
                {
                    if (toolSwingProgress < 0.5f)
                    {
                        float toolZRotation = Mathf.Lerp(windUpZRotation, toolTargetZRotation, toolSwingProgress * 2);
                        toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, toolZRotation);
                    }
                    else if (toolSwingProgress < 1f)
                    {
                        float toolZRotation = Mathf.Lerp(toolTargetZRotation, initialZRotation, (toolSwingProgress - 0.5f) * 2);
                        toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, toolZRotation);
                    }
                }
            }

            if (swingTimer >= windUpDuration + playerSwingDuration + toolSwingDuration + TOOL_DELAY)
            {
                // Swing complete for both
                playerSprite.transform.rotation = Quaternion.Euler(0, playerSprite.transform.rotation.eulerAngles.y, initialZRotation);
                toolSprite.transform.rotation = Quaternion.Euler(0, toolSprite.transform.rotation.eulerAngles.y, initialZRotation);
                isSwinging = false;
            }
        }
    }
}

