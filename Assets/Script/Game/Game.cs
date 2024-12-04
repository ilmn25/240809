using System;
using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{ 
    public static bool GUIBusy = false;
    public static string DOWNLOAD_PATH;
    public static string PLAYER_SAVE_PATH;  
    public static Game Instance { get; private set; }  
    
    public static LayerMask LayerMap; // just map
    public static LayerMask LayerCollide;
    public static int MapLayerIndex;
    public static int UILayerIndex;
    public static int EntityLayerIndex;
    
    public static GameObject UserSystem;
    public static GameObject Player;
    public static GameObject ViewportSystem;
    public static GameObject Camera;
    public static Camera GUICamera;
    public static GameObject GUI;
    public static GameObject GUIDialogue;
    public static TextMeshProUGUI GUIDialogueText;
    public static GameObject GUIInv;
    public static GameObject GUIInvCrafting;
    public static GameObject GUIInvStorage;
    public static GameObject GUICursor;
    public static GameObject GUICursorInfo;
    public static GameObject GUICursorSlot;
    public static GameObject AudioSystem;
    public static GameObject WorldSystem;
    public static GameObject EntitySystem;
    public static GameObject MapSystem;
    public static AudioClip DigSound;
     
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
        MapLayerIndex = LayerMask.NameToLayer("Map");
        UILayerIndex = LayerMask.NameToLayer("UI");
        EntityLayerIndex = LayerMask.NameToLayer("Entity");
        
        DOWNLOAD_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        PLAYER_SAVE_PATH = $"{DOWNLOAD_PATH}\\PlayerData.dat";
        
        
        UserSystem = GameObject.Find("game_system");
        Player = GameObject.Find("player");
        ViewportSystem = GameObject.Find("viewport_system");
        Camera = GameObject.Find("main_camera");

        GUICamera = GameObject.Find("hud_camera").GetComponent<Camera>();
        GUI = GameObject.Find("gui");
        GUIDialogue = GUI.transform.Find("dialogue_box").gameObject;
        GUIDialogueText = GUIDialogue.transform.Find("text").GetComponent<TextMeshProUGUI>();
        GUIInv = GUI.transform.Find("inventory").gameObject;
        GUIInvCrafting = GUIInv.transform.Find("crafting").gameObject;
        GUIInvStorage = GUIInv.transform.Find("storage").gameObject;
        GUICursor = GUI.transform.Find("cursor").Find("cursor").gameObject;
        GUICursorInfo = GUICursor.transform.Find("info").gameObject;
        GUICursorSlot = GUICursor.transform.Find("slot").gameObject;
        
        AudioSystem = GameObject.Find("audio_system");
        WorldSystem = GameObject.Find("world_system");
        EntitySystem = GameObject.Find("entity_system");
        MapSystem = GameObject.Find("map_system");
         
        Physics.IgnoreLayerCollision(Game.EntityLayerIndex, Game.MapLayerIndex);
        Physics.IgnoreLayerCollision(Game.UILayerIndex, Game.MapLayerIndex);
        Physics.IgnoreLayerCollision(Game.UILayerIndex, Game.EntityLayerIndex);
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
