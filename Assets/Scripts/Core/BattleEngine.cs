using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Player;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Конечный автомат боя. Гоняет ходы согласно порядку фаз из плана, запрашивая
    /// действия у IPlayerController. Работает и с человеком (async ждёт UI),
    /// и с ИИ (синхронно). Уведомляет о смене фазы через опциональный callback (для UI).
    /// </summary>
    public sealed class BattleEngine
    {
        private readonly BattleContext _ctx;
        private readonly IPlayerController _controller;

        /// <summary>Уведомление о смене фазы (для UI/логов). Может быть null.</summary>
        public Action<TurnPhase> OnPhaseChanged;

        /// <summary>Предохранитель от зависания на бесконечном PlayLoop (число решений за ход).</summary>
        public int MaxDecisionsPerTurn { get; set; } = 200;

        public BattleEngine(BattleContext ctx, IPlayerController controller)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        /// <summary>Провести бой целиком. deck — карты личной колоды на этот бой.</summary>
        public async Task<BattleResult> RunBattleAsync(IEnumerable<Card> deck)
        {
            // --- BattleStart ---
            OnPhaseChanged?.Invoke(TurnPhase.BattleStart);
            _ctx.Stats.ResetToBase();
            _ctx.Deck.LoadDeck(deck);
            _ctx.Deck.ShuffleDeck();
            _ctx.BossHooks?.OnBattleStart(_ctx);

            var outcome = BattleOutcome.InProgress;

            while (outcome == BattleOutcome.InProgress)
            {
                // --- TurnStart ---
                OnPhaseChanged?.Invoke(TurnPhase.TurnStart);
                _ctx.BeginTurn();

                // --- Draw ---
                OnPhaseChanged?.Invoke(TurnPhase.Draw);
                _ctx.DrawCards(_ctx.Stats.CurrentDraw);

                // --- Roll ---
                OnPhaseChanged?.Invoke(TurnPhase.Roll);
                _ctx.RollDiceForTurn();

                // --- PlayLoop ---
                OnPhaseChanged?.Invoke(TurnPhase.PlayLoop);
                await RunPlayLoopAsync();

                // --- TurnEnd ---
                OnPhaseChanged?.Invoke(TurnPhase.TurnEnd);
                _ctx.EndTurn();

                // --- Resolve ---
                OnPhaseChanged?.Invoke(TurnPhase.Resolve);
                _ctx.ResolveTurn();

                outcome = EvaluateOutcome();
            }

            return new BattleResult(outcome, _ctx.TurnsTakenVsEnemy, _ctx.Limit);
        }

        private async Task RunPlayLoopAsync()
        {
            for (int i = 0; i < MaxDecisionsPerTurn; i++)
            {
                var action = await _controller.DecideAsync(_ctx);
                if (action.Type == PlayerActionType.EndTurn)
                    return;

                ApplyAction(action);
            }
        }

        private void ApplyAction(PlayerAction action)
        {
            switch (action.Type)
            {
                case PlayerActionType.PlayCard:
                    if (action.Target != null) _ctx.PlayCard(action.Target);
                    break;
                case PlayerActionType.DiscardCard:
                    if (action.Target != null) _ctx.DiscardCard(action.Target, manual: true);
                    break;
                case PlayerActionType.ExileCard:
                    if (action.Target != null) _ctx.ExileCard(action.Target);
                    break;
                case PlayerActionType.EndTurn:
                    break;
            }
        }

        /// <summary>
        /// Исход после хода: сначала проверяем победу (можно выиграть на последнем ходу),
        /// затем — исчерпание ходов по Скорости.
        /// </summary>
        private BattleOutcome EvaluateOutcome()
        {
            if (_ctx.Limit <= 0)
                return BattleOutcome.Win;
            if (_ctx.TurnsTakenVsEnemy >= _ctx.Stats.CurrentSpeed)
                return BattleOutcome.Lose;
            return BattleOutcome.InProgress;
        }
    }
}
