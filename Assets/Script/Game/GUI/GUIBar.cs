using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class GUIBar
{
        protected readonly List<Image> Bar = new List<Image>();
        private static readonly GUIBar HealthBar = new GUIHealthBar(new Vector3Int(-455, 250, 0));
        private static readonly GUIBar HungerBar = new GUIHungerBar(new Vector3Int(-455, 225, 0));

        protected GUIBar(Vector3Int position)
        {
                GameObject obj = new GameObject("Bar", typeof(RectTransform));
                RectTransform rectTransform = obj.GetComponent<RectTransform>();

                rectTransform.SetParent(Main.GUIInv.transform, false);
                rectTransform.localPosition = position;
                rectTransform.localRotation = Quaternion.Euler(-15, 0, 0);

                for (int i = 0; i < 10; i++)
                {
                        GameObject iconObj = Object.Instantiate(Resources.Load<GameObject>("Prefab/GUIIcon"), rectTransform);
                        RectTransform iconRect = iconObj.GetComponent<RectTransform>();

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

        protected abstract int Max { get; }
        protected abstract int Current { get; }
        protected abstract string FullIcon { get; }
        protected abstract string EmptyIcon { get; }

        private void Logic()
        {
                for (int i = 0; i < Bar.Count; i += 1)
                {
                        int target = i * 2 + 2;
                        if (target > Max)
                        {
                                Bar[i].sprite = Cache.LoadSprite("GUI/Null");
                        }
                        else if (target <= Current)
                        {
                                Bar[i].sprite = Cache.LoadSprite(FullIcon);
                        }
                        else
                        {
                                Bar[i].sprite = Cache.LoadSprite(EmptyIcon);
                        }
                }
        }
}

public class GUIHealthBar : GUIBar
{
        public GUIHealthBar(Vector3Int position) : base(position) { }

        protected override int Max => Main.PlayerInfo.HealthMax;
        protected override int Current => Main.PlayerInfo.Health;
        protected override string FullIcon => "GUI/GUIHeartFull";
        protected override string EmptyIcon => "GUI/GUIHeartEmpty";
}

public class GUIHungerBar : GUIBar
{
        public GUIHungerBar(Vector3Int position) : base(position) { }

        protected override int Max => Main.PlayerInfo.HungerMax;
        protected override int Current => Main.PlayerInfo.Hunger;
        protected override string FullIcon => "GUI/GUIHungerFull";
        protected override string EmptyIcon => "GUI/GUIHungerEmpty";
}