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
            scrollTask.Finished += _ => 
            { 
                Audio.StopSFX(audioSource);
                textBox.text = text;
            }; 
            return scrollTask;
        }

        private static IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed, CoroutineTask scrollTask = null)
        { 
            foreach (var letter in line.ToCharArray())        
            {  
                textBox.text += letter;
                yield return new WaitForSeconds(1f / speed);
            }
        }
    }
