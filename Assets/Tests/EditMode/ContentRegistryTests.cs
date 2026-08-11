using ArithmosRequiem.Cards;
using ArithmosRequiem.Data;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    public class ContentRegistryTests
    {
        [Test]
        public void CardRegistry_CreatesFreshInstances()
        {
            var reg = ContentFactory.BuildCardRegistry();

            var a = reg.Create("start_1");
            var b = reg.Create("start_1");

            Assert.IsNotNull(a);
            Assert.AreNotSame(a, b); // фабрика даёт новые экземпляры
            Assert.AreEqual("start_1", a.DefinitionId);
        }

        [Test]
        public void CardRegistry_ContainsRegisteredEdgeCards()
        {
            var reg = ContentFactory.BuildCardRegistry();
            Assert.IsTrue(reg.Contains("card_019"));
            Assert.IsTrue(reg.Contains("card_085"));
            Assert.IsFalse(reg.Contains("card_999"));
        }

        [Test]
        public void BossRegistry_CreatesById()
        {
            var reg = ContentFactory.BuildBossRegistry();
            var boss = reg.Create("boss_13_diplos");
            Assert.AreEqual("Диплос", boss.DisplayName);
        }

        [Test]
        public void StartingDeck_HasElevenCards()
        {
            var deck = ContentFactory.BuildStartingDeck();
            // 4 + 3 + 2 + 1 + 1 = 11 карт.
            Assert.AreEqual(11, deck.Count);
        }

        [TestCase(1, 12)]
        [TestCase(2, 18)]
        [TestCase(8, 240)]
        [TestCase(99, 0)]
        public void Chapter_BaseLimit_Matches(int chapter, int expected)
        {
            Assert.AreEqual(expected, ChapterProgression.BaseLimitForChapter(chapter));
        }

        [Test]
        public void EnemyDefinition_RoleMultipliers()
        {
            var first = new EnemyDefinition("e", "E", 12, EnemyRole.First);
            var second = new EnemyDefinition("e", "E", 12, EnemyRole.Second);
            var boss = new EnemyDefinition("e", "E", 12, EnemyRole.Boss);

            Assert.AreEqual(12, first.EffectiveLimit());
            Assert.AreEqual(18, second.EffectiveLimit()); // x1.5
            Assert.AreEqual(24, boss.EffectiveLimit());    // x2
        }
    }
}
