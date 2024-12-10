using System;
using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{ 
    public static bool GUIBusy = false;
    public static string DOWNLOAD_PATH;
    public static string PLAYER_SAVE_PATH;  
    public static Game Instance { get; private set; }  
    
    public static LayerMask MaskMap; // just map
    public static LayerMask MaskMapAndCollision;
    public static int IndexMap;
    public static int IndexUI;
    public static int IndexEntity;
    public static int IndexLC;
    
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
    public static AudioClip PickUpSound;
    public static AudioClip ChatSound;
     
    public static float MAX_DELTA_TIME = 0.03f;
    float FIXED_UPDATE_MS = 0.10f;
    void Awake()
    { 
        Time.fixedDeltaTime = FIXED_UPDATE_MS;
        Application.targetFrameRate = 200;
        
        DigSound = Resources.Load<AudioClip>("audio/sfx/dig/stone");
        PickUpSound = Resources.Load<AudioClip>("audio/sfx/pick_up");
        ChatSound = Resources.Load<AudioClip>("audio/sfx/chat");
        Instance = this;
        
        MaskMap  = LayerMask.GetMask("Map"); 
        MaskMapAndCollision  = LayerMask.GetMask("Collision", "Map"); 
        LayerMask.GetMask("Entity"); 
        LayerMask.GetMask("LC");
        IndexMap = LayerMask.NameToLayer("Map");
        IndexUI = LayerMask.NameToLayer("UI");
        IndexEntity = LayerMask.NameToLayer("Entity");
        LayerMask.NameToLayer("Collision");
        IndexLC = LayerMask.NameToLayer("LC");
        
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
         
        Physics.IgnoreLayerCollision(Game.IndexEntity, Game.IndexMap);
        Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexMap);
        Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexEntity);
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

    public static bool isLayer(int colliderIndex, int targetIndex)
    {
        return (colliderIndex & (1 << targetIndex)) != 0;
    }
    
    public static float GetDeltaTime()
    {
        return (Time.deltaTime < MAX_DELTA_TIME) ? Time.deltaTime : MAX_DELTA_TIME;
    }
}
