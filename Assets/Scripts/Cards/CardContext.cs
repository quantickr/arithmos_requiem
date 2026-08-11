using ArithmosRequiem.Systems.Events;

namespace ArithmosRequiem.Cards
{
    /// <summary>
    /// Контекст одного розыгрыша/срабатывания карты. Отличается от BattleContext:
    /// BattleContext — состояние всего боя, CardContext — данные конкретного вызова эффекта.
    /// </summary>
    public sealed class CardContext
    {
        public Card SourceCard { get; }

        /// <summary>Сработало как постоянный триггер «Когда» (а не при обычном розыгрыше).</summary>
        public bool IsFromWhenTrigger { get; }

        /// <summary>Событие, вызвавшее срабатывание «Когда» (иначе null).</summary>
        public BattleEvent TriggeringEvent { get; }

        /// <summary>
        /// Вызов эффекта происходит «в порядке использования» карты №85, а не как
        /// самостоятельный розыгрыш (не считать за розыгрыш, не давать штрафы босса).
        /// </summary>
        public bool IsReusedEffect { get; }

        public CardContext(Card sourceCard,
                           bool isFromWhenTrigger = false,
                           BattleEvent triggeringEvent = null,
                           bool isReusedEffect = false)
        {
            SourceCard = sourceCard;
            IsFromWhenTrigger = isFromWhenTrigger;
            TriggeringEvent = triggeringEvent;
            IsReusedEffect = isReusedEffect;
        }
    }
}
