    using System.Collections;
    using TMPro;
    using UnityEngine;

    public class TextScroller
    {
        public static CoroutineTask HandleScroll(TextMeshProUGUI textBox, int speed = 85, SfxID sound = SfxID.Null)
        { 
            string text = textBox.text;
            textBox.text = "";  
            AudioSource audioSource = sound != SfxID.Null? Audio.PlaySFX(sound, true) : null;
            
            CoroutineTask scrollTask =  new CoroutineTask(ScrollText(text, textBox, speed));
            scrollTask.Finished += (bool manual) => 
            { 
                Audio.StopSFX(audioSource);
                // Only restore the full text on natural completion. On a manual
                // stop a new scroll is replacing this one, so writing the stale
                // text back would double up characters.
                if (!manual) textBox.text = text;
            }; 
            return scrollTask;
            
            IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed, CoroutineTask scrollTask = null)
            { 
                float progress = 0;
                int shown = 0;
                while (shown < line.Length)
                {
                    progress += Time.unscaledDeltaTime * speed;
                    int target = Mathf.Min(line.Length, Mathf.FloorToInt(progress));
                    if (target > shown)
                    {
                        textBox.text = line.Substring(0, target);
                        shown = target;
                    }

                    yield return null;
                }
            }
        }
 
    }
