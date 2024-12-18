using System;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{
    public static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
    public static readonly float MaxDeltaTime = 0.03f;
    
    private const float FixedUpdateMS = 0.10f;
    public static string DownloadPath;
    public static string PlayerSavePath;  
    
    public static LayerMask MaskMap;  
    public static LayerMask MaskStatic;
    public static LayerMask MaskEntity;
    public static int IndexMap;
    public static int IndexStatic;
    public static int IndexDynamic;
    public static int IndexUI;
    
    public static GameObject Player;
    public static GameObject CameraObject;
    public static Camera Camera;
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
    
    public static AudioClip DigSound;
    public static AudioClip PickUpSound;
    public static AudioClip ChatSound;
    
    public void Awake()
    { 
        Time.fixedDeltaTime = FixedUpdateMS;
        Application.targetFrameRate = 200;
        SetConstants(); 
        // Physics.IgnoreLayerCollision(Game.IndexEntity, Game.IndexMap);
        // Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexMap);
        // Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexEntity);
    }

    private void Start()
    {
        Player.transform.position = new Vector3( 
            World.ChunkSize * WorldGen.Size.x / 2,
            World.ChunkSize * WorldGen.Size.y + 15,
            World.ChunkSize * WorldGen.Size.z / 2);
          
        Item.Initialize();
        global::GUI.Initialize();
        PlayerData.Load(); 
        WorldGen.Initialize();
        Audio.Initialize();
        PlayerStatus.Initialize(); 
        Block.Initialize(); 
        MapCull.Initialize(); 
        EntityDynamicLoad.Initialize();  
        MapLoad.Initialize();
        Scene.Initialize(); 
    }

    private void Update()
    { 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            World.Save(0);
        }
        
        global::GUI.Update();
        SetPiece.Update();
        PlayerStatus.Update();
        Inventory.Update();
        PlayerTerraform.Update();
        InputHandler.Update();
        MapCull.Update();
         
    }
    private void FixedUpdate()
    { 
        Scene.Update(); 
    }

    private void OnApplicationQuit()
    {
        Block.Dispose();
        PlayerData.Save();
    }

    public static void SetConstants()
    {
        DigSound = Resources.Load<AudioClip>("audio/sfx/dig/stone");
        PickUpSound = Resources.Load<AudioClip>("audio/sfx/pick_up");
        ChatSound = Resources.Load<AudioClip>("audio/sfx/chat"); 
        
        MaskMap  = LayerMask.GetMask("Map"); 
        MaskStatic  = LayerMask.GetMask("Static", "Map"); 
        MaskEntity = LayerMask.GetMask("Static", "Dynamic");
        IndexMap = LayerMask.NameToLayer("Map");
        IndexStatic = LayerMask.NameToLayer("Static");
        IndexDynamic = LayerMask.NameToLayer("Dynamic");
        IndexUI = LayerMask.NameToLayer("UI");
        
        DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        PlayerSavePath = $"{DownloadPath}\\PlayerData.dat";
        
        Player = GameObject.Find("player");
        CameraObject = GameObject.Find("main_camera");
        Camera = CameraObject.GetComponent<Camera>();
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
         
    }
 
}
