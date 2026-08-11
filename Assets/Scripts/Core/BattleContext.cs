using System;
using System.Collections.Generic;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Systems.Events;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Центральное состояние боя и ЕДИНСТВЕННАЯ точка мутации Мощи, кубиков и зон карт.
    /// Все изменения проходят через фасады этого класса, которые (а) применяют перехваты
    /// боссов, (б) эмитят события. Карты и боссы никогда не трогают поля напрямую.
    /// </summary>
    public sealed class BattleContext
    {
        // --- Числовое состояние ---
        public int Power { get; private set; }
        public int Limit { get; private set; }
        public int InitialLimit { get; private set; }
        public int TurnNumber { get; private set; }
        public int TurnsTakenVsEnemy { get; private set; }

        // --- Подсистемы ---
        public DiceManager Dice { get; }
        public DeckManager Deck { get; }
        public PlayerStats Stats { get; }
        public PowerManager PowerMgr { get; }
        public EventBus Events { get; }
        public TriggerRegistry Triggers { get; }
        public IRandomProvider Rng { get; }

        // --- Босс (может быть null) ---
        public IBossHooks BossHooks { get; private set; }
        public IBossModifiers BossMods { get; private set; }

        // --- Флаги и счётчики хода ---
        public bool ExiledThisTurn { get; private set; }
        public bool DiscardedThisTurn { get; private set; }
        public int CardsPlayedThisTurn { get; private set; }
        public int CardsDiscardedThisTurn { get; private set; }
        public int CardsDrawnThisTurn { get; private set; }
        public Card LastPlayedCard { get; private set; }
        public List<Card> PlayHistoryThisTurn { get; } = new List<Card>();

        /// <summary>Кости < 0: кубики вычитают Мощь вместо прибавления (режим этого хода).</summary>
        public bool DiceSubtractMode { get; private set; }

        public BattleContext(IRandomProvider rng,
                             int initialLimit,
                             IBossHooks bossHooks = null,
                             IBossModifiers bossMods = null)
        {
            Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            Dice = new DiceManager(rng);
            Deck = new DeckManager(rng);
            Stats = new PlayerStats();
            PowerMgr = new PowerManager();
            Events = new EventBus();
            Triggers = new TriggerRegistry(Events);
            InitialLimit = initialLimit;
            Limit = initialLimit;
            BossHooks = bossHooks;
            BossMods = bossMods ?? NullBossModifiers.Instance;
        }

        public void SetBoss(IBossHooks hooks, IBossModifiers mods)
        {
            BossHooks = hooks;
            BossMods = mods ?? NullBossModifiers.Instance;
        }

        /// <summary>
        /// Масштабирует начальный и текущий Лимит (боссы Диплос x2, Кайрос :2, Мономос :3).
        /// Обычно вызывается боссом в OnBattleStart.
        /// </summary>
        public void ScaleLimit(double factor)
        {
            InitialLimit = Math.Max(0, (int)Math.Round(InitialLimit * factor, MidpointRounding.AwayFromZero));
            Limit = Math.Max(0, (int)Math.Round(Limit * factor, MidpointRounding.AwayFromZero));
        }

        /// <summary>Изменяет текущий Лимит на delta (Эратосфен +12, Кер -12). Не ниже 0.</summary>
        public void AdjustLimit(int delta)
        {
            InitialLimit = Math.Max(0, InitialLimit + delta);
            Limit = Math.Max(0, Limit + delta);
        }

        // ================= Управление ходом =================

        public void BeginTurn()
        {
            TurnNumber++;
            Power = 0;
            ExiledThisTurn = false;
            DiscardedThisTurn = false;
            CardsPlayedThisTurn = 0;
            CardsDiscardedThisTurn = 0;
            CardsDrawnThisTurn = 0;
            LastPlayedCard = null;
            PlayHistoryThisTurn.Clear();
            DiceSubtractMode = false;
            Triggers.ClearTurnScoped();

            BossHooks?.OnTurnStart(this);
            Events.Emit(new TurnStartEvent(TurnNumber));
        }

        /// <summary>Бросок кубиков по характеристике Кости (с учётом отрицательного режима).</summary>
        public void RollDiceForTurn()
        {
            int dice = Stats.CurrentDice;
            DiceSubtractMode = dice < 0;
            int count = Math.Abs(dice);

            Dice.RollAll(count);

            // Отключаем кубики, запрещённые боссом.
            if (BossHooks != null)
                foreach (var d in Dice.Dice)
                    d.IsDisabled = BossHooks.IsDieValueDisabled(d.Value);

            Events.Emit(new DiceRolledEvent());

            // Эмитим «выпало X» и дубль по результатам броска.
            foreach (var d in Dice.ActiveDice)
                Events.Emit(new RolledValueEvent(d.Value));
            EmitDoubleIfPresent();

            RecomputePowerFromDice();
        }

        /// <summary>Конец хода: применить Мощь к Лимиту, обновить счётчики, исход считает движок.</summary>
        public void EndTurn()
        {
            BossHooks?.OnTurnEnd(this);
            Limit = PowerMgr.ApplyPowerToLimit(Limit, Power, InitialLimit, BossMods);
            Events.Emit(new TurnEndEvent());
        }

        /// <summary>Уборка после хода: рука в сброс, временные кубики убрать, счётчик ходов.</summary>
        public void ResolveTurn()
        {
            Deck.DiscardHand();
            Dice.RemoveTemporary();
            TurnsTakenVsEnemy++;
        }

        // ================= Мощь =================

        /// <summary>Единая точка изменения Мощи. Применяет операцию и эмитит PowerChanged.</summary>
        public void ModifyPower(PowerOp op, Card source = null)
        {
            int old = Power;
            Power = PowerMgr.Apply(Power, op, BossMods);
            if (Power != old)
                Events.Emit(new PowerChangedEvent(old, Power));
        }

        /// <summary>Прямая установка Мощи (для внутреннего пересчёта из кубиков).</summary>
        public void SetPowerRaw(int value)
        {
            int old = Power;
            Power = value;
            if (Power != old)
                Events.Emit(new PowerChangedEvent(old, Power));
        }

        /// <summary>
        /// Базовая Мощь от кубиков: сумма активных значений (в обычном режиме)
        /// или их отрицание (режим Кости &lt; 0). Не сбрасывает бонусы карт —
        /// используется только в момент броска, до розыгрыша карт.
        /// </summary>
        public void RecomputePowerFromDice()
        {
            int sum = Dice.SumOfActive();
            SetPowerRaw(DiceSubtractMode ? -sum : sum);
        }

        // ================= Кубики =================

        /// <summary>Единая точка изменения кубика. Эмитит DiceValueChanged ВСЕГДА.</summary>
        public DieChangeResult ChangeDie(Die die, DieOp op)
        {
            var result = Dice.Apply(die, op);

            // Пересчитать отключённость по новому значению.
            if (BossHooks != null)
                die.IsDisabled = BossHooks.IsDieValueDisabled(die.Value);

            // Событие изменения кубика — всегда, даже если значение то же.
            Events.Emit(new DiceValueChangedEvent(die, result.OldValue, result.NewValue));
            // «Выпало X» на новое значение.
            if (!die.IsDisabled)
                Events.Emit(new RolledValueEvent(die.Value));
            EmitDoubleIfPresent();

            BossHooks?.OnDiceChanged(this);

            // Изменение кубика меняет сумму → в момент броска Мощь пересчитывается,
            // но во время розыгрыша карт Мощь уже включает бонусы, поэтому пересчёт
            // базовой суммы не делаем здесь автоматически (карты сами учитывают).
            return result;
        }

        /// <summary>Добавить кубики (карты действий — временные на 1 ход).</summary>
        public void AddDice(int count, bool temporary, int? fixedValue = null)
        {
            for (int i = 0; i < count; i++)
            {
                Die die = fixedValue.HasValue
                    ? Dice.AddDie(fixedValue.Value, temporary)
                    : Dice.AddRolledDie(temporary);

                if (BossHooks != null)
                    die.IsDisabled = BossHooks.IsDieValueDisabled(die.Value);

                if (!die.IsDisabled)
                    Events.Emit(new RolledValueEvent(die.Value));
            }
            EmitDoubleIfPresent();
        }

        private void EmitDoubleIfPresent()
        {
            foreach (var d in Dice.ActiveDice)
            {
                if (CountActiveWithValue(d.Value) >= 2)
                {
                    Events.Emit(new DoubleFormedEvent(d.Value));
                    break;
                }
            }
        }

        private int CountActiveWithValue(int value)
        {
            int c = 0;
            foreach (var d in Dice.ActiveDice)
                if (d.Value == value) c++;
            return c;
        }

        // ================= Карты =================

        /// <summary>Добор n карт из колоды в руку с эмиссией событий и хуком OnDraw.</summary>
        public void DrawCards(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var card = Deck.DrawOne();
                if (card == null) break; // колода и сброс пусты
                CardsDrawnThisTurn++;
                card.OnDraw(this);
                Events.Emit(new CardDrawnEvent(card));
            }
        }

        /// <summary>
        /// Розыгрыш карты. Проверяет CanPlay (для «Если»), применяет штрафы босса,
        /// для «Когда» регистрирует триггеры, иначе исполняет Execute сразу.
        /// </summary>
        public bool PlayCard(Card card)
        {
            if (!card.CanPlay(this)) return false;

            LastPlayedCard = card;
            PlayHistoryThisTurn.Add(card);
            CardsPlayedThisTurn++;

            BossHooks?.OnCardPlayed(this);
            Events.Emit(new CardPlayedEvent(card));

            if (card.Condition == CardCondition.When)
                card.RegisterWhenTriggers(this);
            else
                card.Execute(this, new CardContext(card));

            // Карта уходит в сброс в конце хода (ResolveTurn), кроме изгнанных.
            Deck.DiscardFromHand(card);
            return true;
        }

        /// <summary>Сбросить карту из руки вручную (эмитит событие, вызывает OnDiscard).</summary>
        public void DiscardCard(Card card, bool manual)
        {
            card.OnDiscard(this);
            Deck.DiscardFromHand(card);
            CardsDiscardedThisTurn++;
            if (manual) DiscardedThisTurn = true;
            Events.Emit(new CardDiscardedEvent(card));
        }

        /// <summary>Сбросить n случайных карт из руки.</summary>
        public void DiscardRandomFromHand(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var card = Deck.RandomHandCard();
                if (card == null) break;
                DiscardCard(card, manual: false);
            }
        }

        /// <summary>Изгнать карту (до конца боя), снять её «Когда»-триггеры.</summary>
        public void ExileCard(Card card)
        {
            if (!card.CanBeExiled) return;
            Triggers.UnregisterByOwner(card);
            card.OnExile(this);
            Deck.ExileCard(card);
            ExiledThisTurn = true;
            Events.Emit(new CardExiledEvent(card));
        }

        /// <summary>Удалить карту навсегда.</summary>
        public void RemoveCard(Card card)
        {
            if (!card.CanBeRemoved) return;
            Triggers.UnregisterByOwner(card);
            card.OnRemove(this);
            Deck.RemoveCard(card);
            Events.Emit(new CardRemovedEvent(card));
        }
    }
}
