using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIMenu : GUI
{ 
    public GUIMenu()
    { 
        Text = Game.GUIMainMenu.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        GameObject = Game.GUIMainMenu;
    }
    public void Update()
    {
        if (!Control.Inst.ActionPrimary.KeyDown()) return;
        if (Game.GUIMainMenuButtonNew.IsHovered)
        {
            Save.NewSave(GenType.Abyss);  
            Show(false);
        }
        else if (Game.GUIMainMenuButtonLoad.IsHovered)
        {
            GUIMain.GUILoad.Show(true);
            Show(false);
        }
        else if (Game.GUIMainMenuButtonExit.IsHovered)
        {
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