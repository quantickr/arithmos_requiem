using System.Collections.Generic;
using ArithmosRequiem.Core;

namespace ArithmosRequiem.Tests
{
    /// <summary>
    /// Детерминированный RNG для тестов. Отдаёт заранее заданную очередь значений
    /// (по кругу). Позволяет управлять и бросками d6, и Range независимо.
    /// </summary>
    public sealed class FakeRandomProvider : IRandomProvider
    {
        private readonly int[] _d6Sequence;
        private int _d6Index;

        private readonly int[] _rangeSequence;
        private int _rangeIndex;

        /// <param name="d6Sequence">Значения, которые вернёт RollD6 по порядку (по кругу).</param>
        /// <param name="rangeSequence">Значения для Range (по кругу). Если null — вернётся minInclusive.</param>
        public FakeRandomProvider(int[] d6Sequence = null, int[] rangeSequence = null)
        {
            _d6Sequence = (d6Sequence != null && d6Sequence.Length > 0)
                ? d6Sequence
                : new[] { 1, 2, 3, 4, 5, 6 };
            _rangeSequence = rangeSequence;
        }

        public int Range(int minInclusive, int maxInclusive)
        {
            if (_rangeSequence == null || _rangeSequence.Length == 0)
                return minInclusive;

            int raw = _rangeSequence[_rangeIndex % _rangeSequence.Length];
            _rangeIndex++;

            // Клампим в допустимый диапазон, чтобы тест не выходил за границы.
            if (raw < minInclusive) return minInclusive;
            if (raw > maxInclusive) return maxInclusive;
            return raw;
        }

        public int RollD6()
        {
            int v = _d6Sequence[_d6Index % _d6Sequence.Length];
            _d6Index++;
            return v;
        }
    }
}
