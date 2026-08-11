using System;
using ArithmosRequiem.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace ArithmosRequiem.View
{
    /// <summary>
    /// Визуал одной карты в руке: скруглённая «лицевая» панель, заголовок и текст правил.
    /// Клик по карте вызывает onClick с моделью карты. Играбельность подсвечивается рамкой.
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        private Card _card;
        private Action<Card> _onClick;
        private Image _face;
        private Image _border;
        private Text _title;
        private Text _rules;

        public Card Card => _card;

        public static CardView Create(Transform parent, Card card, Action<Card> onClick)
        {
            var rt = UiBuilder.Panel(parent, $"Card_{card.DefinitionId}", UiTheme.CardBorder, 18);
            rt.sizeDelta = new Vector2(UiTheme.CardWidth, UiTheme.CardHeight);

            var view = rt.gameObject.AddComponent<CardView>();
            view._card = card;
            view._onClick = onClick;
            view._border = rt.GetComponent<Image>();

            // Лицевая часть (чуть внутри рамки).
            var faceColor = card.Kind == CardKind.Starting ? UiTheme.CardFaceStarting : UiTheme.CardFace;
            var faceRt = UiBuilder.Panel(rt, "Face", faceColor, 14);
            UiBuilder.Stretch(faceRt, 5);
            view._face = faceRt.GetComponent<Image>();

            // Заголовок (id/название сверху).
            view._title = UiBuilder.Label(faceRt, "Title", card.DisplayName, 18, UiTheme.CardText,
                TextAnchor.UpperCenter);
            UiBuilder.SetAnchor(view._title.rectTransform,
                new Vector2(0, 0.72f), new Vector2(1, 1),
                new Vector2(8, 0), new Vector2(-8, -8));

            // Текст правил (по центру-низу).
            view._rules = UiBuilder.Label(faceRt, "Rules", card.RulesText, 15, UiTheme.CardText,
                TextAnchor.MiddleCenter);
            UiBuilder.SetAnchor(view._rules.rectTransform,
                new Vector2(0, 0.08f), new Vector2(1, 0.72f),
                new Vector2(8, 0), new Vector2(-8, 0));

            // Кликабельность.
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = view._border;
            btn.onClick.AddListener(view.HandleClick);

            return view;
        }

        private void HandleClick() => _onClick?.Invoke(_card);

        /// <summary>Подсветка играбельности: играбельная — золотая рамка, иначе — тёмная.</summary>
        public void SetPlayable(bool playable)
        {
            _border.color = playable ? UiTheme.AccentGold : UiTheme.CardBorder;
        }
    }
}
