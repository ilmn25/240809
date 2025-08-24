using UnityEngine;

public partial class ViewPort
{
    private static int _shakeDuration;
    private const int ShakeInterval = 2;
    private static float _currentShakeTime;
    private static int _currentShakeInterval;
    
    private static float _shakeMagnitude = 0.1f; 
    public static Vector3 ShakeOffset;
    
    private static void HandleScreenShake()
    {
        if (_currentShakeTime != 0)
        {
            _currentShakeInterval++;
            if (_currentShakeInterval == ShakeInterval)
            { 
                ShakeOffset = Random.insideUnitSphere * (_shakeMagnitude * (_currentShakeTime / _shakeDuration));
                _currentShakeTime --;
                _currentShakeInterval = 0;
            }  
        }
        else
        {
            _currentShakeInterval = 0;
            ShakeOffset = Vector3.zero;
        }
    }

    public static void StartScreenShake(int duration, float magnitude)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
        _currentShakeTime = _shakeDuration;
    }
}
