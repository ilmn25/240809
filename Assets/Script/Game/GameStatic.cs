using UnityEngine;

public class GameStatic : MonoBehaviour
{
    float FIXED_UPDATE_MS = 0.10f;
    public static float MAX_DELTA_TIME = 0.03f;
    public static float _deltaTime = 0.04f;

    private static GameObject _player;

    void Start()
    {
        _player = GameObject.Find("player");
        Time.fixedDeltaTime = FIXED_UPDATE_MS;
        Application.targetFrameRate = 200;
    }

    void Update()
    {
        if (_player.transform.position.y < -100)
        {
            GameObject.Find("map_system").GetComponent<MapCullStatic>().ForceRevertMesh();
            _player.transform.position = Lib.AddToVector(_player.transform.position, 0, 200, 0);
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
