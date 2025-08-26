using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Game : MonoBehaviour
{
    public const float MaxDeltaTime = 0.03f; 
    private const float FixedUpdateMS = 0.30f; 
            
    public static bool BuildMode = false;
    public static bool Fly = false;
    
    public static LayerMask MaskMap;  
    public static LayerMask MaskStatic;
    public static LayerMask MaskEntity;
    public static int IndexMap;
    public static int IndexCollide;
    public static int IndexNoCollide;
    public static int IndexSemiCollide; 
    
    public static GameObject Player;
    public static PlayerInfo PlayerInfo;
    public static GameObject ViewPortObject;
    public static GameObject CameraObject;
    public static GameObject LightIndoor;
    public static Light DirectionalLight;
    public static Light SpotLight;
    public static GameObject LightSelf;
    public static Volume Volume;
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
        Application.targetFrameRate = 100;
        Application.runInBackground = true;
        SetConstants(); 
        // PlayerData.Load();  
        
        
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
        surrogateSelector.AddSurrogate(typeof(Vector3Int), new StreamingContext(StreamingContextStates.All), new Vector3IntSerializationSurrogate());
        Helper.BinaryFormatter.SurrogateSelector = surrogateSelector; 
    }

    private void Start()
    {  
        EnvParticle.Initialize();  
        Control.Initialize();  
        Item.Initialize();
        Entity.Initialize(); 
        Loot.Initialize();
        GUIMain.Initialize(); 
        WorldGen.Initialize(); 
        Audio.Initialize();
        Block.Initialize();   
        MapCull.Initialize(); 
        Scene.Initialize();
        ViewPort.Initialize();
        Terraform.Initialize();
        Instantiate(Resources.Load<GameObject>($"Prefab/StructurePreviewPrefab")).AddComponent<StructurePreviewMachine>();
    }

    private void Update()
    {   
        if (Control.Inst.Pause.KeyDown()) _ = new CoroutineTask(Quit());
            // EditorApplication.isPlaying = false;
        
        Terraform.Update();
        GUIMain.Update();
        Inventory.Update();
        Control.Update();
        MapCull.Update();
        ViewPort.Update();  
        Environment.Update();
        MobSpawner.Update();
        return;
        
        IEnumerator Quit()
        {
            Environment.Target = Environment.Black;
            yield return new WaitForSeconds(2);
            World.Save(0);
            Application.Quit();
        }
    }
    private void FixedUpdate()
    { 
        Scene.Update(); 
    }

    private void OnApplicationQuit()
    {
        Block.Dispose();
        // PlayerData.Save();
        Control.Save(); 
        MapLoad.CancellationTokenSourceKillGame.Cancel();
    }

    public static void SetConstants()
    { 
        
        MaskMap  = LayerMask.GetMask("Map"); 
        MaskStatic  = LayerMask.GetMask("Collide", "Map"); 
        MaskEntity = LayerMask.GetMask("Collide", "NoCollide", "SemiCollide");
        IndexMap = LayerMask.NameToLayer("Map");
        IndexCollide = LayerMask.NameToLayer("Collide");
        IndexNoCollide = LayerMask.NameToLayer("NoCollide");
        IndexSemiCollide = LayerMask.NameToLayer("SemiCollide");
 
        ViewPortObject = GameObject.Find("Viewport");
        CameraObject = GameObject.Find("MainCamera");
        Camera = CameraObject.GetComponent<Camera>();
        LightIndoor = ViewPortObject.transform.Find("LightIndoor").gameObject;
        LightSelf = ViewPortObject.transform.Find("LightSelf").gameObject;   
        DirectionalLight = ViewPortObject.transform.Find("LightDir").gameObject.GetComponent<Light>();  
        SpotLight = ViewPortObject.transform.Find("LightSpot").gameObject.GetComponent<Light>();   
        Volume = CameraObject.gameObject.GetComponent<Volume>();
        GUICamera = GameObject.Find("HudCamera").GetComponent<Camera>();
        
        GUIObject = GameObject.Find("GUI");
        GUIDialogue = GUIObject.transform.Find("Dialogue").gameObject;
        GUIDialogueText = GUIDialogue.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        GUIImage = GUIObject.transform.Find("Image").gameObject;
        GUIInv = GUIObject.transform.Find("Inventory").gameObject;
        GUIInvCrafting = GUIInv.transform.Find("Crafting").gameObject;
        GUIInfoPanel = GUIInv.transform.Find("Info").gameObject;
        GUICursor = GUIObject.transform.Find("Cursor").Find("Cursor").gameObject;
        GUICursorInfo = GUICursor.transform.Find("Info").gameObject;    
        GUICursorSlot = GUICursor.transform.Find("Slot").gameObject;
    }
 
}
