using System.Collections.Generic;

namespace ArithmosRequiem.Data
{
    /// <summary>
    /// Базовые Лимиты глав кампании (по правилам игры): 8 глав.
    /// В каждой главе: 1-й враг (базовый), 2-й враг (x1.5), босс (x2).
    /// </summary>
    public static class ChapterProgression
    {
        /// <summary>Базовые Лимиты первых врагов по главам (индекс 0 = глава 1).</summary>
        public static readonly int[] BaseLimits =
        {
            12, 18, 30, 42, 60, 84, 132, 240
        };

        public static int ChapterCount => BaseLimits.Length;

        /// <summary>Базовый Лимит главы (1-based номер главы).</summary>
        public static int BaseLimitForChapter(int chapterNumber)
        {
            int idx = chapterNumber - 1;
            if (idx < 0 || idx >= BaseLimits.Length) return 0;
            return BaseLimits[idx];
        }

        /// <summary>
        /// Три врага главы: обычный, усиленный (x1.5), босс (x2).
        /// bossId привязывается к боссу главы (может быть null на раннем этапе).
        /// </summary>
        public static IReadOnlyList<EnemyDefinition> BuildChapterEnemies(int chapterNumber, string bossId = null)
        {
            int baseLimit = BaseLimitForChapter(chapterNumber);
            return new[]
            {
                new EnemyDefinition($"ch{chapterNumber}_e1", $"Глава {chapterNumber} — Враг I",
                    baseLimit, EnemyRole.First),
                new EnemyDefinition($"ch{chapterNumber}_e2", $"Глава {chapterNumber} — Враг II",
                    baseLimit, EnemyRole.Second),
                new EnemyDefinition($"ch{chapterNumber}_boss", $"Глава {chapterNumber} — Босс",
                    baseLimit, EnemyRole.Boss, bossId),
            };
        }
    }
}
