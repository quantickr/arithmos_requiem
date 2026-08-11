using System;
using System.Collections.Generic;

namespace ArithmosRequiem.Systems.Events
{
    /// <summary>
    /// Синхронная шина событий боя. Слушатели вызываются в порядке подписки.
    /// Есть защита от бесконечного каскада (когда обработчик события меняет состояние
    /// и порождает новое событие, которое снова триггерит эффект и т.д.).
    /// </summary>
    public sealed class EventBus
    {
        /// <summary>Максимальная глубина вложенной эмиссии, чтобы не зациклиться.</summary>
        public int MaxCascadeDepth { get; set; } = 64;

        private readonly Dictionary<BattleEventType, List<Action<BattleEvent>>> _listeners
            = new Dictionary<BattleEventType, List<Action<BattleEvent>>>();

        private int _currentDepth;

        public void Subscribe(BattleEventType type, Action<BattleEvent> handler)
        {
            if (!_listeners.TryGetValue(type, out var list))
            {
                list = new List<Action<BattleEvent>>();
                _listeners[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe(BattleEventType type, Action<BattleEvent> handler)
        {
            if (_listeners.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        /// <summary>
        /// Эмиссия события всем подписчикам. Копия списка на время вызова, чтобы
        /// обработчики могли подписываться/отписываться без нарушения итерации.
        /// </summary>
        public void Emit(BattleEvent evt)
        {
            if (_currentDepth >= MaxCascadeDepth)
                return; // защита от бесконечной рекурсии триггеров

            if (!_listeners.TryGetValue(evt.Type, out var list) || list.Count == 0)
                return;

            _currentDepth++;
            try
            {
                // Снимок, т.к. обработчик может изменить список подписок.
                var snapshot = list.ToArray();
                foreach (var handler in snapshot)
                    handler(evt);
            }
            finally
            {
                _currentDepth--;
            }
        }

        /// <summary>Снять все подписки (конец боя).</summary>
        public void Clear() => _listeners.Clear();
    }
}
