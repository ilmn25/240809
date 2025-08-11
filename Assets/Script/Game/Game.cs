using System;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine; 

public class Game : MonoBehaviour
{
    public static readonly float MaxDeltaTime = 0.03f;
    
    private const float FixedUpdateMS = 0.10f;
     
    
    public static LayerMask MaskMap;  
    public static LayerMask MaskStatic;
    public static LayerMask MaskEntity;
    public static int IndexMap;
    public static int IndexCollide;
    public static int IndexNoCollide;
    public static int IndexUI;
    
    public static GameObject Player;
    public static GameObject ViewPortObject;
    public static GameObject CameraObject;
    public static Camera Camera;
    public static Camera GUICamera;
    public static GameObject GUIObject;
    public static GameObject GUIDialogue;
    public static GameObject GUIImage;
    public static TextMeshProUGUI GUIDialogueText;
    public static GameObject GUIInv;
    public static GameObject GUIInvCrafting;
    public static GameObject GUIInfoPanel;
    public static GameObject GUICursor;
    public static GameObject GUICursorInfo;
    public static GameObject GUICursorSlot;
     
    
    public void Awake()
    { 
        Time.fixedDeltaTime = FixedUpdateMS;
        Application.targetFrameRate = 160;
        SetConstants(); 
        PlayerData.Load();  
    }

    private void Start()
    { 
        Control.Initialize(); 
        Item.Initialize();
        Entity.Initialize();
        Loot.Initialize();
        GUIMain.Initialize(); 
        WorldGen.Initialize();
        Audio.Initialize();
        Block.Initialize(); 
        MapCull.Initialize(); 
        EntityDynamicLoad.Initialize();  
        MapLoad.Initialize();
        Scene.Initialize();  
        ViewPort.Initialize();  
        Instantiate(Resources.Load<GameObject>($"prefab/item")).AddComponent<StructurePreviewMachine>(); 
    }

    private void Update()
    { 
        if (Control.Inst.Pause.KeyDown())
        {
            World.Save(0);
        }
        
        GUIMain.Update();
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
        IndexCollide = LayerMask.NameToLayer("Static");
        IndexNoCollide = LayerMask.NameToLayer("Dynamic");
        IndexUI = LayerMask.NameToLayer("UI");
 
        
        Player = GameObject.Find("player");
        ViewPortObject = GameObject.Find("viewport");
        CameraObject = GameObject.Find("main_camera");
        Camera = CameraObject.GetComponent<Camera>();
        GUICamera = GameObject.Find("hud_camera").GetComponent<Camera>();
        
        GUIObject = GameObject.Find("gui");
        GUIDialogue = GUIObject.transform.Find("dialogue_box").gameObject;
        GUIDialogueText = GUIDialogue.transform.Find("text").GetComponent<TextMeshProUGUI>();
        GUIImage = GUIObject.transform.Find("image").gameObject;
        GUIInv = GUIObject.transform.Find("inventory").gameObject;
        GUIInvCrafting = GUIInv.transform.Find("crafting").gameObject;
        GUIInfoPanel = GUIInv.transform.Find("info").gameObject;
        GUICursor = GUIObject.transform.Find("cursor").Find("cursor").gameObject;
        GUICursorInfo = GUICursor.transform.Find("info").gameObject;
        GUICursorSlot = GUICursor.transform.Find("slot").gameObject;
    }
 
}
