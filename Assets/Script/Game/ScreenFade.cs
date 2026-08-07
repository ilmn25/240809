using UnityEngine;

/// <summary>
/// Independent screen-fade overlay, completely decoupled from the
/// Environment/weather lighting system. Uses a full-screen black
/// texture whose alpha is animated by FadeIn / FadeOut.
///
/// Call ScreenFade.Update() every frame (done in Main.Update) and
/// ScreenFade.OnGUI() to render the overlay (done in Main.OnGUI).
/// </summary>
public static class ScreenFade
{
    private static float _alpha = 0f;        // 0 = transparent, 1 = fully black
    private static float _targetAlpha = 0f;
    private static float _fadeSpeed = 0f;    // alpha units per second
    private static Texture2D _blackTexture;

    /// <summary>Current overlay alpha (0 = transparent, 1 = black).</summary>
    public static float Alpha => _alpha;

    /// <summary>True while a fade animation is in progress.</summary>
    public static bool IsFading => _alpha != _targetAlpha;

    private static void EnsureTexture()
    {
        if (_blackTexture == null)
        {
            _blackTexture = new Texture2D(1, 1);
            _blackTexture.SetPixel(0, 0, Color.black);
            _blackTexture.Apply();
        }
    }

    /// <summary>Fade from current alpha to 1 (fully black).</summary>
    public static void FadeOut(float duration = 0.5f)
    {
        _targetAlpha = 1f;
        _fadeSpeed = duration > 0.001f ? 1f / duration : 10f;
    }

    /// <summary>Fade from current alpha to 0 (fully transparent).</summary>
    public static void FadeIn(float duration = 0.5f)
    {
        _targetAlpha = 0f;
        _fadeSpeed = duration > 0.001f ? 1f / duration : 10f;
    }

    /// <summary>Start fully black and fade in to transparent (e.g. on app launch).</summary>
    public static void FadeInFromBlack(float duration = 0.5f)
    {
        _alpha = 1f;
        FadeIn(duration);
    }

    /// <summary>Call every frame from Main.Update() to animate the alpha.</summary>
    public static void Update()
    {
        if (_alpha == _targetAlpha) return;

        float step = _fadeSpeed * Time.deltaTime;
        if (_alpha < _targetAlpha)
        {
            _alpha += step;
            if (_alpha > _targetAlpha) _alpha = _targetAlpha;
        }
        else
        {
            _alpha -= step;
            if (_alpha < _targetAlpha) _alpha = _targetAlpha;
        }
    }

    /// <summary>Call from Main.OnGUI() to render the black overlay.</summary>
    public static void OnGUI()
    {
        if (_alpha <= 0.001f) return;
        EnsureTexture();
        Color prev = UnityEngine.GUI.color;
        UnityEngine.GUI.color = new Color(0, 0, 0, _alpha);
        UnityEngine.GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _blackTexture);
        UnityEngine.GUI.color = prev;
    }
}
