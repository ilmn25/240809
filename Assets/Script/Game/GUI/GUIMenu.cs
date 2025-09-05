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
            Save.NewSave("Game", GenType.Abyss); 
            Save.LoadSave("Game");
            Scene.LoadWorld();
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
public class GUILoad : GUI
{
    public static string Target;
    public GUILoad()
    { 
        GameObject = Game.GUILoadMenu;
    }
    public static void RefreshList()
    {
        int i = 0;
        foreach (string save in Save.Inst.List.Keys)
        { 
            GameObject obj = Object.Instantiate(Resources.Load<GameObject>("Prefab/GUISave"), Game.GUILoadMenu.transform);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, i * -86, 130);
            obj.GetComponent<GUISave>().ID = save;
            i++;
        }
    }
    
    public void Update()
    {
        if (Control.Inst.Pause.KeyDown() && Showing)
        {
            Show(false);
            GUIMain.GUIMenu.Show(true);
        }
        if (Control.Inst.ActionPrimary.KeyDown() && Target != null && Game.SceneMode == SceneMode.Menu)
        {
            Save.LoadSave(Target);
            Scene.LoadWorld();
            Show(false); 
        } 
    }
}