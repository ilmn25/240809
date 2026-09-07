using UnityEngine;

public static class ScreenDamageFlash
{
    private static float _alpha;
    private static float _targetAlpha;
    private static float _fadeSpeed;
    private static Texture2D _texture;

    public static void Initialize()
    {
        _texture = new Texture2D(1, 1);
        _texture.SetPixel(0, 0, Color.white);
        _texture.Apply();
    }

    public static void Show(float alpha = 0.28f, float duration = 0.2f)
    {
        _alpha = Mathf.Max(_alpha, alpha);
        _targetAlpha = 0f;
        _fadeSpeed = _alpha / Mathf.Max(0.001f, duration);
    }

    public static void Update()
    {
        _alpha = Mathf.MoveTowards(_alpha, _targetAlpha, _fadeSpeed * Time.deltaTime);
    }

    public static void OnGUI()
    {
        if (_alpha <= 0.001f || _texture == null) return;

        Color previousColor = UnityEngine.GUI.color;
        UnityEngine.GUI.color = new Color(0.55f, 0f, 0f, _alpha);
        UnityEngine.GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _texture);
        UnityEngine.GUI.color = previousColor;
    }
}