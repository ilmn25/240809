using UnityEngine;

public class GameStatic : MonoBehaviour
{
    public static GameStatic Instance { get; private set; }  
    
    float FIXED_UPDATE_MS = 0.10f;
    public static float MAX_DELTA_TIME = 0.03f;
    public static float _deltaTime = 0.04f;


    void Start()
    {
        Instance = this;  
        Time.fixedDeltaTime = FIXED_UPDATE_MS;
        Application.targetFrameRate = 200;
    }

    void Update()
    {
        if (Game.Player.transform.position.y < -100)
        {
            GameObject.Find("map_system").GetComponent<MapCullStatic>().ForceRevertMesh();
            Game.Player.transform.position = new Vector3(Game.Player.transform.position.x , WorldStatic.CHUNKDEPTH + 100, Game.Player.transform.position.z);
        }

        _deltaTime = (Time.deltaTime < MAX_DELTA_TIME) ? Time.deltaTime : MAX_DELTA_TIME;

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
