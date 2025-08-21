    using System.Collections;
    using TMPro;
    using UnityEngine;

    public class TextScroller
    {
        public static CoroutineTask HandleScroll(TextMeshProUGUI textBox, int speed = 85, bool sound = false)
        {
            string text = textBox.text;
            AudioSource audioSource = sound? Audio.PlaySFX(SfxID.Text, true) : null;
            
            CoroutineTask scrollTask =  new CoroutineTask(ScrollText(text, textBox, speed));
            scrollTask.Finished += (bool isManual) => 
            { 
                Audio.StopSFX(audioSource);
                audioSource = null;
            }; 
            return scrollTask;
        }

        private static IEnumerator ScrollText(string line, TextMeshProUGUI textBox, int speed, CoroutineTask scrollTask = null)
        {
            textBox.text = "";  
    
            foreach (var letter in line.ToCharArray())        
            {  
                textBox.text += letter;
                yield return new WaitForSeconds(1f / speed);
            }
        }
    }
