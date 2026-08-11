using System;
using System.Collections.Generic;
using System.Linq;

namespace ArithmosRequiem.Bosses
{
    /// <summary>
    /// Реестр боссов: сопоставляет BossId с фабрикой экземпляра. Чистый C#.
    /// Наполняется на слое загрузки (Register/Register&lt;T&gt;).
    /// </summary>
    public sealed class BossRegistry
    {
        private readonly Dictionary<string, Func<Boss>> _factories =
            new Dictionary<string, Func<Boss>>(StringComparer.Ordinal);

        public void Register(string bossId, Func<Boss> factory)
        {
            if (string.IsNullOrEmpty(bossId))
                throw new ArgumentException("bossId не может быть пустым");
            _factories[bossId] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void Register<T>() where T : Boss, new()
        {
            var probe = new T();
            Register(probe.BossId, () => new T());
        }

        public bool Contains(string bossId) => _factories.ContainsKey(bossId);

        public Boss Create(string bossId)
        {
            if (!_factories.TryGetValue(bossId, out var factory))
                throw new KeyNotFoundException($"Босс не зарегистрирован: {bossId}");
            return factory();
        }

        public bool TryCreate(string bossId, out Boss boss)
        {
            if (_factories.TryGetValue(bossId, out var factory))
            {
                boss = factory();
                return true;
            }
            boss = null;
            return false;
        }

        public IReadOnlyCollection<string> AllIds => _factories.Keys.ToArray();
        public int Count => _factories.Count;
    }
}
