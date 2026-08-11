using System.Collections.Generic;
using ArithmosRequiem.Bosses;
using ArithmosRequiem.Bosses.Definitions;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Cards.Definitions.Numbered;
using ArithmosRequiem.Cards.Definitions.StartingCards;

namespace ArithmosRequiem.Data
{
    /// <summary>
    /// Единая точка сборки контента: наполняет реестры реализованными на этом этапе
    /// картами и боссами, а также строит стандартную стартовую колоду.
    /// По мере добавления карт/боссов расширяется здесь (регистрация нового типа — одна строка).
    /// </summary>
    public static class ContentFactory
    {
        /// <summary>Собрать реестр карт со всеми реализованными определениями.</summary>
        public static CardRegistry BuildCardRegistry()
        {
            var reg = new CardRegistry();

            // Стартовые карты.
            reg.Register<Start_1_PlusOne>();
            reg.Register<Start_2_DieAdjust>();
            reg.Register<Start_3_RerollDie>();
            reg.Register<Start_4_IfPowerLt6Draw2>();
            reg.Register<Start_5_Unplayable>();

            // Числовые карты (крайние случаи — smoke-набор архитектуры).
            reg.Register<Card_019_WhenRolled3ToPrime>();
            reg.Register<Card_023_TripleParitySwing>();
            reg.Register<Card_085_UseLastPlayedEffect>();
            reg.Register<Card_090_EvenOrDiscardBonus>();

            return reg;
        }

        /// <summary>Собрать реестр боссов со всеми реализованными определениями.</summary>
        public static BossRegistry BuildBossRegistry()
        {
            var reg = new BossRegistry();

            reg.Register<Boss_01_Perissos>();
            reg.Register<Boss_02_Artios>();
            reg.Register<Boss_03_Katabasis>();
            reg.Register<Boss_06_Lethe>();
            reg.Register<Boss_11_Foros>();
            reg.Register<Boss_12_Moira>();
            reg.Register<Boss_13_Diplos>();
            reg.Register<Boss_14_Metron>();
            reg.Register<Boss_22_Tetragon>();
            reg.Register<Boss_23_Eratosthenes>();
            reg.Register<Boss_25_Geras>();

            return reg;
        }

        /// <summary>
        /// Стандартная стартовая колода по правилам:
        /// 4×«+1», 3×«+1/-1 к кубику», 2×«переброс», 1×«Если Мощь&lt;6 +2 карты», 1×«неиграбельная».
        /// </summary>
        public static List<Card> BuildStartingDeck()
        {
            var deck = new List<Card>();
            for (int i = 0; i < 4; i++) deck.Add(new Start_1_PlusOne());
            for (int i = 0; i < 3; i++) deck.Add(new Start_2_DieAdjust());
            for (int i = 0; i < 2; i++) deck.Add(new Start_3_RerollDie());
            deck.Add(new Start_4_IfPowerLt6Draw2());
            deck.Add(new Start_5_Unplayable());
            return deck;
        }
    }
}
