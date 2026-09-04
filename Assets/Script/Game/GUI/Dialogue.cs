using System;
using System.Collections.Generic;
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
    private static int _frameClosedAt = -1;
    private static string _displayText;
    
    public string Text;
    public Sprite Sprite;
    /// <summary>Labelled branches. Non-empty keys are shown as a numbered choice
    /// menu; "" is reserved for "continue to the next page".</summary>
    public Dictionary<string, Dialogue> Next;
    /// <summary>One-shot side effect run when this node becomes active.</summary>
    public Action OnOpen;

    /// <summary>True while the current dialogue waits for a numbered choice.</summary>
    public static bool ChoicesActive => GetChoices(Target) != null;

    public static void Show(bool isShow)
    {
        if (isShow)
        {
            if (_frameClosedAt == Time.frameCount)
                return;

            if (!Showing)
            {
                Showing = true;
                GUIMain.Show(false);
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
                _frameClosedAt = Time.frameCount;
                GUIMain.Show(true);
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
    /// <summary>Shows a one-line event notice (e.g. the full moon rising, a
    /// bandwagon arriving) with a notification sound.</summary>
    public static void ShowEvent(string message)
    {
        Target = new Dialogue { Text = message };
        Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    /// <summary>Shows a multi-page event notice. The player advances through each
    /// page (ActionSecondary) and it closes after the last one.</summary>
    public static void ShowEventChain(params string[] messages)
    {
        Dialogue next = null;
        for (int i = messages.Length - 1; i >= 0; i--)
        {
            next = new Dialogue
            {
                Text = messages[i],
                Next = next == null ? null : new Dictionary<string, Dialogue> { { "", next } }
            };
        }
        Target = next;
        Show(true);
        Audio.PlaySFX(SfxID.Notification);
    }

    public static void Update()
    {
        if (!Showing) return;

        List<KeyValuePair<string, Dialogue>> choices = GetChoices(Target);
        bool interact = Control.Inst.ActionSecondary.KeyDown() || Control.Inst.ActionSecondaryNear.KeyDown();

        // Choice menus only reveal early on interact; a number must be pressed.
        if (choices != null)
        {
            if (_scrollTask != null && _scrollTask.Running)
            {
                if (interact)
                {
                    Audio.PlaySFX(SfxID.Text);
                    _scrollTask.Stop();
                    Main.GUIDialogueText.text = _displayText;
                }
            }
            else
                HandleChoiceKeys(choices);
            return;
        }

        if (!interact) return;
        Audio.PlaySFX(SfxID.Text);

        if (_scrollTask != null && _scrollTask.Running)
        {
            _scrollTask.Stop();
            Main.GUIDialogueText.text = _displayText;
            return;
        }

        if (Target.Next != null)
        {
            foreach (KeyValuePair<string, Dialogue> option in Target.Next)
                if (option.Key == "")
                {
                    Target = option.Value;
                    break;
                }
            SetDialogue();
        }
        else
            Show(false);
    }

    private static void SetDialogue()
    {
        SetSprite(Target.Sprite);

        List<KeyValuePair<string, Dialogue>> choices = GetChoices(Target);
        _displayText = BuildDisplayText(Target.Text, choices);
        Main.GUIDialogueText.text = _displayText;
        float mult = Settings.ScrollSpeeds[Settings.Inst.ScrollSpeedIndex];
        _scrollTask = TextScroller.HandleScroll(Main.GUIDialogueText, speed: Mathf.RoundToInt(213 * mult), sound: SfxID.Text);

        if (Target.OnOpen != null)
        {
            Action onOpen = Target.OnOpen;
            Target.OnOpen = null;
            onOpen();
        }
    }

    /// <summary>Returns the labelled branches of a node, or null for a page/node.</summary>
    private static List<KeyValuePair<string, Dialogue>> GetChoices(Dialogue d)
    {
        if (d?.Next == null) return null;
        List<KeyValuePair<string, Dialogue>> choices = null;
        foreach (KeyValuePair<string, Dialogue> option in d.Next)
            if (option.Key.Length > 0)
            {
                if (choices == null) choices = new List<KeyValuePair<string, Dialogue>>();
                choices.Add(option);
            }
        return choices;
    }

    /// <summary>The body plus numbered options, e.g. "1 > Hire for 5 gold".</summary>
    private static string BuildDisplayText(string body, List<KeyValuePair<string, Dialogue>> choices)
    {
        if (choices == null || choices.Count == 0) return body ?? "";

        List<string> lines = new List<string>();
        if (!string.IsNullOrEmpty(body)) lines.Add(body);
        for (int i = 0; i < choices.Count; i++)
            lines.Add($"{i + 1} > {choices[i].Key}");
        return string.Join("\n", lines);
    }

    private static void HandleChoiceKeys(List<KeyValuePair<string, Dialogue>> choices)
    {
        int n = 0;
        if (Input.GetKeyDown(KeyCode.Alpha1)) n = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) n = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) n = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) n = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) n = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) n = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) n = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha8)) n = 8;
        else if (Input.GetKeyDown(KeyCode.Alpha9)) n = 9;
        if (n == 0 || n > choices.Count) return;

        Audio.PlaySFX(SfxID.Text);
        Target = choices[n - 1].Value;
        SetDialogue();
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


 