using System.Collections.Generic;
using ArithmosRequiem.Bosses;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Core;
using ArithmosRequiem.Data;
using ArithmosRequiem.Player;
using ArithmosRequiem.Systems.Events;
using UnityEngine;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Точка входа боевой сцены. Собирает BattleContext (враг + опциональный босс),
    /// строит процедурный UI и гоняет BattleEngine. Поддерживает два режима управления:
    /// человек (клики по картам → HumanController) и ИИ (авто-прогон для отладки).
    /// Все обновления UI идут на главном потоке Unity через колбэки движка/событий.
    /// </summary>
    public sealed class BattleRunner : MonoBehaviour
    {
        [Header("Настройки боя")]
        [Tooltip("Базовый Лимит врага (по правилам глава 1 = 12).")]
        public int enemyBaseLimit = 12;

        [Tooltip("Id босса из BossRegistry. Пусто — бой без босса.")]
        public string bossId = "";

        [Tooltip("Включить ИИ-режим (авто-прогон для разработчика).")]
        public bool aiMode = false;

        [Tooltip("Сид RNG (0 = случайный по времени).")]
        public int seed = 0;

        private BattleView _view;
        private BattleContext _ctx;
        private HumanController _human;
        private TurnPhase _phase;
        private bool _battleRunning;

        private void Start() => StartBattle();

        private void StartBattle()
        {
            if (_view == null)
            {
                _view = BattleView.Create();
                _view.OnCardClicked += HandleCardClicked;
                _view.OnEndTurnClicked += HandleEndTurn;
                _view.OnRestartClicked += HandleRestart;
            }

            IRandomProvider rng = seed != 0
                ? new SystemRandomProvider(seed)
                : new SystemRandomProvider();

            _ctx = new BattleContext(rng, enemyBaseLimit);

            var bossRegistry = ContentFactory.BuildBossRegistry();
            if (!string.IsNullOrEmpty(bossId) && bossRegistry.TryCreate(bossId, out Boss boss))
                _ctx.SetBoss(boss, boss);

            // Лог розыгрыша/добора — для наглядности.
            _ctx.Events.Subscribe(BattleEventType.CardPlayed, OnAnyMutation);
            _ctx.Events.Subscribe(BattleEventType.CardDrawn, OnAnyMutation);
            _ctx.Events.Subscribe(BattleEventType.PowerChanged, OnAnyMutation);
            _ctx.Events.Subscribe(BattleEventType.DiceValueChanged, OnAnyMutation);

            IPlayerController controller;
            if (aiMode)
            {
                controller = new AIController();
            }
            else
            {
                _human = new HumanController();
                controller = _human;
            }

            var engine = new BattleEngine(_ctx, controller);
            engine.OnPhaseChanged = OnPhaseChanged;

            _battleRunning = true;
            RunAsync(engine);
        }

        private async void RunAsync(BattleEngine engine)
        {
            var deck = ContentFactory.BuildStartingDeck();
            var result = await engine.RunBattleAsync(deck);
            _battleRunning = false;

            RefreshUi();
            string verdict = result.Outcome switch
            {
                BattleOutcome.Win => "ПОБЕДА! Лимит повержен.",
                BattleOutcome.Lose => "Поражение: ходы закончились.",
                _ => "Бой завершён."
            };
            _view.SetLog($"{verdict}  (ходов: {result.TurnsTaken}, Лимит: {result.FinalLimit})");
        }

        // --- Колбэки движка/событий ---

        private void OnPhaseChanged(TurnPhase phase)
        {
            _phase = phase;
            RefreshUi();
        }

        private void OnAnyMutation(BattleEvent _) => RefreshUi();

        private void RefreshUi()
        {
            if (_view == null || _ctx == null) return;
            _view.ClearDynamic();
            bool interactable = _battleRunning && !aiMode &&
                                _human != null && _human.IsWaiting;
            _view.Refresh(_ctx, _phase, interactable);
        }

        // --- Ввод игрока ---

        private void HandleCardClicked(Card card)
        {
            if (aiMode || _human == null || !_human.IsWaiting) return;
            if (!card.CanBePlayed || !card.CanPlay(_ctx)) return;
            _human.Submit(PlayerAction.Play(card));
        }

        private void HandleEndTurn()
        {
            if (aiMode || _human == null || !_human.IsWaiting) return;
            _human.Submit(PlayerAction.End());
        }

        private void HandleRestart()
        {
            // Простой перезапуск: пересоздаём состояние (UI переиспользуется).
            _battleRunning = false;
            StartBattle();
        }
    }
}
