using UnityEngine;
using UnityEngine.UI;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Утилиты для процедурного построения uGUI-иерархии: панели, тексты, кнопки.
    /// Держит код BattleView компактным и декларативным.
    /// </summary>
    public static class UiBuilder
    {
        public static RectTransform Panel(Transform parent, string name, Color color,
                                          int cornerRadius = 18)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            if (cornerRadius > 0)
            {
                img.sprite = ProceduralSprites.RoundedRect(64, 64, cornerRadius);
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.sprite = ProceduralSprites.White();
            }
            img.color = color;
            return rt;
        }

        public static Text Label(Transform parent, string name, string text, int fontSize,
                                 Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string caption,
                                    Color bg, Color fg, out Text captionText)
        {
            var rt = Panel(parent, name, bg, 14);
            var go = rt.gameObject;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();

            captionText = Label(rt, "Caption", caption, 22, fg);
            Stretch(captionText.rectTransform);
            return btn;
        }

        /// <summary>Растянуть RectTransform на весь родитель.</summary>
        public static void Stretch(RectTransform rt, float margin = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        public static void SetAnchor(RectTransform rt, Vector2 min, Vector2 max,
                                     Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }
    }
}
