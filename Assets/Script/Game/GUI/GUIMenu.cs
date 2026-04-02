using System.Collections;
using System.Collections.Generic;
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
        if (!Control.Inst.ActionPrimary.KeyDown()) return;
        if (Main.GUIMainMenuButtonNew.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            Save.NewSave(GenType.Abyss);  
            Show(false);
        }
        else if (Main.GUIMainMenuButtonLoad.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            GUIMain.GUILoad.Show(true);
            Show(false);
        }
        else if (Main.GUIMainMenuButtonExit.IsHovered)
        {
            Audio.PlaySFX(SfxID.Text);
            _ = new CoroutineTask(Quit());
            IEnumerator Quit()
            {
                Environment.Target = EnvironmentType.Black;
                yield return new WaitForSeconds(3);
                Application.Quit();
            }
        }
    }
}