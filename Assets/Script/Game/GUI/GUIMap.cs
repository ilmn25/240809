using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Don't Starve-style 2D map panel. Shows the current world's top-down terrain
/// texture (with fog of war) plus structure icons and a player marker. Toggled
/// with the Map key (M). Full screen, with drag-to-pan and scroll-to-zoom.
/// </summary>
public class GUIMap : GUI
{
    private RawImage _mapImage;
    private RectTransform _mapRect;
    private RectTransform _panelRect;
    private readonly Dictionary<MapMarker, Image> _icons = new Dictionary<MapMarker, Image>();
    private readonly List<RectTransform> _playerMarkers = new List<RectTransform>();
    private readonly Dictionary<ID, Sprite> _spriteCache = new Dictionary<ID, Sprite>();

    private float _zoom = 1.5f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 6f;
    private const float ZoomSpeed = 0.12f;
    /// <summary>How many map pixels each sprite pixel occupies, so pixel-art
    /// sprites keep consistent density relative to each other.</summary>
    private const float PixelsPerSpritePixel = 0.2f;

    private bool _dragging;
    private Vector2 _dragStartMouse;
    private Vector2 _dragStartPan;
    private bool _needsFocus;

    /// <summary>Whether the map is currently open (used to block game input).</summary>
    public bool IsOpen => Showing;

    /// <summary>Centers the map on the player the next time it's updated.</summary>
    public void FocusOnPlayer() => _needsFocus = true;

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

        // Smoothly rotate the map to match the camera's orbit (like the camera lerp).
        Quaternion targetRotation = Quaternion.Euler(0, 0, ViewPort.OrbitRotation);
        _mapRect.localRotation = Quaternion.Lerp(_mapRect.localRotation, targetRotation, Time.deltaTime * 7f);

        // Refocus the map onto the player when it's opened.
        if (_needsFocus && Main.Player != null)
        {
            Vector3Int bounds = World.Inst.Bounds;
            Vector2 size = _mapRect.sizeDelta;
            Vector3 p = Main.Player.transform.position;
            _mapRect.anchoredPosition = new Vector2(
                (0.5f - p.x / bounds.x) * size.x,
                (0.5f - p.z / bounds.z) * size.y);
            _needsFocus = false;
        }

        HandleInput();

        UpdatePlayerMarkers();
        UpdateIcons(map);
    }

    /// <summary>Positions a marker for every player on the map.</summary>
    private void UpdatePlayerMarkers()
    {
        if (Save.Inst == null) return;

        int count = 0;
        foreach (PlayerInfo player in Save.Inst.players)
        {
            if (player == null || player.Machine == null) continue;

            RectTransform marker;
            if (count < _playerMarkers.Count)
            {
                marker = _playerMarkers[count];
            }
            else
            {
                GameObject markerObj = new GameObject("PlayerMarker", typeof(RectTransform), typeof(Image));
                markerObj.transform.SetParent(_mapRect, false);
                marker = markerObj.GetComponent<RectTransform>();
                marker.GetComponent<Image>().color = Color.white;
                _playerMarkers.Add(marker);
            }

            // Use the player's character sprite so each player is distinct.
            Image img = marker.GetComponent<Image>();
            img.sprite = GetSprite(player.CharSprite);

            Vector3 p = player.Machine.transform.position;
            PlaceAndSize(marker, img.sprite, p.x, p.z);
            // Render players above structure icons.
            marker.SetAsLastSibling();
            marker.gameObject.SetActive(true);
            count++;
        }

        // Hide any unused player markers.
        for (int i = count; i < _playerMarkers.Count; i++)
            _playerMarkers[i].gameObject.SetActive(false);
    }

    /// <summary>Positions structure icons over the map. One icon is created per
    /// marker (lazily) and cached, so there is no fixed pool limit.</summary>
    private void UpdateIcons(WorldMap map)
    {
        Vector3Int bounds = World.Inst.Bounds;

        foreach (MapMarker m in map.Markers)
        {
            if (m.x < 0 || m.x >= bounds.x || m.z < 0 || m.z >= bounds.z) continue;

            // Only show icons in explored columns.
            int idx = m.z * bounds.x + m.x;
            if (map.Explored == null || map.Explored[idx] == 0) continue;

            if (!_icons.TryGetValue(m, out Image icon))
            {
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(_mapRect, false);
                icon = iconObj.GetComponent<Image>();
                icon.color = Color.white;
                _icons[m] = icon;
            }

            icon.enabled = true;
            icon.sprite = GetSprite(m.id);
            PlaceAndSize(icon.rectTransform, icon.sprite, m.x, m.z);
        }
    }

    /// <summary>Positions a marker/icon at a world column and sizes it to the
    /// sprite's pixel dimensions (consistent pixel density). Also counter-rotates
    /// it so it stays upright regardless of map rotation.</summary>
    private void PlaceAndSize(RectTransform rt, Sprite sprite, float worldX, float worldZ)
    {
        Vector3Int bounds = World.Inst.Bounds;
        Vector2 size = _mapRect.sizeDelta;
        float pixelsPerColumn = size.x / (float)Mathf.Max(1, bounds.x);

        rt.anchoredPosition = new Vector2(
            ((worldX / bounds.x) - 0.5f) * size.x,
            ((worldZ / bounds.z) - 0.5f) * size.y);

        // Scale each sprite by a constant pixels-per-sprite-pixel factor, so
        // pixel-art sprites keep the same pixel density relative to each other
        // (a 32x32 sprite renders twice as large as a 16x16 one).
        if (sprite != null && sprite.rect.width > 0 && sprite.rect.height > 0)
        {
            float scale = PixelsPerSpritePixel * pixelsPerColumn;
            rt.sizeDelta = new Vector2(sprite.rect.width * scale, sprite.rect.height * scale);
        }
        else
        {
            rt.sizeDelta = Vector2.zero;
        }

        // Counter-rotate so the marker stays upright regardless of map rotation.
        rt.localRotation = Quaternion.Euler(0, 0, -ViewPort.OrbitRotation);
    }

    /// <summary>Caches and returns the sprite for an ID.</summary>
    private Sprite GetSprite(ID id)
    {
        if (!_spriteCache.TryGetValue(id, out Sprite sprite))
        {
            sprite = Cache.LoadSprite("Sprite/" + id);
            _spriteCache[id] = sprite;
        }
        return sprite;
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
