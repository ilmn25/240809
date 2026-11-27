using System.Collections.Generic;
using UnityEngine;

public static class DamageText
{
    private struct Entry
    {
        public Vector3 World;
        public string Text;
        public Color Color;
        public float Age;
        public float Life;
        public int Size;
    }

    private static readonly List<Entry> Entries = new();
    private static GUIStyle _style;

    public static void Show(Vector3 world, int damage, bool crit, bool onPlayer)
    {
        Color color = crit ? new Color(1f, 0.82f, 0.2f) : onPlayer ? new Color(1f, 0.35f, 0.35f) : Color.white;
        Entries.Add(new Entry
        {
            World = world,
            Text = crit ? damage + "!" : damage.ToString(),
            Color = color,
            Age = 0f,
            Life = crit ? 1f : 0.7f,
            Size = crit ? 22 : 16,
        });
    }

    public static void Update()
    {
        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            Entry entry = Entries[i];
            entry.Age += Time.deltaTime;
            if (entry.Age >= entry.Life) Entries.RemoveAt(i);
            else Entries[i] = entry;
        }
    }

    public static void OnGUI()
    {
        if (Entries.Count == 0 || Main.Camera == null) return;

        if (_style == null)
            _style = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

        foreach (Entry entry in Entries)
        {
            Vector3 screen = Main.Camera.WorldToScreenPoint(entry.World + Vector3.up * (entry.Age * 0.8f));
            if (screen.z < 0f) continue;

            float alpha = 1f - entry.Age / entry.Life;
            _style.fontSize = entry.Size;
            UnityEngine.GUI.color = new Color(entry.Color.r, entry.Color.g, entry.Color.b, alpha);
            UnityEngine.GUI.Label(new Rect(screen.x - 50f, Screen.height - screen.y - 12f, 100f, 30f), entry.Text, _style);
        }

        UnityEngine.GUI.color = Color.white;
    }
}
