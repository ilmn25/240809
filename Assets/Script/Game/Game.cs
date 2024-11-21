using TMPro;
using UnityEngine;
 
public class Game : MonoBehaviour
{
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
    
    public static Material MeshMaterial;
    public static Material SpriteMaterial;

    void Awake()
    {
        MeshMaterial = Resources.Load<Material>("shader/material/custom_lit");
        SpriteMaterial = Resources.Load<Material>("shader/material/custom_lit");
        
        UserSystem = GameObject.Find("game_system");
        Player = GameObject.Find("player");
        ViewportSystem = GameObject.Find("viewport_system");
        Camera = GameObject.Find("main_camera");
        DialogueBox = GameObject.Find("gui").transform.Find("dialogue_box").gameObject;
        DialogueText = DialogueBox.transform.Find("dialogue_text").GetComponent<TextMeshProUGUI>(); 
        AudioSystem = GameObject.Find("audio_system");
        WorldSystem = GameObject.Find("world_system");
        EntitySystem = GameObject.Find("entity_system");
        MapSystem = GameObject.Find("map_system");
         
    }
}
