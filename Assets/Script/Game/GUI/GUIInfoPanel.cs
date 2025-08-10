using TMPro;
using UnityEngine;

public class GUIInfoPanel : GUI
{
    private static TextMeshProUGUI _infoText; 
    
    public new void Initialize()
    {
        _infoText = Game.GUIInfoPanel.transform.Find("text").GetComponent<TextMeshProUGUI>(); 
        Rect = Game.GUIInfoPanel.GetComponent<RectTransform>();
        GameObject = Game.GUIInfoPanel.gameObject;
        GameObject.AddComponent<HoverModule>().GUI = this;
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
 