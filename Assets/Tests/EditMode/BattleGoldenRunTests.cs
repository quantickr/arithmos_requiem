using System.Collections.Generic;
using System.Threading.Tasks;
using ArithmosRequiem.Bosses.Definitions;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Cards.Definitions.StartingCards;
using ArithmosRequiem.Core;
using ArithmosRequiem.Player;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    /// <summary>
    /// Интеграционные «золотые» прогоны целого боя через BattleEngine + AIController
    /// с детерминированным RNG. Цель — проверить, что весь конвейер (фазы хода,
    /// розыгрыш карт, применение Мощи к Лимиту, исход) связывается и стабилен.
    /// </summary>
    public class BattleGoldenRunTests
    {
        /// <summary>Стандартная стартовая колода по правилам: 4×+1, 3×кубик, 2×реролл, 1×если, 1×нельзя.</summary>
        private static List<Card> BuildStartingDeck()
        {
            var deck = new List<Card>();
            for (int i = 0; i < 4; i++) deck.Add(new Start_1_PlusOne());
            for (int i = 0; i < 3; i++) deck.Add(new Start_2_DieAdjust());
            for (int i = 0; i < 2; i++) deck.Add(new Start_3_RerollDie());
            deck.Add(new Start_4_IfPowerLt6Draw2());
            deck.Add(new Start_5_Unplayable());
            return deck;
        }

        [Test]
        public async Task Battle_RunsToTermination_NoBoss()
        {
            // Фиксированные броски кубика → детерминированный бой.
            var rng = new FakeRandomProvider(
                d6Sequence: new[] { 6, 5, 4, 3, 2, 1 },
                rangeSequence: new[] { 0, 1, 2, 1, 0 });

            var ctx = new BattleContext(rng, initialLimit: 12);
            var engine = new BattleEngine(ctx, new AIController());

            var result = await engine.RunBattleAsync(BuildStartingDeck());

            // Бой обязан завершиться (Win или Lose), а не зависнуть в InProgress.
            Assert.AreNotEqual(BattleOutcome.InProgress, result.Outcome);
            // Число ходов не превышает Скорость (по умолчанию 4).
            Assert.LessOrEqual(ctx.TurnsTakenVsEnemy, ctx.Stats.CurrentSpeed);
        }

        [Test]
        public async Task Battle_WithBoss_AppliesOnBattleStart()
        {
            var rng = new FakeRandomProvider(d6Sequence: new[] { 6, 6, 6 });

            // Диплос: Лимит x2 в начале боя. Стартовый 12 → 24.
            var boss = new Boss_13_Diplos();
            var ctx = new BattleContext(rng, initialLimit: 12);
            ctx.SetBoss(boss, boss);

            var engine = new BattleEngine(ctx, new AIController());
            await engine.RunBattleAsync(BuildStartingDeck());

            // После OnBattleStart Лимит удвоился (InitialLimit тоже).
            Assert.AreEqual(24, ctx.InitialLimit);
        }

        [Test]
        public async Task Battle_Deterministic_SameSeedSameResult()
        {
            async Task<int> RunOnce()
            {
                var rng = new FakeRandomProvider(
                    d6Sequence: new[] { 4, 2, 6, 1, 3, 5 },
                    rangeSequence: new[] { 1, 0, 2 });
                var ctx = new BattleContext(rng, initialLimit: 18);
                var engine = new BattleEngine(ctx, new AIController());
                await engine.RunBattleAsync(BuildStartingDeck());
                return ctx.Limit;
            }

            int a = await RunOnce();
            int b = await RunOnce();
            Assert.AreEqual(a, b);
        }
    }
}
