using System;
using System.Collections.Generic;
using System.Linq;

namespace ArithmosRequiem.Core
{
    public enum DieOpType
    {
        Add,        // +X к кубику (клампится 1..6)
        Subtract,   // -X к кубику
        Set,        // =X на кубике
        Reroll,     // переброс
        Flip        // переворот: 7 - value
    }

    public readonly struct DieOp
    {
        public readonly DieOpType Type;
        public readonly int Amount; // для Add/Subtract/Set

        public DieOp(DieOpType type, int amount = 0)
        {
            Type = type;
            Amount = amount;
        }

        public static DieOp Add(int x) => new DieOp(DieOpType.Add, x);
        public static DieOp Sub(int x) => new DieOp(DieOpType.Subtract, x);
        public static DieOp Set(int x) => new DieOp(DieOpType.Set, x);
        public static DieOp Reroll() => new DieOp(DieOpType.Reroll);
        public static DieOp Flip() => new DieOp(DieOpType.Flip);
    }

    /// <summary>
    /// Результат операции над кубиком: старое и новое значение.
    /// «Изменение значения кубика» считается произошедшим ВСЕГДА при вызове операции,
    /// даже если NewValue == OldValue (правило игры).
    /// </summary>
    public readonly struct DieChangeResult
    {
        public readonly Die Die;
        public readonly int OldValue;
        public readonly int NewValue;

        public DieChangeResult(Die die, int oldValue, int newValue)
        {
            Die = die;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Владеет активными кубиками на столе. Все изменения значений проходят через Apply,
    /// который возвращает результат для эмиссии событий на уровне BattleContext.
    /// Сам DiceManager не эмитит события — это делает BattleContext (единая точка).
    /// </summary>
    public sealed class DiceManager
    {
        private readonly List<Die> _dice = new List<Die>();
        private readonly IRandomProvider _rng;
        private int _nextId;

        public DiceManager(IRandomProvider rng)
        {
            _rng = rng;
        }

        public IReadOnlyList<Die> Dice => _dice;

        /// <summary>Все кубики, кроме отключённых боссом.</summary>
        public IEnumerable<Die> ActiveDice => _dice.Where(d => !d.IsDisabled);

        // --- Создание / удаление ---

        /// <summary>Убрать все кубики (перед новым ходом).</summary>
        public void Clear() => _dice.Clear();

        /// <summary>Убрать только временные кубики (конец хода).</summary>
        public void RemoveTemporary() => _dice.RemoveAll(d => d.IsTemporary);

        /// <summary>Добавить кубик с заданным значением (без броска).</summary>
        public Die AddDie(int value, bool temporary = false)
        {
            var die = new Die(_nextId++, value, temporary);
            _dice.Add(die);
            return die;
        }

        /// <summary>Добавить кубик и сразу бросить его.</summary>
        public Die AddRolledDie(bool temporary = false)
        {
            var die = new Die(_nextId++, _rng.RollD6(), temporary);
            _dice.Add(die);
            return die;
        }

        /// <summary>
        /// Полный бросок в начале хода: создаёт diceCount постоянных кубиков и бросает их.
        /// diceCount — уже модуль характеристики Кости (обработка отрицательных — выше).
        /// </summary>
        public List<Die> RollAll(int diceCount)
        {
            _dice.Clear();
            var rolled = new List<Die>(diceCount);
            for (int i = 0; i < diceCount; i++)
                rolled.Add(AddRolledDie());
            return rolled;
        }

        // --- Операции над одним кубиком ---

        /// <summary>
        /// Применяет операцию к кубику. Возвращает результат (old/new).
        /// Событие изменения эмитится ВСЕГДА снаружи (BattleContext), даже если значение то же.
        /// </summary>
        public DieChangeResult Apply(Die die, DieOp op)
        {
            int oldValue = die.Value;
            int newValue;
            switch (op.Type)
            {
                case DieOpType.Add:
                    newValue = Die.Clamp(oldValue + op.Amount);
                    break;
                case DieOpType.Subtract:
                    newValue = Die.Clamp(oldValue - op.Amount);
                    break;
                case DieOpType.Set:
                    newValue = Die.Clamp(op.Amount);
                    break;
                case DieOpType.Reroll:
                    newValue = _rng.RollD6();
                    break;
                case DieOpType.Flip:
                    newValue = Die.Clamp(7 - oldValue);
                    break;
                default:
                    newValue = oldValue;
                    break;
            }

            die.Value = newValue;
            return new DieChangeResult(die, oldValue, newValue);
        }

        // --- Запросы состояния ---

        /// <summary>Есть ли дубль (2+ активных кубика с одинаковым значением).</summary>
        public bool HasDouble()
        {
            return ActiveDice
                .GroupBy(d => d.Value)
                .Any(g => g.Count() >= 2);
        }

        /// <summary>Есть ли N+ активных кубиков с одинаковым значением.</summary>
        public bool HasNOfAKind(int n)
        {
            return ActiveDice
                .GroupBy(d => d.Value)
                .Any(g => g.Count() >= n);
        }

        /// <summary>Есть ли активный «Кубик X» на столе.</summary>
        public bool HasValue(int value) => ActiveDice.Any(d => d.Value == value);

        /// <summary>Сумма значений активных кубиков.</summary>
        public int SumOfActive() => ActiveDice.Sum(d => d.Value);

        /// <summary>Количество активных кубиков.</summary>
        public int ActiveCount() => ActiveDice.Count();

        /// <summary>Количество всех кубиков на столе (включая отключённые).</summary>
        public int TotalCount() => _dice.Count;
    }
}
