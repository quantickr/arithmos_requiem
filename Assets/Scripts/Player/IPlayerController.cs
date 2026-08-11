using System.Threading.Tasks;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Player
{
    /// <summary>
    /// Источник решений в фазе розыгрыша. Async-модель одинаково обслуживает
    /// ручной режим (ждёт ввод UI) и авто-режим (ИИ считает синхронно).
    /// </summary>
    public interface IPlayerController
    {
        Task<PlayerAction> DecideAsync(BattleContext ctx);
    }
}
