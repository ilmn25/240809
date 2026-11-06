using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor : GUI
{
    public static ItemSlot Data = new ItemSlot();

    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;
    private static Sprite _aimIcon;      // default cursor icon when nothing is held / hovered
    private static Sprite _interactIcon; // shown when hovering an interactable/pickupable target

    // Cached held-item sprite + id so the per-frame refresh doesn't reload it every frame.
    private static Sprite _itemSprite;
    private static ID _itemSpriteId;

    public new void Initialize()
    {
        Rect = Main.GUICursor.GetComponent<RectTransform>();
        base.Initialize();        
        _cursorSlotText = Main.GUICursorSlot.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Main.GUICursorSlot.transform.Find("Image").GetComponent<Image>();
        // The slot's default image is the aim cursor; fall back to it if a resource is missing.
        _aimIcon = Resources.Load<Sprite>("Texture/GUI/Cursor/Aim") ?? _cursorSlotImage.sprite;
        _interactIcon = Resources.Load<Sprite>("Texture/GUI/Cursor/Interact") ?? _aimIcon;
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
        // Always show the cursor. A held item takes the icon over, otherwise swap
        // between the aim and interact icons based on what's under the cursor: an
        // interactable/pickupable world target, or an interactive GUI element
        // (storage slot, button, draggable window).
        Main.GUICursorSlot.SetActive(true);
        if (Data.Stack == 0)
        {
            bool overGui = GUIMain.IsHover || GUIStorage.HoveringSlot;
            SetCursorIcon(Control.HoverInteract || overGui ? _interactIcon : _aimIcon, "");
        }
        else
        {
            SetCursorIcon(HeldItemIcon(Data.ID), Data.Stack.ToString());
        } 
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
