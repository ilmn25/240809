using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Don't Starve-style 2D map panel. Shows the current world's top-down terrain
/// texture (with fog of war) plus a player marker. Toggled with the Map key (M).
/// Full screen, with drag-to-pan and scroll-to-zoom.
/// </summary>
public class GUIMap : GUI
{
    private RawImage _mapImage;
    private RectTransform _mapRect;
    private RectTransform _playerMarker;
    private RectTransform _panelRect;

    private float _zoom = 1f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 6f;
    private const float ZoomSpeed = 0.12f;

    private bool _dragging;
    private Vector2 _dragStartMouse;
    private Vector2 _dragStartPan;

    /// <summary>Whether the map is currently open (used to block game input).</summary>
    public bool IsOpen => Showing;

    // The GUI canvas is a world-space canvas (scale 0.04) with a reference
    // resolution of 800x600. Its root has SizeDelta 0x0, so we give the map
    // panel an explicit size and scale it up to fill the screen.
    private const float PanelWidth = 800f;
    private const float PanelHeight = 600f;
    private const float PanelScale = 8f;

    public new void Initialize()
    {
        // Parent under the GUI canvas root (world-space canvas, ref res 800x600).
        GameObject = new GameObject("GUIMap", typeof(RectTransform));
        GameObject.transform.SetParent(Main.GUIObject.transform, false);
        Rect = GameObject.GetComponent<RectTransform>();
        Rect.localScale = Vector3.one * PanelScale;
        Rect.localRotation = Quaternion.identity;
        Rect.anchorMin = new Vector2(0.5f, 0.5f);
        Rect.anchorMax = new Vector2(0.5f, 0.5f);
        Rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        Rect.anchoredPosition = Vector2.zero;
        Position = Vector2.zero;
        base.Initialize();

        // Full-screen background panel with a mask so the map is clipped to it.
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        panel.transform.SetParent(GameObject.transform, false);
        _panelRect = panel.GetComponent<RectTransform>();
        _panelRect.anchorMin = Vector2.zero;
        _panelRect.anchorMax = Vector2.one;
        _panelRect.offsetMin = Vector2.zero;
        _panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.07f, 1f);

        // Map texture image (child of panel; panned and zoomed).
        GameObject mapObj = new GameObject("MapImage", typeof(RectTransform), typeof(RawImage));
        mapObj.transform.SetParent(panel.transform, false);
        _mapRect = mapObj.GetComponent<RectTransform>();
        _mapRect.anchoredPosition = Vector2.zero;
        _mapImage = mapObj.GetComponent<RawImage>();

        // Player marker (child of the map image so it pans/zooms with it).
        GameObject marker = new GameObject("PlayerMarker", typeof(RectTransform), typeof(Image));
        marker.transform.SetParent(mapObj.transform, false);
        _playerMarker = marker.GetComponent<RectTransform>();
        _playerMarker.sizeDelta = new Vector2(8, 8);
        Image markerImage = marker.GetComponent<Image>();
        markerImage.color = Color.white;

        Show(false);
    }

    public void Update()
    {
        if (!Showing || World.Inst?.Map == null) return;

        WorldMap map = World.Inst.Map;
        if (map.Dirty)
        {
            map.BuildMarkers(World.Inst);
            map.RegenerateTexture(World.Inst);
        }
        if (map.Texture != null)
        {
            _mapImage.texture = map.Texture;

            // Size the map image to the world's aspect ratio, fitting within the
            // panel at zoom 1.
            Vector3Int bounds = World.Inst.Bounds;
            float aspect = bounds.x / (float)Mathf.Max(1, bounds.z);
            Vector2 panelSize = _panelRect.rect.size;
            float baseSize = Mathf.Min(panelSize.x, panelSize.y) * 0.9f;
            _mapRect.sizeDelta = new Vector2(baseSize * aspect, baseSize);
        }

        HandleInput();

        // Position the player marker on the map (in map image local space).
        if (Main.Player != null)
        {
            Vector3 p = Main.Player.transform.position;
            Vector3Int bounds = World.Inst.Bounds;
            float u = bounds.x > 0 ? p.x / bounds.x : 0f;
            float v = bounds.z > 0 ? p.z / bounds.z : 0f;
            Vector2 size = _mapRect.sizeDelta;
            _playerMarker.anchoredPosition = new Vector2(
                (u - 0.5f) * size.x,
                (v - 0.5f) * size.y);
        }
    }

    private void HandleInput()
    {
        // Zoom with the scroll wheel.
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _zoom = Mathf.Clamp(_zoom + scroll * ZoomSpeed, MinZoom, MaxZoom);
            _mapRect.localScale = Vector3.one * _zoom;
        }

        // Drag to pan.
        if (Input.GetMouseButtonDown(0))
        {
            _dragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _panelRect, Input.mousePosition, Main.GUICamera, out _dragStartMouse);
            _dragStartPan = _mapRect.anchoredPosition;
        }
        if (_dragging && Input.GetMouseButton(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _panelRect, Input.mousePosition, Main.GUICamera, out Vector2 mouse);
            _mapRect.anchoredPosition = _dragStartPan + (mouse - _dragStartMouse);
        }
        if (Input.GetMouseButtonUp(0))
        {
            _dragging = false;
        }
    }
}
