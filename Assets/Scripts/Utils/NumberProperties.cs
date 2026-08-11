using System;
using ArithmosRequiem.Data;

namespace ArithmosRequiem.Utils
{
    /// <summary>
    /// Проверки свойств чисел и поиск ближайшего числа с заданным свойством.
    /// Чистый C#, без зависимостей от Unity — тестируется в EditMode.
    /// </summary>
    public static class NumberProperties
    {
        /// <summary>
        /// Предел поиска для Next/Prev, чтобы избежать бесконечного цикла,
        /// если числа с нужным свойством нет в разумном диапазоне.
        /// </summary>
        public const int SearchIterationLimit = 100000;

        // --- Базовые свойства ---

        public static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n < 4) return true;            // 2, 3
            if (n % 2 == 0) return false;
            for (int i = 3; (long)i * i <= n; i += 2)
                if (n % i == 0) return false;
            return true;
        }

        public static bool IsEven(int n) => n % 2 == 0;

        public static bool IsOdd(int n) => n % 2 != 0;

        public static bool IsMultipleOf(int n, int divisor)
        {
            if (divisor == 0) return false;
            return n % divisor == 0;
        }

        /// <summary>Точный квадрат: существует целое k, что k^2 == n. Отрицательные — не квадраты.</summary>
        public static bool IsSquare(int n)
        {
            if (n < 0) return false;
            int r = (int)Math.Round(Math.Sqrt(n));
            for (int k = Math.Max(0, r - 1); k <= r + 1; k++)
                if ((long)k * k == n) return true;
            return false;
        }

        /// <summary>Отрицательный квадрат: n = -(k^2), т.е. -n — точный квадрат и n &lt; 0.</summary>
        public static bool IsNegativeSquare(int n)
        {
            if (n >= 0) return false;
            return IsSquare(-n);
        }

        /// <summary>Куб: существует целое k, что k^3 == n (учитывает отрицательные).</summary>
        public static bool IsCube(int n)
        {
            long an = Math.Abs((long)n);
            int r = (int)Math.Round(Math.Cbrt(an));
            for (int k = Math.Max(0, r - 1); k <= r + 1; k++)
            {
                if ((long)k * k * k == an)
                    return true;
            }
            return false;
        }

        /// <summary>Квадрат простого числа: n = p^2, где p простое.</summary>
        public static bool IsPrimeSquared(int n)
        {
            if (n < 4) return false;
            int r = (int)Math.Round(Math.Sqrt(n));
            for (int k = Math.Max(0, r - 1); k <= r + 1; k++)
            {
                if ((long)k * k == n && IsPrime(k))
                    return true;
            }
            return false;
        }

        /// <summary>Палиндром по десятичной записи. Для отрицательных берётся модуль (знак игнорируется).</summary>
        public static bool IsPalindrome(int n)
        {
            long v = Math.Abs((long)n);
            long reversed = 0, original = v;
            while (v > 0)
            {
                reversed = reversed * 10 + v % 10;
                v /= 10;
            }
            return reversed == original;
        }

        /// <summary>Сумма цифр (по модулю числа).</summary>
        public static int DigitSum(int n)
        {
            long v = Math.Abs((long)n);
            int sum = 0;
            if (v == 0) return 0;
            while (v > 0)
            {
                sum += (int)(v % 10);
                v /= 10;
            }
            return sum;
        }

        /// <summary>Содержит ли десятичная запись указанную цифру (0..9).</summary>
        public static bool ContainsDigit(int n, int digit)
        {
            if (digit < 0 || digit > 9) return false;
            long v = Math.Abs((long)n);
            if (v == 0) return digit == 0;
            while (v > 0)
            {
                if (v % 10 == digit) return true;
                v /= 10;
            }
            return false;
        }

        // --- Построение предиката по типу свойства ---

        /// <summary>
        /// Строит предикат для свойства. Учёт босса «простые не считаются свойством»
        /// делается снаружи через overridePrimeToFalse (см. NumberPropertyPredicates).
        /// </summary>
        public static Predicate<int> BuildPredicate(NumberPropertyType type, int param = 0)
        {
            switch (type)
            {
                case NumberPropertyType.Prime: return IsPrime;
                case NumberPropertyType.Even: return IsEven;
                case NumberPropertyType.Odd: return IsOdd;
                case NumberPropertyType.MultipleOfN: return n => IsMultipleOf(n, param);
                case NumberPropertyType.Square: return IsSquare;
                case NumberPropertyType.Palindrome: return IsPalindrome;
                case NumberPropertyType.Cube: return IsCube;
                case NumberPropertyType.PrimeSquared: return IsPrimeSquared;
                case NumberPropertyType.DigitSumEquals: return n => DigitSum(n) == param;
                case NumberPropertyType.ContainsDigit: return n => ContainsDigit(n, param);
                case NumberPropertyType.NegativeSquare: return IsNegativeSquare;
                case NumberPropertyType.SquareOrNegativeSquare:
                    return n => IsSquare(n) || IsNegativeSquare(n);
                default:
                    return _ => false;
            }
        }

        // --- Поиск ближайшего числа со свойством ---

        /// <summary>
        /// Ближайшее число, строго БОЛЬШЕ from, удовлетворяющее предикату.
        /// Если не найдено в пределах лимита — возвращает from (значение не меняется).
        /// </summary>
        public static int Next(int from, Predicate<int> predicate)
        {
            for (int i = 1; i <= SearchIterationLimit; i++)
            {
                int candidate = from + i;
                if (predicate(candidate)) return candidate;
            }
            return from;
        }

        /// <summary>
        /// Ближайшее число, строго МЕНЬШЕ from, удовлетворяющее предикату.
        /// Если не найдено в пределах лимита — возвращает from (значение не меняется).
        /// </summary>
        public static int Prev(int from, Predicate<int> predicate)
        {
            for (int i = 1; i <= SearchIterationLimit; i++)
            {
                int candidate = from - i;
                if (predicate(candidate)) return candidate;
            }
            return from;
        }
    }
}
