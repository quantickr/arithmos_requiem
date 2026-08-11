namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Игральная кость. Значение всегда клампится в диапазон [1..6].
    /// </summary>
    public sealed class Die
    {
        public const int MinValue = 1;
        public const int MaxValue = 6;

        public int Id { get; }

        private int _value;
        public int Value
        {
            get => _value;
            set => _value = Clamp(value);
        }

        /// <summary>Временный кубик (добавлен картой действия) — живёт до конца хода.</summary>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Отключён боссом: значение не работает для эффектов карт и не даёт Мощь.
        /// (например Катабасис отключает кубики 1/2/3).
        /// </summary>
        public bool IsDisabled { get; set; }

        public Die(int id, int value, bool isTemporary = false)
        {
            Id = id;
            _value = Clamp(value);
            IsTemporary = isTemporary;
        }

        public static int Clamp(int v)
        {
            if (v < MinValue) return MinValue;
            if (v > MaxValue) return MaxValue;
            return v;
        }

        public override string ToString() =>
            $"Die#{Id}={Value}{(IsTemporary ? " (temp)" : string.Empty)}{(IsDisabled ? " (off)" : string.Empty)}";
    }
}
