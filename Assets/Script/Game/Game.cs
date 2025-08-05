using System;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
public enum GameState {
    MainMenu,
    Loading,
    Playing,
    Paused,
    Inventory,
    GameOver
}

public class Game : MonoBehaviour
{
    public static readonly float MaxDeltaTime = 0.03f;
    
    private const float FixedUpdateMS = 0.10f;
    
    public static GameState GameState = GameState.Loading;
    
    public static LayerMask MaskMap;  
    public static LayerMask MaskStatic;
    public static LayerMask MaskEntity;
    public static int IndexMap;
    public static int IndexStatic;
    public static int IndexDynamic;
    public static int IndexUI;
    
    public static GameObject Player;
    public static GameObject ViewPortObject;
    public static GameObject CameraObject;
    public static Camera Camera;
    public static Camera GUICamera;
    public static GameObject GUIObject;
    public static GameObject GUIDialogue;
    public static TextMeshProUGUI GUIDialogueText;
    public static GameObject GUIInv;
    public static GameObject GUIInvCrafting;
    public static GameObject GUIInvStorage;
    public static GameObject GUICursor;
    public static GameObject GUICursorInfo;
    public static GameObject GUICursorSlot;
     
    
    public void Awake()
    { 
        Time.fixedDeltaTime = FixedUpdateMS;
        Application.targetFrameRate = 160;
        SetConstants(); 
        // Physics.IgnoreLayerCollision(Game.IndexEntity, Game.IndexMap);
        // Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexMap);
        // Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexEntity);
    }

    private void Start()
    {
        Player.transform.position = new Vector3( 
            World.ChunkSize * WorldGen.Size.x / 2,
            World.ChunkSize * WorldGen.Size.y-25,
            World.ChunkSize * WorldGen.Size.z / 2);
          
        Control.Initialize(); 
        Item.Initialize();
        Loot.Initialize();
        GUI.Initialize(); 
        WorldGen.Initialize();
        Audio.Initialize();
        Block.Initialize(); 
        MapCull.Initialize(); 
        EntityDynamicLoad.Initialize();  
        MapLoad.Initialize();
        Scene.Initialize();  
        ViewPort.Initialize();  
    }

    private void Update()
    { 
        if (Control.Inst.Pause.KeyDown())
        {
            World.Save(0);
        }
        
        GUI.Update();
        SetPiece.Update();
        Inventory.Update();
        Control.Update();
        MapCull.Update();
        ViewPort.Update(); 
         
    }
    private void FixedUpdate()
    { 
        Scene.Update(); 
    }

    private void OnApplicationQuit()
    {
        Block.Dispose();
        PlayerData.Save();
        Control.Save(); 
        MapLoad.CancellationTokenSourceKillGame.Cancel();
    }

    public static void SetConstants()
    { 
        
        MaskMap  = LayerMask.GetMask("Map"); 
        MaskStatic  = LayerMask.GetMask("Static", "Map"); 
        MaskEntity = LayerMask.GetMask("Static", "Dynamic");
        IndexMap = LayerMask.NameToLayer("Map");
        IndexStatic = LayerMask.NameToLayer("Static");
        IndexDynamic = LayerMask.NameToLayer("Dynamic");
        IndexUI = LayerMask.NameToLayer("UI");
 
        
        Player = GameObject.Find("player");
        ViewPortObject = GameObject.Find("viewport");
        CameraObject = GameObject.Find("main_camera");
        Camera = CameraObject.GetComponent<Camera>();
        GUICamera = GameObject.Find("hud_camera").GetComponent<Camera>();
        
        GUIObject = GameObject.Find("gui");
        GUIDialogue = GUIObject.transform.Find("dialogue_box").gameObject;
        GUIDialogueText = GUIDialogue.transform.Find("text").GetComponent<TextMeshProUGUI>();
        GUIInv = GUIObject.transform.Find("inventory").gameObject;
        GUIInvCrafting = GUIInv.transform.Find("crafting").gameObject;
        GUIInvStorage = GUIInv.transform.Find("storage").gameObject;
        GUICursor = GUIObject.transform.Find("cursor").Find("cursor").gameObject;
        GUICursorInfo = GUICursor.transform.Find("info").gameObject;
        GUICursorSlot = GUICursor.transform.Find("slot").gameObject;
         
    }
 
}
