using System.Linq;
using System.Threading.Tasks;
using ArithmosRequiem.Cards;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Player
{
    /// <summary>
    /// Простой жадный ИИ для отладки и авто-прогонов боя. Играет по одной карте за
    /// решение: выбирает разыгрываемую карту, которая (по грубой эвристике) не вредит,
    /// иначе завершает ход. Не идеален по балансу — цель в детерминированной проверке движка.
    /// </summary>
    public sealed class AIController : IPlayerController
    {
        public Task<PlayerAction> DecideAsync(BattleContext ctx)
        {
            // Кандидаты — карты руки, которые можно разыграть прямо сейчас.
            var playable = ctx.Deck.Hand
                .Where(c => c.CanBePlayed && c.CanPlay(ctx))
                .ToList();

            if (playable.Count == 0)
                return Task.FromResult(PlayerAction.End());

            // Приоритет: безусловные и «Если»-плюсы к Мощи первыми; «Когда» — тоже играем
            // (регистрируют триггеры). Простейшая эвристика — играть первую доступную.
            var choice = playable
                .OrderByDescending(EstimateBenefit)
                .First();

            if (EstimateBenefit(choice) <= 0)
                return Task.FromResult(PlayerAction.End());

            return Task.FromResult(PlayerAction.Play(choice));
        }

        /// <summary>Грубая оценка полезности карты (без симуляции состояния).</summary>
        private static int EstimateBenefit(Card card)
        {
            // Эвристика по типу условия: безусловные полезны, «Если» контекстно,
            // «Когда» умеренно (даёт потенциал). Реальную оценку заменим симуляцией позже.
            switch (card.Condition)
            {
                case CardCondition.None: return 3;
                case CardCondition.If: return 2;
                case CardCondition.When: return 1;
                default: return 0;
            }
        }
    }
}
