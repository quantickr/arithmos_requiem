using System.Collections.Generic;
using System.Linq;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Cards
{
    /// <summary>
    /// Управляет зонами карт во время боя: колода, рука, сброс, изгнанные, удалённые.
    /// Не эмитит события сам — это делает BattleContext (единая точка).
    /// Перемешивание использует инъектированный RNG (детерминизм в тестах).
    /// </summary>
    public sealed class DeckManager
    {
        private readonly IRandomProvider _rng;

        private readonly List<Card> _deck = new List<Card>();     // верх колоды = конец списка
        private readonly List<Card> _hand = new List<Card>();
        private readonly List<Card> _discard = new List<Card>();
        private readonly List<Card> _exiled = new List<Card>();
        private readonly List<Card> _removed = new List<Card>();

        public DeckManager(IRandomProvider rng)
        {
            _rng = rng;
        }

        public IReadOnlyList<Card> Deck => _deck;
        public IReadOnlyList<Card> Hand => _hand;
        public IReadOnlyList<Card> Discard => _discard;
        public IReadOnlyList<Card> Exiled => _exiled;
        public IReadOnlyList<Card> Removed => _removed;

        public int DeckCount => _deck.Count;
        public int HandCount => _hand.Count;

        /// <summary>Загрузить полный список карт колоды в начало боя (все в зону Deck).</summary>
        public void LoadDeck(IEnumerable<Card> cards)
        {
            _deck.Clear(); _hand.Clear(); _discard.Clear(); _exiled.Clear(); _removed.Clear();
            foreach (var c in cards)
            {
                c.Zone = CardZone.Deck;
                _deck.Add(c);
            }
        }

        /// <summary>Перемешать колоду (Fisher–Yates на инъектированном RNG).</summary>
        public void ShuffleDeck()
        {
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = _rng.Range(0, i);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
        }

        /// <summary>
        /// Вытянуть одну карту в руку. Если колода пуста — замешать сброс в колоду.
        /// Возвращает вытянутую карту или null, если брать нечего (колода и сброс пусты).
        /// </summary>
        public Card DrawOne()
        {
            if (_deck.Count == 0)
                ReshuffleDiscardIntoDeck();

            if (_deck.Count == 0)
                return null; // совсем нет карт

            var card = _deck[_deck.Count - 1];
            _deck.RemoveAt(_deck.Count - 1);
            card.Zone = CardZone.Hand;
            _hand.Add(card);
            return card;
        }

        /// <summary>Замешать сброс обратно в колоду (когда колода закончилась).</summary>
        public void ReshuffleDiscardIntoDeck()
        {
            if (_discard.Count == 0) return;
            foreach (var c in _discard)
            {
                c.Zone = CardZone.Deck;
                _deck.Add(c);
            }
            _discard.Clear();
            ShuffleDeck();
        }

        /// <summary>Переместить карту из руки в сброс.</summary>
        public void DiscardFromHand(Card card)
        {
            if (_hand.Remove(card))
            {
                card.Zone = CardZone.Discard;
                _discard.Add(card);
            }
        }

        /// <summary>Все карты руки → в сброс (конец хода).</summary>
        public void DiscardHand()
        {
            foreach (var c in _hand)
            {
                c.Zone = CardZone.Discard;
                _discard.Add(c);
            }
            _hand.Clear();
        }

        /// <summary>Изгнать карту (до конца боя) из любой зоны.</summary>
        public void ExileCard(Card card)
        {
            RemoveFromAllZones(card);
            card.Zone = CardZone.Exiled;
            _exiled.Add(card);
        }

        /// <summary>Удалить карту навсегда из любой зоны.</summary>
        public void RemoveCard(Card card)
        {
            RemoveFromAllZones(card);
            card.Zone = CardZone.Removed;
            _removed.Add(card);
        }

        /// <summary>Случайная карта из руки (для «сбросьте случайную» механик).</summary>
        public Card RandomHandCard()
        {
            if (_hand.Count == 0) return null;
            return _hand[_rng.Range(0, _hand.Count - 1)];
        }

        private void RemoveFromAllZones(Card card)
        {
            _deck.Remove(card);
            _hand.Remove(card);
            _discard.Remove(card);
            _exiled.Remove(card);
            _removed.Remove(card);
        }
    }
}
