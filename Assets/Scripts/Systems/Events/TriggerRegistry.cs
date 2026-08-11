using System;
using System.Collections.Generic;

namespace ArithmosRequiem.Systems.Events
{
    /// <summary>
    /// Один зарегистрированный триггер «Когда»: подписан на тип события, проверяет
    /// предикат и при истинности запускает эффект. Может быть привязан к карте (owner)
    /// и сниматься в конце хода (карты действий с «Когда»).
    /// </summary>
    public sealed class WhenTrigger
    {
        public BattleEventType EventType { get; }
        public Predicate<BattleEvent> Condition { get; }
        public Action<BattleEvent> Effect { get; }
        public object Owner { get; }        // карта-источник (или null для боссов)
        public bool ExpiresAtTurnEnd { get; }

        public WhenTrigger(BattleEventType eventType,
                           Predicate<BattleEvent> condition,
                           Action<BattleEvent> effect,
                           object owner,
                           bool expiresAtTurnEnd)
        {
            EventType = eventType;
            Condition = condition ?? (_ => true);
            Effect = effect;
            Owner = owner;
            ExpiresAtTurnEnd = expiresAtTurnEnd;
        }
    }

    /// <summary>
    /// Реестр активных «Когда»-триггеров. Подключается к EventBus и вызывает эффекты
    /// при совпадении условия. Триггеры хода снимаются в конце хода.
    /// </summary>
    public sealed class TriggerRegistry
    {
        private readonly EventBus _bus;
        private readonly List<WhenTrigger> _triggers = new List<WhenTrigger>();
        private readonly HashSet<BattleEventType> _subscribed = new HashSet<BattleEventType>();

        public TriggerRegistry(EventBus bus)
        {
            _bus = bus;
        }

        public IReadOnlyList<WhenTrigger> Triggers => _triggers;

        /// <summary>Зарегистрировать триггер и подписаться на его тип события (один раз на тип).</summary>
        public void Register(WhenTrigger trigger)
        {
            _triggers.Add(trigger);
            if (_subscribed.Add(trigger.EventType))
                _bus.Subscribe(trigger.EventType, Dispatch);
        }

        /// <summary>Снять триггер (например, изгнали карту-источник).</summary>
        public void Unregister(WhenTrigger trigger) => _triggers.Remove(trigger);

        /// <summary>Снять все триггеры, привязанные к конкретному владельцу (карта).</summary>
        public void UnregisterByOwner(object owner)
        {
            _triggers.RemoveAll(t => ReferenceEquals(t.Owner, owner));
        }

        /// <summary>Конец хода: снять триггеры, помеченные ExpiresAtTurnEnd.</summary>
        public void ClearTurnScoped()
        {
            _triggers.RemoveAll(t => t.ExpiresAtTurnEnd);
        }

        /// <summary>Полная очистка (конец боя).</summary>
        public void ClearAll()
        {
            _triggers.Clear();
            foreach (var type in _subscribed)
                _bus.Unsubscribe(type, Dispatch);
            _subscribed.Clear();
        }

        private void Dispatch(BattleEvent evt)
        {
            // Снимок, т.к. эффект может изменить список триггеров.
            var snapshot = _triggers.ToArray();
            foreach (var trigger in snapshot)
            {
                if (trigger.EventType != evt.Type) continue;
                if (!_triggers.Contains(trigger)) continue; // мог быть снят по ходу
                if (trigger.Condition(evt))
                    trigger.Effect(evt);
            }
        }
    }
}
