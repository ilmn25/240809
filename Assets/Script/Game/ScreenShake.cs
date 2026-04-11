using UnityEngine;

public static class ScreenShake
{
    private static float _speed;
    private static float _strength;
    private static float _duration;
    private static float _timeRemaining;
    private static Vector3 _direction;

    public static Vector3 Offset { get; private set; }

    public static Vector3 SampleNoiseOffset(float speed, float strength, float ySeed = 0.73f, float zSeed = 1.13f)
    {
        float t = Time.time * Mathf.Max(0f, speed);
        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(t, 0.37f) * 2f - 1f,
            Mathf.PerlinNoise(ySeed, t) * 2f - 1f,
            Mathf.PerlinNoise(t, zSeed) * 2f - 1f
        );
        return noise * Mathf.Max(0f, strength);
    }

    public static void Shake(float speed, float strength, float duration)
    {
        Shake(speed, strength, duration, Vector3.zero);
    }

    public static void Shake(float speed, float strength, float duration, Vector3 direction)
    {
        _speed = Mathf.Max(0f, speed);
        _strength = Mathf.Max(0f, strength);
        _duration = Mathf.Max(0f, duration);
        _timeRemaining = _duration;
        _direction = direction;

        if (_duration <= 0f || _strength <= 0f)
        {
            Offset = Vector3.zero;
        }
    }

    public static void Update()
    {
        if (_timeRemaining <= 0f || _duration <= 0f || _strength <= 0f)
        {
            Offset = Vector3.zero;
            return;
        }

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        float decay = _timeRemaining / _duration;

        if (_direction == Vector3.zero)
        {
            Offset = SampleNoiseOffset(_speed, _strength * decay);
        }
        else
        {
            Offset = _direction.normalized * (_strength * decay);
        }
    }
}
