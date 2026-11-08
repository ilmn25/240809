using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor : GUI
{
    public static ItemSlot Data = new ItemSlot();

    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;
    private static Sprite _defaultIcon;  // plain pointer when nothing is held / hovered
    private static Sprite _interactIcon; // shown when hovering an interactable/pickupable target
    private static Sprite _targetIcon;   // shown when the cursor is over an entity

    // Cached held-item sprite + id so the per-frame refresh doesn't reload it every frame.
    private static Sprite _itemSprite;
    private static ID _itemSpriteId;

    public new void Initialize()
    {
        Rect = Main.GUICursor.GetComponent<RectTransform>();
        base.Initialize();        
        _cursorSlotText = Main.GUICursorSlot.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Main.GUICursorSlot.transform.Find("Image").GetComponent<Image>();
        // The slot's default image is the pointer; fall back to it if a resource is missing.
        _defaultIcon = Resources.Load<Sprite>("Texture/GUI/Cursor/Default") ?? _cursorSlotImage.sprite;
        _interactIcon = Resources.Load<Sprite>("Texture/GUI/Cursor/Interact") ?? _defaultIcon;
        _targetIcon = Resources.Load<Sprite>("Texture/GUI/Cursor/Target") ?? _defaultIcon;
    }

    public void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, Input.mousePosition,  
            Main.GUICamera,out Vector2 mousePosition);
        Rect.anchoredPosition = mousePosition;
        // Refresh every frame so the icon tracks what's under the cursor (hover swap).
        UpdateCursorSlot();
    }

    /// <summary>Quick-actions while the inventory is open. The cursor item is the held
    /// item (see Inventory.SyncCurrentItemState), so the normal place/use paths already
    /// work from the cursor; here we only handle dropping the held item.</summary>
    public void HandleInteraction()
    {
        if (Data.isEmpty()) return;

        if (Control.Inst.ActionSecondary.KeyDown() && !Input.GetKey(KeyCode.LeftShift))
            DropToWorld();
    }

    private static void DropToWorld()
    {
        Inventory.DropToWorld(Data, Data.Stack, Main.PlayerInfo.Storage, Main.Player.transform.position);
        Audio.PlaySFX(SfxID.Item);
        UpdateCursorSlot();
    }

    public static void UpdateCursorSlot()
    { 
        // Always show the cursor. A held item takes the icon over, otherwise pick the
        // target reticle over any entity, the interact icon over an
        // interactable/pickupable world target or interactive GUI element (storage slot,
        // button, draggable window), and the plain pointer everywhere else.
        Main.GUICursorSlot.SetActive(true);
        if (Data.Stack == 0)
        {
            SetCursorIcon(NothingHeldIcon(), "");
        }
        else
        {
            SetCursorIcon(HeldItemIcon(Data.ID), Data.Stack.ToString());
        } 
    }

    /// <summary>The icon for a cursor holding nothing: target reticle over an entity,
    /// otherwise the interact icon over anything interactive, otherwise the plain pointer.</summary>
    private static Sprite NothingHeldIcon()
    {
        if (Control.HoverEntity) return _targetIcon;
        bool overGui = GUIMain.IsHover || GUIStorage.HoveringSlot;
        return Control.HoverInteract || overGui ? _interactIcon : _defaultIcon;
    }

    /// <summary>The held item's sprite, loaded once per distinct item id.</summary>
    private static Sprite HeldItemIcon(ID id)
    {
        if (_itemSpriteId != id)
        {
            _itemSpriteId = id;
            _itemSprite = Resources.Load<Sprite>($"Texture/Sprite/{id}");
        }

        return _itemSprite;
    }

    /// <summary>Applies a cursor icon/text only when it actually changes, so the
    /// per-frame hover refresh doesn't dirty the image every single frame.</summary>
    private static void SetCursorIcon(Sprite sprite, string text)
    {
        if (_cursorSlotImage.sprite == sprite && _cursorSlotText.text == text) return;
        _cursorSlotImage.sprite = sprite;
        _cursorSlotText.text = text;
    }
}
