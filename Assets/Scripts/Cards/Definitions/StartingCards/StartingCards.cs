using System.Linq;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Cards.Definitions.StartingCards
{
    /// <summary>Стартовая карта №1: «+1» (в колоде 4 копии).</summary>
    public sealed class Start_1_PlusOne : Card
    {
        public override string DefinitionId => "start_1";
        public override string RulesText => "+1";
        public override CardKind Kind => CardKind.Starting;
        public override CardCondition Condition => CardCondition.None;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            AddP(ctx, 1);
        }
    }

    /// <summary>
    /// Стартовая карта №2: «+1/-1 к кубику» (3 копии).
    /// Требует выбор кубика и знака. Пока по умолчанию поднимает наименьший кубик на +1
    /// (эвристика; UI позже даст явный выбор через SetTarget).
    /// </summary>
    public sealed class Start_2_DieAdjust : Card
    {
        public override string DefinitionId => "start_2";
        public override string RulesText => "+1/-1 к кубику";
        public override CardKind Kind => CardKind.Starting;
        public override CardCondition Condition => CardCondition.None;

        /// <summary>Явно выбранная цель (id кубика) и дельта (+1/-1). Задаётся UI перед розыгрышем.</summary>
        public int? TargetDieId { get; set; }
        public int Delta { get; set; } = +1;

        public override bool CanPlay(BattleContext ctx) => ctx.Dice.Dice.Count > 0;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            var die = TargetDieId.HasValue
                ? ctx.Dice.Dice.FirstOrDefault(d => d.Id == TargetDieId.Value)
                : ctx.Dice.Dice.OrderBy(d => d.Value).FirstOrDefault();
            if (die == null) return;

            ctx.ChangeDie(die, Delta >= 0 ? DieOp.Add(1) : DieOp.Sub(1));
            ctx.RecomputePowerFromDice();
        }
    }

    /// <summary>Стартовая карта №3: «Переброс кубика» (2 копии).</summary>
    public sealed class Start_3_RerollDie : Card
    {
        public override string DefinitionId => "start_3";
        public override string RulesText => "Переброс кубика";
        public override CardKind Kind => CardKind.Starting;
        public override CardCondition Condition => CardCondition.None;

        public int? TargetDieId { get; set; }

        public override bool CanPlay(BattleContext ctx) => ctx.Dice.Dice.Count > 0;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            var die = TargetDieId.HasValue
                ? ctx.Dice.Dice.FirstOrDefault(d => d.Id == TargetDieId.Value)
                : ctx.Dice.Dice.OrderBy(d => d.Value).FirstOrDefault();
            if (die == null) return;

            ctx.ChangeDie(die, DieOp.Reroll());
            ctx.RecomputePowerFromDice();
        }
    }

    /// <summary>Стартовая карта №4: «Если Мощь &lt; 6, +2 карты».</summary>
    public sealed class Start_4_IfPowerLt6Draw2 : Card
    {
        public override string DefinitionId => "start_4";
        public override string RulesText => "Если Мощь < 6, +2 карты";
        public override CardKind Kind => CardKind.Starting;
        public override CardCondition Condition => CardCondition.If;

        // «Если»: карту можно разыграть только при выполнении условия.
        public override bool CanPlay(BattleContext ctx) => ctx.Power < 6;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            DrawCards(ctx, 2);
        }
    }

    /// <summary>
    /// Стартовая карта №5: «Эту карту нельзя разыграть или удалить».
    /// Мёртвый груз в колоде — нельзя ни сыграть, ни удалить, ни изгнать.
    /// </summary>
    public sealed class Start_5_Unplayable : Card
    {
        public override string DefinitionId => "start_5";
        public override string RulesText => "Эту карту нельзя разыграть или удалить";
        public override CardKind Kind => CardKind.Starting;
        public override CardCondition Condition => CardCondition.None;

        public override bool CanBePlayed => false;
        public override bool CanBeRemoved => false;
        public override bool CanBeExiled => false;

        public override bool CanPlay(BattleContext ctx) => false;

        public override void Execute(BattleContext ctx, CardContext play)
        {
            // Ничего не делает.
        }
    }
}
