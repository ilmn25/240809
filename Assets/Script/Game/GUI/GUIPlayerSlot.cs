using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>A single player entry in the player list. Shows the player's
/// sprite and current HP, pops up the info panel on hover, and takes control
/// of the player on click.</summary>
public class GUIPlayerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image _image;
    private TextMeshProUGUI _text;
    public PlayerInfo Player;
    public GUIPlayerList GUIPlayerList;

    private void Start()
    {
        _image = transform.Find("Image").GetComponent<Image>();
        _text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void Set(PlayerInfo player)
    {
        Player = player;
        if (player == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        _image.sprite = Cache.LoadSprite("Sprite/" + player.CharSprite);
        _image.color = Color.white;
        _text.text = player.Health.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GUIMain.Showing || !GUIPlayerList.Showing) return;
        if (Player == null) return;
        Audio.PlaySFX(SfxID.Text);
        GUIPlayerList.ShowInfo(Player);
        ScaleSlot(1.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GUIMain.Showing || !GUIPlayerList.Showing) return;
        GUIPlayerList.HideInfo();
        ScaleSlot(1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GUIMain.Showing || !GUIPlayerList.Showing) return;
        if (Player == null) return;
        if (GUIPlayerList.TryControl(Player))
            GUIPlayerList.ShowInfo(Player); // refresh while still hovering
    }

    private void ScaleSlot(float scale)
    {
        _image.rectTransform.localScale = Vector3.one * scale;
    }
}
