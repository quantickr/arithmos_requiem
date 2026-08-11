using System;

namespace ArithmosRequiem.Data
{
    /// <summary>Роль врага в главе: определяет множитель базового Лимита.</summary>
    public enum EnemyRole
    {
        First,   // первый враг главы — базовый Лимит
        Second,  // второй враг — x1.5
        Boss     // босс главы — x2
    }

    /// <summary>
    /// Чистая C#-модель определения врага: базовый Лимит, роль, опциональный id босса.
    /// Unity-независима. SO-обёртка (для правки в инспекторе) — на слое View.
    /// </summary>
    public sealed class EnemyDefinition
    {
        public string EnemyId { get; }
        public string DisplayName { get; }
        public int BaseLimit { get; }
        public EnemyRole Role { get; }

        /// <summary>Id босса из BossRegistry (только для Role == Boss). Может быть null.</summary>
        public string BossId { get; }

        public EnemyDefinition(string enemyId, string displayName, int baseLimit,
                               EnemyRole role = EnemyRole.First, string bossId = null)
        {
            EnemyId = enemyId;
            DisplayName = displayName;
            BaseLimit = baseLimit;
            Role = role;
            BossId = bossId;
        }

        /// <summary>Эффективный стартовый Лимит с учётом роли (First x1, Second x1.5, Boss x2).</summary>
        public int EffectiveLimit()
        {
            double factor = Role switch
            {
                EnemyRole.Second => 1.5,
                EnemyRole.Boss => 2.0,
                _ => 1.0
            };
            return Math.Max(0, (int)Math.Round(BaseLimit * factor, MidpointRounding.AwayFromZero));
        }
    }
}
