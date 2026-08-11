using UnityEngine;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Генерация спрайтов кодом (без ассетов): скруглённый прямоугольник для карт/панелей,
    /// плоский белый пиксель для заливок. Кэширует созданные спрайты по параметрам.
    /// Позже эти спрайты можно заменить художественными ассетами, не трогая логику View.
    /// </summary>
    public static class ProceduralSprites
    {
        private static Sprite _white;
        private static readonly System.Collections.Generic.Dictionary<int, Sprite> _rounded =
            new System.Collections.Generic.Dictionary<int, Sprite>();

        /// <summary>Единичный белый спрайт 4x4 (для Image с заливкой цветом).</summary>
        public static Sprite White()
        {
            if (_white != null) return _white;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            return _white;
        }

        /// <summary>
        /// Скруглённый прямоугольник заданного размера и радиуса (белый, тонируется через Image.color).
        /// Используется как фон карт/панелей в стиле Balatro.
        /// </summary>
        public static Sprite RoundedRect(int width, int height, int radius)
        {
            int key = (width * 73856093) ^ (height * 19349663) ^ (radius * 83492791);
            if (_rounded.TryGetValue(key, out var cached)) return cached;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var clear = new Color(1, 1, 1, 0);
            var solid = Color.white;
            var px = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    px[y * width + x] = InsideRounded(x, y, width, height, radius) ? solid : clear;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;

            var border = Vector4.one * radius;
            var sprite = Sprite.Create(tex, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            _rounded[key] = sprite;
            return sprite;
        }

        private static bool InsideRounded(int x, int y, int w, int h, int r)
        {
            if (r <= 0) return true;
            // Проверяем четыре угловых окружности.
            int rx0 = r, rx1 = w - 1 - r;
            int ry0 = r, ry1 = h - 1 - r;

            int cx, cy;
            if (x < rx0 && y < ry0) { cx = rx0; cy = ry0; }
            else if (x > rx1 && y < ry0) { cx = rx1; cy = ry0; }
            else if (x < rx0 && y > ry1) { cx = rx0; cy = ry1; }
            else if (x > rx1 && y > ry1) { cx = rx1; cy = ry1; }
            else return true; // не в углу — внутри

            float dx = x - cx, dy = y - cy;
            return dx * dx + dy * dy <= (float)r * r;
        }
    }
}
