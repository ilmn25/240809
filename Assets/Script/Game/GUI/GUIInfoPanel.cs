using TMPro;
using UnityEngine;

public class GUIInfoPanel : GUI
{
    private static TextMeshProUGUI _infoText; 
    
    public new void Initialize()
    {
        _infoText = Game.GUIInfoPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>(); 
        Rect = Game.GUIInfoPanel.GetComponent<RectTransform>();
        GameObject = Game.GUIInfoPanel.gameObject;
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
 