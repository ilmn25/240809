using System;
using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{
    public static string DOWNLOAD_PATH;
    public static string PLAYER_SAVE_PATH;  
    public static Game Instance { get; private set; }  
    
    public static LayerMask LayerMap; // just map
    public static LayerMask LayerCollide;
    public static int IndexLayerMap;
    
    public static GameObject UserSystem;
    public static GameObject Player;
    public static GameObject ViewportSystem;
    public static GameObject Camera;
    public static GameObject DialogueBox;
    public static TextMeshProUGUI DialogueText;
    public static GameObject AudioSystem;
    public static GameObject WorldSystem;
    public static GameObject EntitySystem;
    public static GameObject MapSystem;
    public static AudioClip DigSound;
    
    public float tooldelay = 0f;
    public float windUpDuration = 0.1f; // Duration of the wind-up in seconds
    public float toolSwingDuration = 0.2f; // Duration of the tool swing in seconds
    public float playerSwingDuration = 0.2f; // Duration of the player swing in seconds
    public float initialZRotation = 0f;
    public float windUpZRotation = 20f; // Slight backward rotation for wind-up
    public float toolTargetZRotation = -90f;
    public float playerTargetZRotation = -35f; // Rotation for player's swing
    public GameObject playerSprite;
    public GameObject toolSprite;

    public static float MAX_DELTA_TIME = 0.03f;
    float FIXED_UPDATE_MS = 0.10f;
    void Awake()
    {
        Time.fixedDeltaTime = FIXED_UPDATE_MS;
        Application.targetFrameRate = 200;
        
        DigSound = Resources.Load<AudioClip>("audio/sfx/dig/stone");
        Instance = this;
        LayerMap  = LayerMask.GetMask("Map"); 
        LayerCollide  = LayerMask.GetMask("Map", "Entity"); 
        IndexLayerMap = LayerMask.NameToLayer("Map");
        
        DOWNLOAD_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        PLAYER_SAVE_PATH = $"{DOWNLOAD_PATH}\\PlayerData.dat";
        
        
        UserSystem = GameObject.Find("game_system");
        Player = GameObject.Find("player");
        ViewportSystem = GameObject.Find("viewport_system");
        Camera = GameObject.Find("main_camera");
        
        DialogueBox = GameObject.Find("gui").transform.Find("dialogue_box").gameObject;
        DialogueText = DialogueBox.transform.Find("text").GetComponent<TextMeshProUGUI>();
        
        AudioSystem = GameObject.Find("audio_system");
        WorldSystem = GameObject.Find("world_system");
        EntitySystem = GameObject.Find("entity_system");
        MapSystem = GameObject.Find("map_system");
         
    }
    
    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Screen.fullScreen)
            {
                Screen.SetResolution(960, 540, false);
            }
            else
            {
                Screen.SetResolution(1920, 1080, true);
            }
        }
    }
    
    public static float GetDeltaTime()
    {
        return (Time.deltaTime < MAX_DELTA_TIME) ? Time.deltaTime : MAX_DELTA_TIME;
    }
}
