using ArithmosRequiem.Core;

namespace ArithmosRequiem.Systems.Events
{
    public enum BattleEventType
    {
        TurnStart,
        DiceRolled,
        DiceValueChanged, // изменение значения кубика (эмитится ВСЕГДА при операции)
        RolledValue,      // «выпало X» — при изменении кубика на X (или наличии Кубика X)
        DoubleFormed,     // на столе образовался дубль
        PowerChanged,
        CardPlayed,
        CardDrawn,
        CardDiscarded,
        CardExiled,
        CardRemoved,
        TurnEnd
    }

    /// <summary>
    /// Базовое боевое событие. Конкретные данные — в наследниках.
    /// Используется EventBus для оповещения слушателей и триггеров «Когда».
    /// </summary>
    public abstract class BattleEvent
    {
        public abstract BattleEventType Type { get; }
    }

    public sealed class TurnStartEvent : BattleEvent
    {
        public int TurnNumber { get; }
        public TurnStartEvent(int turnNumber) => TurnNumber = turnNumber;
        public override BattleEventType Type => BattleEventType.TurnStart;
    }

    public sealed class TurnEndEvent : BattleEvent
    {
        public override BattleEventType Type => BattleEventType.TurnEnd;
    }

    public sealed class DiceRolledEvent : BattleEvent
    {
        public override BattleEventType Type => BattleEventType.DiceRolled;
    }

    public sealed class DiceValueChangedEvent : BattleEvent
    {
        public Die Die { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public DiceValueChangedEvent(Die die, int oldValue, int newValue)
        {
            Die = die; OldValue = oldValue; NewValue = newValue;
        }
        public override BattleEventType Type => BattleEventType.DiceValueChanged;
    }

    /// <summary>«Выпало X»: кубик принял значение Value (при изменении или при розыгрыше).</summary>
    public sealed class RolledValueEvent : BattleEvent
    {
        public int Value { get; }
        public RolledValueEvent(int value) => Value = value;
        public override BattleEventType Type => BattleEventType.RolledValue;
    }

    public sealed class DoubleFormedEvent : BattleEvent
    {
        public int Value { get; }
        public DoubleFormedEvent(int value) => Value = value;
        public override BattleEventType Type => BattleEventType.DoubleFormed;
    }

    public sealed class PowerChangedEvent : BattleEvent
    {
        public int OldPower { get; }
        public int NewPower { get; }
        public PowerChangedEvent(int oldPower, int newPower)
        {
            OldPower = oldPower; NewPower = newPower;
        }
        public override BattleEventType Type => BattleEventType.PowerChanged;
    }

    // Карт-события используют object для ссылки на карту, чтобы Systems не зависел от Cards.
    public sealed class CardPlayedEvent : BattleEvent
    {
        public object Card { get; }
        public CardPlayedEvent(object card) => Card = card;
        public override BattleEventType Type => BattleEventType.CardPlayed;
    }

    public sealed class CardDrawnEvent : BattleEvent
    {
        public object Card { get; }
        public CardDrawnEvent(object card) => Card = card;
        public override BattleEventType Type => BattleEventType.CardDrawn;
    }

    public sealed class CardDiscardedEvent : BattleEvent
    {
        public object Card { get; }
        public CardDiscardedEvent(object card) => Card = card;
        public override BattleEventType Type => BattleEventType.CardDiscarded;
    }

    public sealed class CardExiledEvent : BattleEvent
    {
        public object Card { get; }
        public CardExiledEvent(object card) => Card = card;
        public override BattleEventType Type => BattleEventType.CardExiled;
    }

    public sealed class CardRemovedEvent : BattleEvent
    {
        public object Card { get; }
        public CardRemovedEvent(object card) => Card = card;
        public override BattleEventType Type => BattleEventType.CardRemoved;
    }
}
