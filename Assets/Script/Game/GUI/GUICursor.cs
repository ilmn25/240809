using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUICursor : GUI
{
    public static ItemSlot Data = new ItemSlot();

    private static TextMeshProUGUI _infoSlotText;
    private static TextMeshProUGUI _cursorSlotText;
    private static Image _cursorSlotImage;
    private static Sprite _cursorIcon;   // the default cursor icon, shown when no item is held

    public new void Initialize()
    {
        ShowSpeed = 0.25f;
        HideSpeed = 0.1f;
        
        Rect = Main.GUICursor.GetComponent<RectTransform>();
        GameObject = Main.GUICursorInfo;
        base.Initialize();        
        _infoSlotText = Main.GUICursorInfo.transform.Find("Info").GetComponent<TextMeshProUGUI>(); 
        Text = Main.GUICursorInfo.transform.Find("Text").GetComponent<TextMeshProUGUI>(); 
        _cursorSlotText = Main.GUICursorSlot.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        _cursorSlotImage = Main.GUICursorSlot.transform.Find("Image").GetComponent<Image>();
        _cursorIcon = _cursorSlotImage.sprite; // the slot's default image is the cursor icon
    }

    public void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, Input.mousePosition,  
            Main.GUICamera,out Vector2 mousePosition);
        Rect.anchoredPosition = mousePosition;
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

    public void SetItemSlotInfo(ItemSlot item = null, bool ingredient = false)
    {
        if (item == null)
        {
            Show(false);
            return;
        }
        Text.text = item.Info.Name;
        _infoSlotText.text = item.ToString(ingredient);
        Show(true); 
    } 
    
    public void Set(string title = "", string description = "")
    {
        if (title == "")
        {
            Show(false);
            return;
        }
        Text.text = title;
        _infoSlotText.text = description;
        Show(true); 
    } 
    
    public static void UpdateCursorSlot()
    { 
        // Always show the cursor. Swap its icon to the held item when there is one.
        Main.GUICursorSlot.SetActive(true);
        if (Data.Stack == 0)
        {
            _cursorSlotImage.sprite = _cursorIcon;
            _cursorSlotText.text = "";
        }
        else
        {
            _cursorSlotImage.sprite = Resources.Load<Sprite>($"Texture/Sprite/{Data.ID}");
            _cursorSlotText.text = Data.Stack.ToString();
        } 
    } 
}
