using System;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Характеристики игрока. Base — постоянные (дерево прокачки), Current — с учётом
    /// временных модификаторов боя (боссы). В начале боя Current сбрасывается к Base.
    /// </summary>
    public sealed class PlayerStats
    {
        // Стартовые значения по правилам.
        public const int DefaultDraw = 5;
        public const int DefaultSpeed = 4;
        public const int DefaultDice = 1;

        // Базовые (сохраняются между боями, меняются деревом прокачки).
        public int BaseDraw { get; set; } = DefaultDraw;
        public int BaseSpeed { get; set; } = DefaultSpeed;
        public int BaseDice { get; set; } = DefaultDice;

        // Текущие (модификаторы боссов применяются к ним во время боя).
        public int CurrentDraw { get; private set; }
        public int CurrentSpeed { get; private set; }
        public int CurrentDice { get; private set; }

        /// <summary>Сбросить текущие значения к базовым (начало боя).</summary>
        public void ResetToBase()
        {
            CurrentDraw = BaseDraw;
            CurrentSpeed = BaseSpeed;
            CurrentDice = BaseDice;
        }

        // --- Модификаторы (клампинг по правилам) ---

        /// <summary>Добор не может стать ниже 0.</summary>
        public void ModifyDraw(int delta) => CurrentDraw = Math.Max(0, CurrentDraw + delta);

        /// <summary>Скорость не может стать ниже 1.</summary>
        public void ModifySpeed(int delta) => CurrentSpeed = Math.Max(1, CurrentSpeed + delta);

        /// <summary>Кости могут быть отрицательными (особый режим вычитания).</summary>
        public void ModifyDice(int delta) => CurrentDice += delta;

        /// <summary>Прямая установка скорости (боссы Мономос/Кер).</summary>
        public void SetSpeed(int value) => CurrentSpeed = Math.Max(1, value);
    }
}
