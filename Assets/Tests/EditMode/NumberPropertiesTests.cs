using ArithmosRequiem.Data;
using ArithmosRequiem.Utils;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    public class NumberPropertiesTests
    {
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        [TestCase(17, true)]
        [TestCase(1, false)]
        [TestCase(0, false)]
        [TestCase(-7, false)]
        public void IsPrime_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsPrime(n));
        }

        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(4, true)]
        [TestCase(9, true)]
        [TestCase(16, true)]
        [TestCase(2, false)]
        [TestCase(-4, false)]
        public void IsSquare_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsSquare(n));
        }

        [TestCase(-1, true)]
        [TestCase(-4, true)]
        [TestCase(-9, true)]
        [TestCase(-2, false)]
        [TestCase(4, false)]
        [TestCase(0, false)]
        public void IsNegativeSquare_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsNegativeSquare(n));
        }

        [TestCase(1, true)]
        [TestCase(8, true)]
        [TestCase(27, true)]
        [TestCase(-8, true)]
        [TestCase(9, false)]
        public void IsCube_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsCube(n));
        }

        [TestCase(4, true)]   // 2^2
        [TestCase(9, true)]   // 3^2
        [TestCase(25, true)]  // 5^2
        [TestCase(16, false)] // 4^2, 4 не простое
        [TestCase(6, false)]
        public void IsPrimeSquared_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsPrimeSquared(n));
        }

        [TestCase(121, true)]
        [TestCase(1, true)]
        [TestCase(22, true)]
        [TestCase(123, false)]
        [TestCase(-121, true)] // знак игнорируется
        public void IsPalindrome_Works(int n, bool expected)
        {
            Assert.AreEqual(expected, NumberProperties.IsPalindrome(n));
        }

        [TestCase(0, 0)]
        [TestCase(16, 7)]
        [TestCase(1234, 10)]
        [TestCase(-25, 7)]
        public void DigitSum_Works(int n, int expected)
        {
            Assert.AreEqual(expected, NumberProperties.DigitSum(n));
        }

        [Test]
        public void Next_ReturnsStrictlyGreaterPrime()
        {
            // Ближайшее простое строго больше 7 → 11.
            int result = NumberProperties.Next(7, NumberProperties.IsPrime);
            Assert.AreEqual(11, result);
        }

        [Test]
        public void Next_SkipsCurrentEvenIfItSatisfies()
        {
            // 4 — квадрат, но Next должен вернуть строго большее → 9.
            int result = NumberProperties.Next(4, NumberProperties.IsSquare);
            Assert.AreEqual(9, result);
        }

        [Test]
        public void Prev_ReturnsStrictlyLessPrime()
        {
            // Ближайшее простое строго меньше 8 → 7.
            int result = NumberProperties.Prev(8, NumberProperties.IsPrime);
            Assert.AreEqual(7, result);
        }

        [Test]
        public void Prev_CanGoNegativeForNegativeSquare()
        {
            // Ближайший отрицательный квадрат строго меньше 0 → -1.
            int result = NumberProperties.Prev(0, NumberProperties.IsNegativeSquare);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void BuildPredicate_MultipleOfN_UsesParam()
        {
            var pred = NumberProperties.BuildPredicate(NumberPropertyType.MultipleOfN, 3);
            Assert.IsTrue(pred(9));
            Assert.IsFalse(pred(10));
        }

        [Test]
        public void BuildPredicate_DigitSumEquals_UsesParam()
        {
            var pred = NumberProperties.BuildPredicate(NumberPropertyType.DigitSumEquals, 7);
            Assert.IsTrue(pred(16)); // 1+6=7
            Assert.IsFalse(pred(18));
        }
    }
}
