using TMPro;
using UnityEngine;

public class GUIInfoPanel : GUI
{
    private static TextMeshProUGUI _infoText; 
    
    public new void Initialize()
    {
        _infoText = Main.GUIInfoPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>(); 
        Rect = Main.GUIInfoPanel.GetComponent<RectTransform>();
        GameObject = Main.GUIInfoPanel.gameObject;
        GameObject.AddComponent<HoverModule>().GUI = this;
        Position = new Vector2(-360, -125);
        base.Initialize();
    }
    
    public void Set(string info = null)
    {
        if (info == null)
        {
            Show(false);
            return;
        }
        Show(true);
        _infoText.text = info;
    }
}
 