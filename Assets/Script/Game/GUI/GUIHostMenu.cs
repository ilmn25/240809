using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

public class GUIHostMenu : GUI
{
    public GUIHostMenu()
    {
        Text = Main.GUIHostMenu.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        GameObject = Main.GUIHostMenu;
    }

    public void Update()
    {
        // Don't process menu clicks while connected or hosting
        if (NetworkClient.isConnected || NetworkServer.active) return;

        if (Control.Inst.Pause.KeyDown() && Showing)
        {
            Audio.PlaySFX(SfxID.Text);
            Show(false);
            GUIMain.GUIMenu.Show(true);
            return;
        }

        if (!Control.Inst.ActionPrimary.KeyDown()) return;
        if (Main.GUIHostMenuButtonNew.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            ScreenFade.FadeOut(0.3f);
            Save.Inst = new Save(GenType.Abyss);
            _ = new CoroutineTask(Server.StartHost());
            Show(false);
        }
        else if (Main.GUIHostMenuButtonLoad.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            GUIMain.GUILoad.Show(true);
            Show(false);
        }
        else if (Main.GUIHostMenuButtonBack.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            Show(false);
            GUIMain.GUIMenu.Show(true);
        }
    }
}
