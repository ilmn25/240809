using UnityEngine;

public partial class ViewPort
{
    private static int _shakeDuration;
    private const int ShakeInterval = 2;
    private static float _currentShakeTime;
    private static int _currentShakeInterval;
    private static Vector3 _direction;
    
    private static float _shakeMagnitude = 0.1f; 
    public static Vector3 ShakeOffset;
    
    private static void HandleScreenShake()
    {
        if (_currentShakeTime != 0)
        {
            _currentShakeInterval++;
            if (_currentShakeInterval == ShakeInterval)
            { 
                ShakeOffset = (_direction == default ? Random.insideUnitSphere : _direction) * 
                              (_shakeMagnitude * (_currentShakeTime / _shakeDuration));
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

    public static void StartScreenShake(int duration, float magnitude, Vector3 direction = default)
    {
        _direction = direction; 
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
        _currentShakeTime = _shakeDuration;
    }
}
