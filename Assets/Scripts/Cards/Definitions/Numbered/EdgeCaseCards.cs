using System.Linq;
using ArithmosRequiem.Core;
using ArithmosRequiem.Data;
using ArithmosRequiem.Systems.Events;
using ArithmosRequiem.Utils;

namespace ArithmosRequiem.Cards.Definitions.Numbered
{
    /// <summary>
    /// №19: «Когда выпало 3, Мощь → простое число».
    /// Постоянный триггер: каждый раз, когда кубик становится 3 (или уже есть Кубик 3
    /// при розыгрыше), Мощь становится ближайшим строго большим простым.
    /// Карта действия → триггер снимается в конце хода.
    /// </summary>
    public sealed class Card_019_WhenRolled3ToPrime : Card
    {
        public override string DefinitionId => "card_019";
        public override string RulesText => "Когда выпало 3, Мощь → простое число";
        public override CardKind Kind => CardKind.Action;
        public override CardCondition Condition => CardCondition.When;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            ctx.ModifyPower(PowerOp.ToProperty(NumberPropertyType.Prime), this);
        }

        public override void RegisterWhenTriggers(BattleContext ctx)
        {
            // (2) Реакция на изменение кубика на 3 — через событие RolledValue(3).
            RegisterWhen(ctx, BattleEventType.RolledValue,
                evt => evt is RolledValueEvent rv && rv.Value == 3,
                _ => Execute(ctx, new CardContext(this, isFromWhenTrigger: true)));

            // (1) Если в момент розыгрыша на столе уже есть Кубик 3 — сработать сразу.
            if (Rolled(ctx, 3))
                Execute(ctx, new CardContext(this, isFromWhenTrigger: true));
        }
    }

    /// <summary>
    /// №23: «Если Мощь нечётная, х3 и +1, иначе :2; эффект выше срабатывает 3 раза при
    /// розыгрыше этой карты, после, если Мощь кратна 4 или на столе нет Кубика 3, она удаляется».
    /// Демонстрирует многоступенчатую логику: порядок = порядок строк, Мощь пересчитывается сразу.
    /// </summary>
    public sealed class Card_023_TripleParitySwing : Card
    {
        public override string DefinitionId => "card_023";
        public override string RulesText =>
            "Если Мощь нечётная, х3 и +1, иначе :2; повторить 3 раза; " +
            "после, если Мощь кратна 4 или нет Кубика 3 — удали эту карту";
        public override CardKind Kind => CardKind.Numbered;
        public override CardCondition Condition => CardCondition.None;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            for (int i = 0; i < 3; i++)
            {
                if (NumberProperties.IsOdd(ctx.Power))
                {
                    MulP(ctx, 3);
                    AddP(ctx, 1);
                }
                else
                {
                    DivP(ctx, 2);
                }
            }

            if (NumberProperties.IsMultipleOf(ctx.Power, 4) || !Rolled(ctx, 3))
                ctx.RemoveCard(this);
        }
    }

    /// <summary>
    /// №85: «-1 карта и -9, используй эффект последней разыгранной карты» (2 копии).
    /// Берёт предпоследнюю сыгранную карту (последняя перед этой) и повторяет её эффект
    /// как «использование» (не розыгрыш → без штрафов босса и без счётчика розыгрышей).
    /// </summary>
    public sealed class Card_085_UseLastPlayedEffect : Card
    {
        public override string DefinitionId => "card_085";
        public override string RulesText => "-1 карта и -9, используй эффект последней разыгранной карты";
        public override CardKind Kind => CardKind.Numbered;
        public override CardCondition Condition => CardCondition.None;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            DiscardRandomFromHand(ctx, 1);
            SubP(ctx, 9);

            // Последняя разыгранная карта ДО этой (эта карта уже добавлена в историю
            // фасадом PlayCard, поэтому берём предпоследний элемент).
            var history = ctx.PlayHistoryThisTurn;
            Card last = null;
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(history[i], this))
                {
                    last = history[i];
                    break;
                }
            }

            if (last != null)
                last.Execute(ctx, new CardContext(last, isReusedEffect: true));
        }
    }

    /// <summary>
    /// №90: «Мощь → чётное число, если ты сбросил эту карту, +17 (сработает без розыгрыша
    /// этой карты)» (2 копии). Демонстрирует эффект при сбросе (OnDiscard) без розыгрыша.
    /// </summary>
    public sealed class Card_090_EvenOrDiscardBonus : Card
    {
        public override string DefinitionId => "card_090";
        public override string RulesText => "Мощь → чётное число; если ты сбросил эту карту, +17";
        public override CardKind Kind => CardKind.Numbered;
        public override CardCondition Condition => CardCondition.None;

        // Обычный розыгрыш: Мощь → чётное.
        public override void Execute(BattleContext ctx, CardContext play)
        {
            ctx.ModifyPower(PowerOp.ToProperty(NumberPropertyType.Even), this);
        }

        // Сброс без розыгрыша: +17 (срабатывает из DiscardCard фасада).
        public override void OnDiscard(BattleContext ctx)
        {
            ctx.ModifyPower(PowerOp.Add(17), this);
        }
    }
}
