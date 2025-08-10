using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GUI
{
    public bool IsHover;
} 
public class GUIMain 
{ 
    private const float ShowDuration = 0.5f;
    private const float HideDuration = 0.2f;
    
    private static CoroutineTask _showTask; 
    private static GUIStorage _inventory;
    public static GUIStorage Storage; 
    
    public static bool IsHover;
    public static void Initialize()
    {
        GUICraft.Initialize();
        GUICursor.Initialize();
        _inventory = new GUIInventory()
        {
            Storage = Inventory.Storage,
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, 166), 
            Name = "Inventory",
        };
        _inventory.Initialize();
        Storage = new GUIInventory()
        {
            Storage = Inventory.Storage,
            RowAmount = Inventory.InventoryRowAmount,
            SlotAmount = Inventory.InventorySlotAmount,
            Position = new Vector2(0, -19), 
            Name = "Storage",
        };
        Storage.Initialize();
        Storage.Show(false);
        
        GUIDialogue.Show(false);
    }
 
    public static void Update()
    {
        GUIDialogue.Update();
        GUICraft.Update(); 
        GUICursor.Update();
        _inventory.Update();
        Storage.Update();

        if (Control.Inst.Inv.KeyDown())  
        {
            if ((_showTask != null && !_showTask.Running) || _showTask == null)
            {
                if (!Game.GUIInv.activeSelf)
                { 
                    Game.GUIInv.SetActive(true); 
                    RefreshStorage();
                    _showTask = new CoroutineTask(Scale(true, ShowDuration, Game.GUIInv, 0.7f));
                    _showTask.Finished += (bool isManual) => 
                    {
                        _showTask = null;
                    };
                }
                else
                {
                    _showTask = new CoroutineTask(Scale(false, HideDuration, Game.GUIInv, 0));
                    _showTask.Finished += (bool isManual) => 
                    {
                        _showTask = null;
                        Game.GUIInv.SetActive(false);
                    };
                }
            } 
        }
    }

    public static void RefreshStorage()
    {
        _inventory.OnRefreshSlot?.Invoke(_inventory, null);
        Storage.OnRefreshSlot?.Invoke(Storage, null);
    }

    public static IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed = 75)
    {
        textBox.text = ""; 
    
        foreach (var letter in line.ToCharArray())        
        { 
            textBox.text += letter;
            yield return new WaitForSeconds(1f / speed);
        }
      
    }

    public static IEnumerator Scale(bool show, float duration, GameObject target, float scale, float easeSpeed = 0.5f)
    { 
        Vector3 initialScale = target.transform.localScale;
        Vector3 targetScale = Vector3.one * scale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            if (show)
            {
                // if (target.transform.localScale.x > 0.5f) _isInputBlocked = false;
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Apply adjustable ease-out effect
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
            }

            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.localScale = targetScale;
    }
    
    public static IEnumerator Slide(bool show, float duration, GameObject target, Vector3 position, float easeSpeed = 0.5f)
    {
        Vector3 initialPos = target.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            if (show)
            {
                t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Ease-out
            }
            else
            {
                t = Mathf.Lerp(0f, 1f, t); // Linear for hiding
            }

            target.transform.localPosition = Vector3.Lerp(initialPos, position, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.transform.localPosition = position;
    }
}
