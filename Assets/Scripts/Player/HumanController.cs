using System.Threading;
using System.Threading.Tasks;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Player
{
    /// <summary>
    /// Контроллер живого игрока. UI-слой вызывает Submit(action), когда игрок нажал
    /// кнопку/сыграл карту. DecideAsync ждёт этого ввода.
    /// </summary>
    public sealed class HumanController : IPlayerController
    {
        private TaskCompletionSource<PlayerAction> _pending;

        public Task<PlayerAction> DecideAsync(BattleContext ctx)
        {
            _pending = new TaskCompletionSource<PlayerAction>();
            return _pending.Task;
        }

        /// <summary>Вызывается UI для передачи выбранного действия игрока.</summary>
        public void Submit(PlayerAction action)
        {
            _pending?.TrySetResult(action);
            _pending = null;
        }

        /// <summary>Ожидается ли сейчас ввод.</summary>
        public bool IsWaiting => _pending != null && !_pending.Task.IsCompleted;
    }
}
