
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIHealthBar
{
        private static readonly List<Image> HealthBar = new List<Image>();
        public static void Initialize()
        {
                GameObject healthBar = new GameObject("HealthBar", typeof(RectTransform));
                RectTransform rectTransform = healthBar.GetComponent<RectTransform>();

                rectTransform.SetParent(Game.GUIObject.transform, false);  
                rectTransform.localPosition = new Vector3(-326, 178, 203); 
                rectTransform.localScale = Vector3.one * 0.6f;

                GameObject heart;
                for (int i = 0; i < PlayerData.Inst.health / 2; i++)
                {
                        heart = Object.Instantiate(Resources.Load<GameObject>("prefab/gui_heart"), rectTransform);
                        RectTransform heartRect = heart.GetComponent<RectTransform>();

                        // Set anchored position relative to parent
                        heartRect.anchoredPosition = new Vector2(i * 26, 0);
                        heartRect.localScale = Vector3.one;

                        HealthBar.Add(heart.GetComponent<Image>());
                }
        }

        public static void Update()
        {
                for (int i = 0; i < HealthBar.Count; i += 1)
                {
                        int target = i * 2 + 2;
                        if (target < Game.PlayerInfo.Health)
                        {
                                HealthBar[i].sprite = Cache.LoadSprite("sprite/heartfull");
                        }
                        else if (target - 1 < Game.PlayerInfo.Health)
                        {
                                HealthBar[i].sprite = Cache.LoadSprite("sprite/hearthalf");
                        }
                        else
                        {
                                HealthBar[i].sprite = Cache.LoadSprite("sprite/heartempty");
                        }
                }
        }
}