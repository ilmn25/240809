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
        if (PlayerInventorySingleton.CurrentItem.Stack == 0)
        {
            SetState<ItemEmpty>();
            return;
        }
        
        switch (ItemLoadSingleton.GetItem(PlayerInventorySingleton.CurrentItem.StringID).Type)
        {
            case ItemType.Block:
                SetState<ItemBlock>();
                break;
            case ItemType.Tool:
                SetState<ItemTool>();
                break;
            default:
                SetState<ItemEmpty>();
                break;
        }
    }
}

public class ItemEmpty : State
{
    public override void OnEnterState()
    {
        StateMachine.transform.Find("sprite").transform.Find("tool").gameObject.SetActive(false);
    }
}


public class ItemFurniture : State
{
    public string stringID; 

    public override void StateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            stringID = PlayerInventorySingleton.CurrentItem.StringID;
        } 
    }
}

public class ItemBlock : State
{
    public override void StateUpdate()
    {  
        PlayerChunkEditSingleton.Instance._blockStringID = PlayerInventorySingleton.CurrentItem.StringID;
    }

    public override void OnExitState()
    {
        PlayerChunkEditSingleton.Instance._blockStringID = null;
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
        PlayerChunkEditSingleton.Instance._toolData = ItemLoadSingleton.GetItem(PlayerInventorySingleton.CurrentItem.StringID);
    }

    public override void OnExitState()
    {
        PlayerChunkEditSingleton.Instance._toolData = null;
        toolSprite.gameObject.SetActive(false);
    }

    public override void StateUpdate()
    { 
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

