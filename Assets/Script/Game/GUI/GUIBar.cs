
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIBar
{
        protected readonly List<Image> Bar = new List<Image>();
        private static readonly GUIBar HealthBar = new GUIHealthBar(new Vector3Int(-326, 178, 203));
        private static readonly GUIBar HungerBar = new GUIHungerBar(new Vector3Int(-326, 162, 203));

        protected GUIBar(Vector3Int position)
        {
                GameObject obj = new GameObject("Bar", typeof(RectTransform));
                RectTransform rectTransform = obj.GetComponent<RectTransform>();

                rectTransform.SetParent(Main.GUIObject.transform, false);
                rectTransform.localPosition = position;
                rectTransform.localScale = Vector3.one * 0.6f;

                GameObject iconObj;
                RectTransform iconRect;
                for (int i = 0; i < 10; i++)
                {
                        iconObj = Object.Instantiate(Resources.Load<GameObject>("Prefab/GUIIcon"), rectTransform);
                        iconRect = iconObj.GetComponent<RectTransform>();

                        // Set anchored position relative to parent
                        iconRect.anchoredPosition = new Vector2(i * 26, 0);
                        iconRect.localScale = Vector3.one;

                        Bar.Add(iconObj.GetComponent<Image>());
                }
        }

        public static void Update()
        {
                HealthBar.Logic();
                HungerBar.Logic();
        }

        public virtual void Logic() { }
}

public class GUIHealthBar : GUIBar
{
        public GUIHealthBar(Vector3Int position) : base(position) { }

        public override void Logic()
        {
                for (int i = 0; i < Bar.Count; i += 1)
                {
                        int target = i * 2 + 2; 
                        if (target > Main.PlayerInfo.HealthMax)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/Null");
                        }
                        else if (target <= Main.PlayerInfo.Health)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHeartFull");
                        }
                        else if (target - 1 <= Main.PlayerInfo.Health)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHeartHalf");
                        }
                        else
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHeartEmpty");
                        }
                }
        }
}
public class GUIHungerBar : GUIBar
{ 
        public GUIHungerBar(Vector3Int position) : base(position) { }

        public override void Logic()
        {
                for (int i = 0; i < Bar.Count; i += 1)
                {
                        int target = i * 2 + 2; 
                        if (target > Main.PlayerInfo.hungerMax)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/Null");
                        }
                        else if (target <= Main.PlayerInfo.hunger)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHungerFull");
                        }
                        else if (target - 1 <= Main.PlayerInfo.hunger)
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHungerHalf");
                        }
                        else
                        {
                                Bar[i].sprite = Cache.LoadSprite("Sprite/GUIHungerEmpty");
                        }
                }
        }
}