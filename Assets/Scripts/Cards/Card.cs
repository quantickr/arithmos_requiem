using System;
using ArithmosRequiem.Core;
using ArithmosRequiem.Systems.Events;

namespace ArithmosRequiem.Cards
{
    /// <summary>
    /// Базовый класс карты. Каждая конкретная карта — отдельный наследник (по решению
    /// пользователя: код на каждую карту). Логика эффекта в Execute — суб-эффекты идут
    /// в порядке строк, что соответствует «текст карты срабатывает в порядке написания».
    /// protected-хелперы делают наследников короткими.
    /// </summary>
    public abstract class Card
    {
        /// <summary>Стабильный идентификатор экземпляра (для UI и ссылок).</summary>
        public int InstanceId { get; internal set; }

        /// <summary>Строковый id определения карты (например "card_019", "start_1").</summary>
        public abstract string DefinitionId { get; }

        /// <summary>Отображаемое имя / текст (заполняется из метаданных, если есть).</summary>
        public virtual string DisplayName => DefinitionId;
        public abstract string RulesText { get; }

        public abstract CardKind Kind { get; }
        public abstract CardCondition Condition { get; }

        /// <summary>Текущая зона карты.</summary>
        public CardZone Zone { get; internal set; } = CardZone.Deck;

        /// <summary>Можно ли эту карту удалить/изгонять (стартовая №5 — нельзя).</summary>
        public virtual bool CanBeRemoved => true;
        public virtual bool CanBeExiled => true;
        public virtual bool CanBePlayed => true;

        // --- Жизненный цикл ---

        /// <summary>
        /// Проверка легальности розыгрыша. Для «Если» здесь проверяется условие карты.
        /// Возвращает false → карту нельзя разыграть сейчас.
        /// </summary>
        public virtual bool CanPlay(BattleContext ctx) => CanBePlayed;

        /// <summary>
        /// Основной эффект карты. Для «Если»/безусловных — исполняется сразу.
        /// Для «Когда» этот метод НЕ вызывается при розыгрыше напрямую — вместо этого
        /// вызывается RegisterWhenTriggers, а Execute запускается каждый раз при срабатывании.
        /// </summary>
        public abstract void Execute(BattleContext ctx, CardContext play);

        /// <summary>
        /// Для «Когда»-карт: регистрация постоянных триггеров на текущий ход.
        /// По умолчанию — не делает ничего (наследник переопределяет).
        /// </summary>
        public virtual void RegisterWhenTriggers(BattleContext ctx) { }

        /// <summary>Эффект при сбросе карты (карта №90 срабатывает без розыгрыша).</summary>
        public virtual void OnDiscard(BattleContext ctx) { }

        /// <summary>Эффект при доборе карты в руку.</summary>
        public virtual void OnDraw(BattleContext ctx) { }

        /// <summary>Хук при изгнании.</summary>
        public virtual void OnExile(BattleContext ctx) { }

        /// <summary>Хук при удалении.</summary>
        public virtual void OnRemove(BattleContext ctx) { }

        // --- protected хелперы (сокращают наследников) ---

        protected void Power(BattleContext ctx, PowerOp op) => ctx.ModifyPower(op, this);

        protected void AddP(BattleContext ctx, int x) => ctx.ModifyPower(PowerOp.Add(x), this);
        protected void SubP(BattleContext ctx, int x) => ctx.ModifyPower(PowerOp.Sub(x), this);
        protected void SetP(BattleContext ctx, int x) => ctx.ModifyPower(PowerOp.Set(x), this);
        protected void MulP(BattleContext ctx, double f) => ctx.ModifyPower(PowerOp.Mul(f), this);
        protected void DivP(BattleContext ctx, int x) => ctx.ModifyPower(PowerOp.Div(x), this);

        protected void DrawCards(BattleContext ctx, int n) => ctx.DrawCards(n);
        protected void DiscardRandomFromHand(BattleContext ctx, int n) => ctx.DiscardRandomFromHand(n);

        protected bool Rolled(BattleContext ctx, int x) => ctx.Dice.HasValue(x);
        protected bool HasDouble(BattleContext ctx) => ctx.Dice.HasDouble();

        /// <summary>Регистрирует «Когда»-триггер, привязанный к этой карте, снимаемый в конце хода.</summary>
        protected void RegisterWhen(BattleContext ctx,
                                    BattleEventType eventType,
                                    Predicate<BattleEvent> condition,
                                    Action<BattleEvent> effect)
        {
            var trigger = new WhenTrigger(eventType, condition, effect, this,
                                          expiresAtTurnEnd: Kind == CardKind.Action);
            ctx.Triggers.Register(trigger);
        }
    }
}
