using UnityEngine;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Палитра и метрики UI в духе Balatro: тёмный фон, насыщенные акценты,
    /// крупные скруглённые карты. Все цвета/размеры в одном месте для быстрой правки.
    /// </summary>
    public static class UiTheme
    {
        // Фон и панели.
        public static readonly Color Background = new Color32(0x1A, 0x16, 0x22, 0xFF);
        public static readonly Color PanelDark = new Color32(0x24, 0x1E, 0x30, 0xFF);
        public static readonly Color PanelMid = new Color32(0x32, 0x2A, 0x42, 0xFF);

        // Акценты.
        public static readonly Color AccentRed = new Color32(0xE5, 0x3A, 0x40, 0xFF);
        public static readonly Color AccentBlue = new Color32(0x3A, 0x86, 0xE5, 0xFF);
        public static readonly Color AccentGold = new Color32(0xF2, 0xB6, 0x32, 0xFF);
        public static readonly Color AccentGreen = new Color32(0x36, 0xC7, 0x6A, 0xFF);

        // Карты.
        public static readonly Color CardFace = new Color32(0xF4, 0xEE, 0xE2, 0xFF);
        public static readonly Color CardFaceStarting = new Color32(0xDC, 0xE8, 0xF2, 0xFF);
        public static readonly Color CardBorder = new Color32(0x12, 0x0E, 0x1A, 0xFF);
        public static readonly Color CardText = new Color32(0x1A, 0x16, 0x22, 0xFF);

        // Кубики.
        public static readonly Color DieFace = new Color32(0xF4, 0xEE, 0xE2, 0xFF);
        public static readonly Color DiePip = new Color32(0x1A, 0x16, 0x22, 0xFF);
        public static readonly Color DieDisabled = new Color32(0x6A, 0x60, 0x74, 0xFF);

        public static readonly Color TextLight = new Color32(0xF4, 0xEE, 0xE2, 0xFF);
        public static readonly Color TextDim = new Color32(0xA8, 0x9E, 0xB4, 0xFF);

        // Метрики.
        public const float CardWidth = 150f;
        public const float CardHeight = 210f;
        public const float CardSpacing = 16f;
        public const float DieSize = 72f;
    }
}
