using UnityEngine;

public class EquipSwingState : State
{
    private const float ToolDelay = 0.05f; // Constant delay for tool swing
    private EquipState _parent;
    private float _swingTimer = 0f;
    private const float WindUpDuration = 0.1f; // Duration of the wind-up in seconds
    private const float ToolSwingDuration = 0.2f; // Duration of the tool swing in seconds
    private const float PlayerSwingDuration = 0.2f; // Duration of the player swing in seconds
    private const float InitialZRotation = 0f;
    private const float WindUpZRotation = 20f; // Slight backward rotation for wind-up
    private const float ToolTargetZRotation = -90f;
    private const float PlayerTargetZRotation = -35f; // Rotation for player's swing 

    public override void OnInitialize()
    {
        _parent = (EquipState)Parent;
    }
  
    public void Use()
    {
        PlayerStatus.IsBusy = true; 
        _swingTimer = 0f;
        Attack();
    } 
    
    public override void OnUpdateState()
    { 
        if (!PlayerStatus.IsBusy) return;

        float speedMultiplier = PlayerStatus.GetSpeed();  
        _swingTimer += Time.deltaTime;

        // Adjusted durations based on speed
        float windUpDuration = WindUpDuration / speedMultiplier;
        float toolSwingDuration = ToolSwingDuration / speedMultiplier;
        float playerSwingDuration = PlayerSwingDuration / speedMultiplier;
        float toolDelay = ToolDelay / speedMultiplier;

        float windUpProgress = Mathf.Clamp01(_swingTimer / windUpDuration);
        float toolSwingProgress = Mathf.Clamp01((_swingTimer - windUpDuration - toolDelay) / toolSwingDuration);
        float playerSwingProgress = Mathf.Clamp01((_swingTimer - windUpDuration) / playerSwingDuration);

        // Wind-up phase
        if (_swingTimer < windUpDuration)
        {
            float playerZRotation = Mathf.Lerp(InitialZRotation, WindUpZRotation, windUpProgress);
            _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, playerZRotation);

            float toolZRotation = Mathf.Lerp(InitialZRotation, WindUpZRotation, windUpProgress);
            _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, toolZRotation);
        }
        else
        {
            // Player swing phase
            if (playerSwingProgress < 0.5f)
            {
                float playerZRotation = Mathf.Lerp(WindUpZRotation, PlayerTargetZRotation, playerSwingProgress * 2);
                _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, playerZRotation);
            }
            else if (playerSwingProgress < 1f)
            {
                float playerZRotation = Mathf.Lerp(PlayerTargetZRotation, InitialZRotation, (playerSwingProgress - 0.5f) * 2);
                _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, playerZRotation);
            }

            // Tool swing phase
            if (_swingTimer >= windUpDuration + toolDelay)
            {
                if (toolSwingProgress < 0.5f)
                {
                    float toolZRotation = Mathf.Lerp(WindUpZRotation, ToolTargetZRotation, toolSwingProgress * 2);
                    _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, toolZRotation);
                }
                else if (toolSwingProgress < 1f)
                {
                    float toolZRotation = Mathf.Lerp(ToolTargetZRotation, InitialZRotation, (toolSwingProgress - 0.5f) * 2);
                    _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, toolZRotation);
                }
            }
        }

        // Reset when complete
        if (_swingTimer >= windUpDuration + playerSwingDuration + toolSwingDuration + toolDelay)
        {
            _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, InitialZRotation);
            _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, InitialZRotation);
            PlayerStatus.IsBusy = false;
        }
    }


    private void Attack()
    {
        Collider[] hitColliders = Physics.OverlapBox(Machine.transform.position, Vector3.one * PlayerStatus.GetRange(), Quaternion.identity, Game.MaskEntity);
        IHitBox target;
        foreach (Collider collider in hitColliders)
        {
            target = collider.gameObject.GetComponent<IHitBox>();
            if (target == null) continue;
            target.OnHit(); 
        }
    }
}
