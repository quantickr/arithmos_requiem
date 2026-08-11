using System;
using ArithmosRequiem.Data;
using ArithmosRequiem.Utils;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Все вычисления над Мощью. Мощь — целое число; после дробных операций (:X, x1.5)
    /// применяется математическое округление (0.5 → вверх), согласно решению по правилам.
    /// </summary>
    public sealed class PowerManager
    {
        /// <summary>Флаг: последняя операция дала дробное значение (до округления).</summary>
        public bool LastOpWasFractional { get; private set; }

        /// <summary>
        /// Применяет операцию к текущему значению Мощи и возвращает новое (целое).
        /// bossMods нужен для операций →/← (учёт «простые не свойство»).
        /// </summary>
        public int Apply(int current, PowerOp op, IBossModifiers bossMods)
        {
            LastOpWasFractional = false;
            bossMods ??= NullBossModifiers.Instance;

            switch (op.Type)
            {
                case PowerOpType.Add:
                    return current + op.IntAmount;

                case PowerOpType.Subtract:
                    return current - op.IntAmount;

                case PowerOpType.Set:
                    return op.IntAmount;

                case PowerOpType.Multiply:
                {
                    double raw = current * op.Factor;
                    return RoundPower(raw);
                }

                case PowerOpType.Divide:
                {
                    if (op.IntAmount == 0) return current; // деление на 0 игнорируем
                    double raw = (double)current / op.IntAmount;
                    return RoundPower(raw);
                }

                case PowerOpType.ToProperty:
                {
                    var pred = NumberPropertyPredicates.Build(
                        op.Property, op.PropertyParam, bossMods.PrimesLoseProperties);
                    return NumberProperties.Next(current, pred);
                }

                case PowerOpType.FromProperty:
                {
                    var pred = NumberPropertyPredicates.Build(
                        op.Property, op.PropertyParam, bossMods.PrimesLoseProperties);
                    return NumberProperties.Prev(current, pred);
                }

                default:
                    return current;
            }
        }

        /// <summary>
        /// Математическое округление к ближайшему целому (0.5 → вверх, MidpointRounding.AwayFromZero).
        /// Помечает LastOpWasFractional, если было дробное значение.
        /// </summary>
        private int RoundPower(double raw)
        {
            int rounded = (int)Math.Round(raw, MidpointRounding.AwayFromZero);
            LastOpWasFractional = Math.Abs(raw - rounded) > double.Epsilon
                                  || Math.Abs(raw % 1.0) > double.Epsilon;
            return rounded;
        }

        /// <summary>
        /// Вычисляет новое значение Лимита после применения Мощи в конце хода.
        /// - Если Мощь &lt; 0: Лимит ПОВЫШАЕТСЯ на |Мощь| (не понижается).
        /// - Иначе: босс может заблокировать понижение (AllowLimitReduction) или
        ///   ограничить эффективную Мощь (ModifyEffectivePower).
        /// - Лимит не может стать ниже 0.
        /// </summary>
        public int ApplyPowerToLimit(int currentLimit, int power, int initialLimit, IBossModifiers bossMods)
        {
            bossMods ??= NullBossModifiers.Instance;

            if (power < 0)
            {
                // Отрицательная Мощь повышает Лимит.
                return currentLimit + (-power);
            }

            int effective = bossMods.ModifyEffectivePower(power, initialLimit);
            if (effective <= 0)
                return currentLimit;

            if (!bossMods.AllowLimitReduction(effective))
                return currentLimit; // босс запрещает понижение этой Мощью

            int newLimit = currentLimit - effective;
            return Math.Max(0, newLimit);
        }
    }
}
