using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GUIMenu : GUI
{ 
    public GUIMenu()
    { 
        Text = Main.GUIMainMenu.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        GameObject = Main.GUIMainMenu;
    }
    public void Update()
    {
        // Don't process menu clicks while connected or hosting
        if (NetworkClient.isConnected || NetworkServer.active) return;

        if (!Control.Inst.ActionPrimary.KeyDown()) return;
        if (Main.GUIMainMenuButtonHost.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            Show(false);
            GUIMain.GUIHostMenu.Show(true);
        }
        else if (Main.GUIMainMenuButtonJoin.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            _ = new CoroutineTask(Server.StartClient());
            Show(false);
        }
        else if (Main.GUIMainMenuButtonExit.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            _ = new CoroutineTask(Quit());
            IEnumerator Quit()
            {
                ScreenFade.FadeOut(0.5f);
                yield return new WaitForSeconds(0.6f);
                Application.Quit();
            }
        }
    }
}