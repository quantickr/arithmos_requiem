using System.Linq;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Core;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    public class DeckManagerTests
    {
        /// <summary>Минимальная карта-заглушка для проверки перемещений между зонами.</summary>
        private sealed class TestCard : Card
        {
            private readonly string _id;
            public TestCard(string id) => _id = id;

            public override string DefinitionId => _id;
            public override string RulesText => _id;
            public override CardKind Kind => CardKind.Numbered;
            public override CardCondition Condition => CardCondition.None;
            public override void Execute(BattleContext ctx, CardContext play) { }
        }

        private static TestCard[] MakeCards(int count)
        {
            var arr = new TestCard[count];
            for (int i = 0; i < count; i++)
                arr[i] = new TestCard($"c{i}");
            return arr;
        }

        [Test]
        public void LoadDeck_PutsAllInDeckZone()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            var cards = MakeCards(3);

            deck.LoadDeck(cards);

            Assert.AreEqual(3, deck.DeckCount);
            Assert.IsTrue(cards.All(c => c.Zone == CardZone.Deck));
        }

        [Test]
        public void DrawOne_MovesTopToHand()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            var cards = MakeCards(3);
            deck.LoadDeck(cards);

            var drawn = deck.DrawOne();

            Assert.IsNotNull(drawn);
            Assert.AreEqual(CardZone.Hand, drawn.Zone);
            Assert.AreEqual(2, deck.DeckCount);
            Assert.AreEqual(1, deck.HandCount);
        }

        [Test]
        public void DrawOne_EmptyEverything_ReturnsNull()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            deck.LoadDeck(MakeCards(0));

            Assert.IsNull(deck.DrawOne());
        }

        [Test]
        public void DrawOne_ReshufflesDiscardWhenDeckEmpty()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            var cards = MakeCards(1);
            deck.LoadDeck(cards);

            var c = deck.DrawOne();      // рука
            deck.DiscardFromHand(c);     // сброс
            Assert.AreEqual(0, deck.DeckCount);

            // Колода пуста, но сброс не пуст → DrawOne должен замешать и выдать карту.
            var again = deck.DrawOne();
            Assert.IsNotNull(again);
            Assert.AreEqual(CardZone.Hand, again.Zone);
        }

        [Test]
        public void DiscardHand_MovesAllToDiscard()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            deck.LoadDeck(MakeCards(3));
            deck.DrawOne();
            deck.DrawOne();

            deck.DiscardHand();

            Assert.AreEqual(0, deck.HandCount);
            Assert.AreEqual(2, deck.Discard.Count);
        }

        [Test]
        public void ExileCard_MovesToExiledZone()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            var cards = MakeCards(2);
            deck.LoadDeck(cards);
            var c = deck.DrawOne();

            deck.ExileCard(c);

            Assert.AreEqual(CardZone.Exiled, c.Zone);
            Assert.AreEqual(1, deck.Exiled.Count);
            Assert.AreEqual(0, deck.HandCount);
        }

        [Test]
        public void RemoveCard_MovesToRemovedZone()
        {
            var deck = new DeckManager(new FakeRandomProvider());
            var cards = MakeCards(2);
            deck.LoadDeck(cards);
            var c = deck.DrawOne();

            deck.RemoveCard(c);

            Assert.AreEqual(CardZone.Removed, c.Zone);
            Assert.AreEqual(1, deck.Removed.Count);
        }

        [Test]
        public void Shuffle_IsDeterministic_WithSameSeed()
        {
            // rangeSequence управляет Fisher–Yates: одинаковая последовательность → одинаковый порядок.
            var rng1 = new FakeRandomProvider(rangeSequence: new[] { 0, 1, 0 });
            var rng2 = new FakeRandomProvider(rangeSequence: new[] { 0, 1, 0 });

            var d1 = new DeckManager(rng1);
            var d2 = new DeckManager(rng2);
            d1.LoadDeck(MakeCards(4));
            d2.LoadDeck(MakeCards(4));

            d1.ShuffleDeck();
            d2.ShuffleDeck();

            var order1 = d1.Deck.Select(c => c.DefinitionId).ToArray();
            var order2 = d2.Deck.Select(c => c.DefinitionId).ToArray();
            CollectionAssert.AreEqual(order1, order2);
        }
    }
}
