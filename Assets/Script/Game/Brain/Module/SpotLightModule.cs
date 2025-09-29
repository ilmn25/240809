using UnityEngine;
using System.Collections;

public class SpotLightModule : MonoBehaviour
{
    private const int ENABLE_THRESHOLD = 200; // Number of frames shouldEnable needs to be true
    private Light _light;
    private Vector3 _offset;
    private bool _isFading = false;
    private float _intensity;
    private int _enableCounter = 0;

    private void Start()    
    {
        _light = GetComponent<Light>();
        _intensity = _light.intensity;
        StartCoroutine(WaitAndStart());
    }

    private IEnumerator WaitAndStart()
    { 
        while (true)
        {
            while (Game.SceneMode == SceneMode.Menu) yield return null;
            _offset = Game.Player.transform.position - transform.position;
            _offset.y = 0; // Ignore the y-axis
            _offset.Normalize();

            bool shouldEnable = 
                NavMap.Get(Vector3Int.FloorToInt(transform.position)) &&
                NavMap.Get(Vector3Int.FloorToInt(transform.position + _offset + Vector3Int.down));

            if (shouldEnable) // on
            {
                _enableCounter++;
                if (_enableCounter >= ENABLE_THRESHOLD && _light.intensity == 0f && !_isFading)
                {
                    StartCoroutine(FadeLight(true, 0.4f));
                }
            }
            else // off
            { 
                _enableCounter = 0;
                if (_light.intensity > 0f && !_isFading)
                {
                    StartCoroutine(FadeLight(false, 0.15f));
                }
            }

            yield return null;
        }
    }

    private IEnumerator FadeLight(bool fadeIn, float duration)
    {
        _isFading = true;
        float startIntensity = _light.intensity;
        float endIntensity = fadeIn ? _intensity : 0f;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            _light.intensity = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _light.intensity = endIntensity;
        _isFading = false;
    }
}
