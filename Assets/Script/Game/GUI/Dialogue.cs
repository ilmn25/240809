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
    public Dictionary<string, Dialogue> Option;
    public static void Show(bool isShow)
    {
        if (isShow)
        {
            if (!Showing)
            {
                Showing = true;
                Scroll(); 
                _ = new CoroutineTask(GUIMain.Slide(true, 0.2f, Game.GUIImage, 
                    new Vector3(220, -95, 203), EaseSpeed));
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(true, ShowDuration, Game.GUIDialogue, 
                    0.9f, EaseSpeed)); 
                Game.GUIDialogue.SetActive(true);
            }
        }
        else
        {
            if (Showing)
            { 
                Showing = false;
                new CoroutineTask(GUIMain.Slide(false, 0.1f, Game.GUIImage, 
                    new Vector3(500, -95, 203), EaseSpeed));
                _scaleTask?.Stop();
                _scaleTask = new CoroutineTask(GUIMain.Scale(false, HideDuration, Game.GUIDialogue, 
                    0, EaseSpeed));
                _scaleTask.Finished += _ =>
                {
                    if (_scrollTask != null && _scrollTask.Running) _scrollTask.Stop();
                    Game.GUIDialogue.SetActive(false); 
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
                
                if (_scrollTask.Running)
                {
                    _scrollTask.Stop(); 
                }                  
                else
                {
                    if (Target.Option != null)
                    {
                        foreach (KeyValuePair<string, Dialogue> option in Target.Option)
                        {
                            if (option.Key == "") Target = option.Value;
                        }
                        Scroll();
                    }
                    else
                    {
                        Show(false);
                    } 
                }
            }
        }
    }
    
    private static void Scroll()
    {
        if (Target.Sprite)
        {
            Game.GUIImage.SetActive(true);
            Game.GUIImageRenderer.sprite = Target.Sprite;
        }
        else Game.GUIImage.SetActive(false);
        
        Game.GUIDialogueText.text = Target.Text;
        _scrollTask = TextScroller.HandleScroll(Game.GUIDialogueText, sound: SfxID.Text);
    }
}


 