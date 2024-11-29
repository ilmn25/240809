using UnityEngine;
using System.Collections;

public class LightBlock : MonoBehaviour
{
    private const float DELAY = 4.0f; // Delay in seconds before starting the calls
    private Light _light;
    private Vector3 _offset;
    private void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine(WaitAndStart());
    }

    private IEnumerator WaitAndStart()
    { 
        yield return new WaitForSeconds(DELAY);
        while (true)
        {
            _offset = Game.Player.transform.position - transform.position;
            _offset.y = 0; // Ignore the y-axis
            _offset.Normalize();
            // Lib.Log(direction);

            _light.enabled =
                WorldStatic.Instance.GetBoolInMap(Vector3Int.FloorToInt(transform.position)) &&
                WorldStatic.Instance.GetBoolInMap(Vector3Int.FloorToInt(transform.position + _offset + Vector3Int.down));
            yield return null;
        }
    }
}