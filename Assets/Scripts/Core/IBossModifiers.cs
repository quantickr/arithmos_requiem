using ArithmosRequiem.Data;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Набор перехватов, которые босс может влиять на расчёты ядра.
    /// Ядро (PowerManager и т.п.) зависит от этого интерфейса, а не от класса Boss,
    /// чтобы оставаться Unity-независимым и легко тестируемым.
    /// Значения по умолчанию (когда босса нет) даёт NullBossModifiers.
    /// </summary>
    public interface IBossModifiers
    {
        /// <summary>Эратосфен: простые числа не считаются числом со свойствами.</summary>
        bool PrimesLoseProperties { get; }

        /// <summary>
        /// Можно ли понизить Лимит данной эффективной Мощью.
        /// Артиос/Периссос/Тетрагон и т.п. блокируют определённые значения.
        /// </summary>
        bool AllowLimitReduction(int effectivePower);

        /// <summary>
        /// Модификация эффективной Мощи перед применением к Лимиту.
        /// Метрон: не больше трети изначального Лимита за ход.
        /// </summary>
        int ModifyEffectivePower(int power, int initialLimit);

        /// <summary>Модификация значения Лимита при его установке (Диплос x2, Кайрос :2 и т.п.).</summary>
        int ModifyLimitOnSet(int limit);
    }

    /// <summary>Заглушка «босса нет»: ничего не меняет.</summary>
    public sealed class NullBossModifiers : IBossModifiers
    {
        public static readonly NullBossModifiers Instance = new NullBossModifiers();

        public bool PrimesLoseProperties => false;
        public bool AllowLimitReduction(int effectivePower) => true;
        public int ModifyEffectivePower(int power, int initialLimit) => power;
        public int ModifyLimitOnSet(int limit) => limit;
    }
}
