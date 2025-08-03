
using UnityEngine;

public class ToolSwingState : State
{
    private const float ToolDelay = 0.05f; // Constant delay for tool swing
    private ItemToolState _parent;
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
        _parent = (ItemToolState)Parent;
    }
  
    public void StartAnimation()
    {
        if (_parent.IsBusy) return;
        
        _parent.IsBusy = true;
        _swingTimer = 0f; 
    }
    
    public void HandleAnimationUpdate()
    { 
        if (!_parent.IsBusy) return;
        
        _swingTimer += Time.deltaTime;
        float windUpProgress = Mathf.Clamp01(_swingTimer / WindUpDuration);
        float toolSwingProgress = Mathf.Clamp01((_swingTimer - WindUpDuration - ToolDelay) / ToolSwingDuration);
        float playerSwingProgress = Mathf.Clamp01((_swingTimer - WindUpDuration) / PlayerSwingDuration);
    
        if (_swingTimer < WindUpDuration)
        {
            // Wind-up phase: rotate player slightly in the opposite direction
            float playerZRotation = Mathf.Lerp(InitialZRotation, WindUpZRotation, windUpProgress);
            _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, playerZRotation);
            
            // Tool also performs a wind-up
            float toolZRotation = Mathf.Lerp(InitialZRotation, WindUpZRotation, windUpProgress);
            _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, toolZRotation);
        }
        else
        {
            // Swing phase for player
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

            // Swing phase for tool, starts and ends later
            if (_swingTimer >= WindUpDuration + ToolDelay)
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

        if (_swingTimer >= WindUpDuration + PlayerSwingDuration + ToolSwingDuration + ToolDelay)
        {
            // Swing complete for both
            _parent.PlayerSprite.transform.rotation = Quaternion.Euler(0, _parent.PlayerSprite.transform.rotation.eulerAngles.y, InitialZRotation);
            _parent.ToolSprite.transform.rotation = Quaternion.Euler(0, _parent.ToolSprite.transform.rotation.eulerAngles.y, InitialZRotation);
            _parent.IsBusy = false;
        }
    }
}
