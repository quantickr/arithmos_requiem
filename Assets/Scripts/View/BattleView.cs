using System;
using System.Collections.Generic;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Процедурный uGUI-экран боя в стиле Balatro. Строит Canvas, панели статуса
    /// (Мощь/Лимит/фаза/характеристики), ряд кубиков и веер карт руки. Обновляется
    /// из BattleRunner методом Refresh(ctx). Клики по картам и кнопки транслируются наружу.
    /// </summary>
    public sealed class BattleView : MonoBehaviour
    {
        public event Action<Card> OnCardClicked;
        public event Action OnEndTurnClicked;
        public event Action OnRestartClicked;

        private Text _powerLabel;
        private Text _limitLabel;
        private Text _phaseLabel;
        private Text _statsLabel;
        private Text _bossLabel;
        private Text _logLabel;

        private RectTransform _diceRow;
        private RectTransform _handRow;
        private readonly List<GameObject> _spawned = new List<GameObject>();

        public static BattleView Create()
        {
            var canvasGo = new GameObject("BattleCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            var view = canvasGo.AddComponent<BattleView>();
            view.BuildLayout(canvasGo.transform);
            return view;
        }

        private void BuildLayout(Transform root)
        {
            // Фон.
            var bg = UiBuilder.Panel(root, "Background", UiTheme.Background, 0);
            UiBuilder.Stretch(bg);

            // Верхняя панель статуса.
            var top = UiBuilder.Panel(root, "TopBar", UiTheme.PanelDark, 16);
            UiBuilder.SetAnchor(top, new Vector2(0, 0.86f), new Vector2(1, 1),
                new Vector2(16, -8), new Vector2(-16, -16));

            _limitLabel = UiBuilder.Label(top, "Limit", "Лимит: —", 34, UiTheme.AccentRed,
                TextAnchor.MiddleLeft);
            UiBuilder.SetAnchor(_limitLabel.rectTransform, new Vector2(0, 0), new Vector2(0.3f, 1),
                new Vector2(20, 0), new Vector2(0, 0));

            _powerLabel = UiBuilder.Label(top, "Power", "Мощь: 0", 34, UiTheme.AccentGold,
                TextAnchor.MiddleCenter);
            UiBuilder.SetAnchor(_powerLabel.rectTransform, new Vector2(0.3f, 0), new Vector2(0.6f, 1),
                Vector2.zero, Vector2.zero);

            _phaseLabel = UiBuilder.Label(top, "Phase", "Фаза: —", 22, UiTheme.TextDim,
                TextAnchor.MiddleRight);
            UiBuilder.SetAnchor(_phaseLabel.rectTransform, new Vector2(0.6f, 0.5f), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(-20, 0));

            _statsLabel = UiBuilder.Label(top, "Stats", "Добор — · Скорость — · Кости —", 18,
                UiTheme.TextDim, TextAnchor.MiddleRight);
            UiBuilder.SetAnchor(_statsLabel.rectTransform, new Vector2(0.6f, 0), new Vector2(1, 0.5f),
                new Vector2(0, 0), new Vector2(-20, 0));

            // Панель босса (второй сверху).
            _bossLabel = UiBuilder.Label(root, "Boss", "", 20, UiTheme.AccentBlue, TextAnchor.MiddleCenter);
            UiBuilder.SetAnchor(_bossLabel.rectTransform, new Vector2(0, 0.8f), new Vector2(1, 0.855f),
                new Vector2(16, 0), new Vector2(-16, 0));

            // Ряд кубиков (центр).
            var diceHolder = UiBuilder.Panel(root, "DiceHolder", UiTheme.PanelDark, 16);
            UiBuilder.SetAnchor(diceHolder, new Vector2(0, 0.52f), new Vector2(1, 0.78f),
                new Vector2(16, 0), new Vector2(-16, 0));
            _diceRow = BuildHorizontalRow(diceHolder, "DiceRow", UiTheme.DieSize + 12);

            // Ряд руки (низ).
            var handHolder = UiBuilder.Panel(root, "HandHolder", UiTheme.PanelDark, 16);
            UiBuilder.SetAnchor(handHolder, new Vector2(0, 0.1f), new Vector2(1, 0.5f),
                new Vector2(16, 0), new Vector2(-16, 0));
            _handRow = BuildHorizontalRow(handHolder, "HandRow", UiTheme.CardHeight + 12);

            // Нижняя панель кнопок и лога.
            var bottom = UiBuilder.Panel(root, "BottomBar", UiTheme.PanelDark, 16);
            UiBuilder.SetAnchor(bottom, new Vector2(0, 0), new Vector2(1, 0.09f),
                new Vector2(16, 16), new Vector2(-16, 0));

            var endBtn = UiBuilder.Button(bottom, "EndTurn", "Завершить ход",
                UiTheme.AccentGreen, UiTheme.CardText, out _);
            UiBuilder.SetAnchor(endBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0.15f), new Vector2(0.22f, 0.85f),
                new Vector2(16, 0), new Vector2(0, 0));
            endBtn.onClick.AddListener(() => OnEndTurnClicked?.Invoke());

            var restartBtn = UiBuilder.Button(bottom, "Restart", "Заново",
                UiTheme.AccentBlue, UiTheme.CardText, out _);
            UiBuilder.SetAnchor(restartBtn.GetComponent<RectTransform>(),
                new Vector2(0.23f, 0.15f), new Vector2(0.4f, 0.85f),
                new Vector2(8, 0), new Vector2(0, 0));
            restartBtn.onClick.AddListener(() => OnRestartClicked?.Invoke());

            _logLabel = UiBuilder.Label(bottom, "Log", "", 18, UiTheme.TextLight, TextAnchor.MiddleLeft);
            UiBuilder.SetAnchor(_logLabel.rectTransform, new Vector2(0.41f, 0), new Vector2(1, 1),
                new Vector2(16, 0), new Vector2(-16, 0));
        }

        private static RectTransform BuildHorizontalRow(Transform parent, string name, float height)
        {
            var rt = UiBuilder.Panel(parent, name, new Color(0, 0, 0, 0), 0);
            UiBuilder.Stretch(rt, 12);

            var layout = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = UiTheme.CardSpacing;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            return rt;
        }

        /// <summary>Полное обновление UI по текущему состоянию боя.</summary>
        public void Refresh(BattleContext ctx, TurnPhase phase, bool interactable)
        {
            _limitLabel.text = $"Лимит: {ctx.Limit}";
            _powerLabel.text = $"Мощь: {ctx.Power}";
            _phaseLabel.text = $"Фаза: {PhaseName(phase)}";
            _statsLabel.text =
                $"Добор {ctx.Stats.CurrentDraw} · Скорость {ctx.Stats.CurrentSpeed} · " +
                $"Кости {ctx.Stats.CurrentDice} · Ход {ctx.TurnNumber}/{ctx.Stats.CurrentSpeed}";

            _bossLabel.text = ctx.BossHooks is Bosses.Boss b
                ? $"Босс: {b.DisplayName} — {b.AbilityText}"
                : "Босс: нет";

            RebuildDice(ctx);
            RebuildHand(ctx, interactable);
        }

        private void RebuildDice(BattleContext ctx)
        {
            foreach (var d in ctx.Dice.Dice)
                DieView.Create(_diceRow, d);
        }

        private void RebuildHand(BattleContext ctx, bool interactable)
        {
            foreach (var card in ctx.Deck.Hand)
            {
                var cv = CardView.Create(_handRow, card, c => OnCardClicked?.Invoke(c));
                bool playable = interactable && card.CanBePlayed && card.CanPlay(ctx);
                cv.SetPlayable(playable);
            }
        }

        /// <summary>Пересобрать динамические ряды (вызывается перед Refresh каждого кадра обновления).</summary>
        public void ClearDynamic()
        {
            ClearChildren(_diceRow);
            ClearChildren(_handRow);
        }

        private static void ClearChildren(RectTransform row)
        {
            if (row == null) return;
            for (int i = row.childCount - 1; i >= 0; i--)
                Destroy(row.GetChild(i).gameObject);
        }

        public void SetLog(string message) => _logLabel.text = message;

        private static string PhaseName(TurnPhase p) => p switch
        {
            TurnPhase.BattleStart => "Начало боя",
            TurnPhase.TurnStart => "Начало хода",
            TurnPhase.Draw => "Добор",
            TurnPhase.Roll => "Бросок",
            TurnPhase.PlayLoop => "Розыгрыш",
            TurnPhase.TurnEnd => "Конец хода",
            TurnPhase.Resolve => "Подсчёт",
            _ => p.ToString()
        };
    }
}
