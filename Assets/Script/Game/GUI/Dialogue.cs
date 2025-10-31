using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dialogue 
{  
    private const float EaseSpeed = 0.4f;
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;

    private static CoroutineTask _scrollTask;
    public static Dialogue Target;
    public static bool Showing = true;
    private static CoroutineTask _scaleTask;  
    
    public string Text;
    public Sprite Sprite;
    public Dictionary<string, Dialogue> Next;
    public static void Show(bool isShow)
    {
        if (isShow)
        {
            if (!Showing)
            {
                Showing = true;
                SetDialogue();  
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(true, ShowDuration, Main.GUIDialogue, 
                    0.9f, EaseSpeed)); 
                Main.GUIDialogue.SetActive(true);
            }
        }
        else
        {
            if (Showing)
            { 
                Showing = false;
                SetSprite();
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(false, HideDuration, Main.GUIDialogue, 
                    0, EaseSpeed));
                _scaleTask.Finished += _ =>
                {
                    if (_scrollTask != null && _scrollTask.Running) _scrollTask.Stop();
                    Main.GUIDialogue.SetActive(false); 
                }; 
            }
        }
    }
    public static void Update()
    { 
        if (Showing){  
            
            if (Control.Inst.ActionSecondary.KeyDown() || Control.Inst.ActionSecondaryNear.KeyDown())
            { 
                Audio.PlaySFX(SfxID.Text); 
                
                if (_scrollTask.Running) _scrollTask.Stop(); 
                else
                {
                    if (Target.Next != null)
                    {
                        foreach (KeyValuePair<string, Dialogue> option in Target.Next)
                            if (option.Key == "") Target = option.Value;
                        SetDialogue();
                    }
                    else
                        Show(false);
                }
            }
        }
    }
    
    private static void SetDialogue()
    {
        SetSprite(Target.Sprite);
        Main.GUIDialogueText.text = Target.Text;
        _scrollTask = TextScroller.HandleScroll(Main.GUIDialogueText, sound: SfxID.Text);
    }

    private static void SetSprite(Sprite sprite = null)
    {
        if (sprite)
        {  
            Main.GUIImageRenderer.sprite = Target.Sprite; 
            if (Main.GUIImageRenderer.transform.position != new Vector3(220, -95, 203))
                _ = new CoroutineTask(GUIMain.Slide(true, 0.2f, Main.GUIImage, 
                    new Vector3(220, -95, 160), EaseSpeed)); 
        }
        else
        {
            if (Main.GUIImageRenderer.transform.position != new Vector3(500, -95, 203))
                _ = new CoroutineTask(GUIMain.Slide(false, 0.1f, Main.GUIImage, 
                    new Vector3(500, -95, 160), EaseSpeed));
        }
    }
}


 