using ArithmosRequiem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Визуал кубика: скруглённый квадрат с точками (pips) по значению 1..6.
    /// Отключённый боссом кубик рисуется приглушённым. Точки раскладываются
    /// процедурно по стандартной раскладке d6.
    /// </summary>
    public sealed class DieView : MonoBehaviour
    {
        private readonly Image[] _pips = new Image[9]; // 3x3 сетка позиций

        public static DieView Create(Transform parent, Die die)
        {
            var rt = UiBuilder.Panel(parent, $"Die_{die.Id}",
                die.IsDisabled ? UiTheme.DieDisabled : UiTheme.DieFace, 12);
            rt.sizeDelta = new Vector2(UiTheme.DieSize, UiTheme.DieSize);

            var view = rt.gameObject.AddComponent<DieView>();
            view.BuildPips(rt);
            view.Render(die.Value);
            return view;
        }

        // Раскладка точек 3x3: индексы 0..8 (слева-направо, сверху-вниз).
        // Для каждого значения — какие позиции активны.
        private static readonly int[][] PipLayout =
        {
            new int[0],                    // 0 (не используется)
            new[] { 4 },                   // 1 — центр
            new[] { 0, 8 },                // 2
            new[] { 0, 4, 8 },             // 3
            new[] { 0, 2, 6, 8 },          // 4
            new[] { 0, 2, 4, 6, 8 },       // 5
            new[] { 0, 2, 3, 5, 6, 8 },    // 6
        };

        private void BuildPips(RectTransform parent)
        {
            const float pad = 0.16f;   // отступ от края
            const float step = 0.34f;  // шаг сетки в долях
            float pipFrac = 0.16f;     // размер точки в долях

            for (int i = 0; i < 9; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float cx = pad + col * step;
                float cy = 1f - (pad + row * step);

                var pipRt = UiBuilder.Panel(parent, $"pip{i}", UiTheme.DiePip, 32);
                UiBuilder.SetAnchor(pipRt,
                    new Vector2(cx - pipFrac / 2, cy - pipFrac / 2),
                    new Vector2(cx + pipFrac / 2, cy + pipFrac / 2),
                    Vector2.zero, Vector2.zero);
                _pips[i] = pipRt.GetComponent<Image>();
                _pips[i].enabled = false;
            }
        }

        public void Render(int value)
        {
            for (int i = 0; i < 9; i++)
                if (_pips[i] != null) _pips[i].enabled = false;

            if (value < 1 || value > 6) return;
            foreach (int idx in PipLayout[value])
                if (_pips[idx] != null) _pips[idx].enabled = true;
        }
    }
}
