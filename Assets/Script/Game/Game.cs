using System;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{  
    public static readonly float MaxDeltaTime = 0.03f;
    private const float FixedUpdateMS = 0.10f;
    public static bool GUIBusy = false;
    public static string DownloadPath;
    public static string PlayerSavePath;  
    
    public static LayerMask MaskMap; // just map
    public static LayerMask MaskMapAndCollision;
    public static int IndexMap;
    public static int IndexUI;
    public static int IndexEntity;
    
    public static GameObject Player;
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
    
    public static AudioClip DigSound;
    public static AudioClip PickUpSound;
    public static AudioClip ChatSound;
    
    public void Awake()
    {
        Time.fixedDeltaTime = FixedUpdateMS;
        Application.targetFrameRate = 200;
        SetConstants(); 
    }

    private void Start()
    {
        PlayerData.Load();
        PlayerStatus.Initialize();
    }

    private void Update()
    { 
        PlayerStatus.Update();
        InventorySingleton.Update();
        PlayerTerraform.Update();
        InputHandler.Update();
    }

    private void OnApplicationQuit()
    {
        PlayerData.Save();
    }

    public static void SetConstants()
    {
        DigSound = Resources.Load<AudioClip>("audio/sfx/dig/stone");
        PickUpSound = Resources.Load<AudioClip>("audio/sfx/pick_up");
        ChatSound = Resources.Load<AudioClip>("audio/sfx/chat"); 
        
        MaskMap  = LayerMask.GetMask("Map"); 
        MaskMapAndCollision  = LayerMask.GetMask("Collision", "Map"); 
        IndexMap = LayerMask.NameToLayer("Map");
        IndexUI = LayerMask.NameToLayer("UI");
        IndexEntity = LayerMask.NameToLayer("Entity");
        
        DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        PlayerSavePath = $"{DownloadPath}\\PlayerData.dat";
        
        
        Player = GameObject.Find("player");
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
        
        Physics.IgnoreLayerCollision(Game.IndexEntity, Game.IndexMap);
        Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexMap);
        Physics.IgnoreLayerCollision(Game.IndexUI, Game.IndexEntity);
    }

    public static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
}
