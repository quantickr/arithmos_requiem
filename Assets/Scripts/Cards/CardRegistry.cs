using System;
using System.Collections.Generic;
using System.Linq;

namespace ArithmosRequiem.Cards
{
    /// <summary>
    /// Реестр всех определений карт: сопоставляет строковый DefinitionId с фабрикой,
    /// создающей новый экземпляр карты. Чистый C# — Unity-независим, наполняется
    /// вручную (Register) или автоматически (RegisterAllFromAssembly через рефлексию
    /// на слое View/загрузчика). Здесь — только хранилище и фабрики.
    /// </summary>
    public sealed class CardRegistry
    {
        private readonly Dictionary<string, Func<Card>> _factories =
            new Dictionary<string, Func<Card>>(StringComparer.Ordinal);

        /// <summary>Зарегистрировать фабрику карты по её id.</summary>
        public void Register(string definitionId, Func<Card> factory)
        {
            if (string.IsNullOrEmpty(definitionId))
                throw new ArgumentException("definitionId не может быть пустым");
            _factories[definitionId] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Зарегистрировать тип карты (должен иметь конструктор без параметров).
        /// Создаёт временный экземпляр, чтобы прочитать DefinitionId.
        /// </summary>
        public void Register<T>() where T : Card, new()
        {
            var probe = new T();
            Register(probe.DefinitionId, () => new T());
        }

        /// <summary>Есть ли карта с таким id.</summary>
        public bool Contains(string definitionId) => _factories.ContainsKey(definitionId);

        /// <summary>Создать новый экземпляр карты по id. Бросает, если id неизвестен.</summary>
        public Card Create(string definitionId)
        {
            if (!_factories.TryGetValue(definitionId, out var factory))
                throw new KeyNotFoundException($"Карта не зарегистрирована: {definitionId}");
            return factory();
        }

        /// <summary>Попытаться создать карту; false, если id неизвестен.</summary>
        public bool TryCreate(string definitionId, out Card card)
        {
            if (_factories.TryGetValue(definitionId, out var factory))
            {
                card = factory();
                return true;
            }
            card = null;
            return false;
        }

        /// <summary>Все зарегистрированные id (для инспекции/отладки).</summary>
        public IReadOnlyCollection<string> AllIds => _factories.Keys.ToArray();

        public int Count => _factories.Count;
    }
}
