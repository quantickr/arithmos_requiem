using System;

namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Реализация IRandomProvider на System.Random. Поддерживает сид для детерминизма.
    /// </summary>
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random;

        public SystemRandomProvider()
        {
            _random = new Random();
        }

        public SystemRandomProvider(int seed)
        {
            _random = new Random(seed);
        }

        public int Range(int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive)
                (minInclusive, maxInclusive) = (maxInclusive, minInclusive);
            return _random.Next(minInclusive, maxInclusive + 1);
        }

        public int RollD6() => _random.Next(1, 7);
    }
}
