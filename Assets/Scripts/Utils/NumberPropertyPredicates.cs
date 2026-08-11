using System;
using ArithmosRequiem.Data;

namespace ArithmosRequiem.Utils
{
    /// <summary>
    /// Построение предикатов свойств с учётом активных модификаторов боя
    /// (например, босс Эратосфен: «простые числа не считаются каким-либо числом
    /// в плане свойств»).
    /// </summary>
    public static class NumberPropertyPredicates
    {
        /// <summary>
        /// Строит предикат свойства. Если primesLoseProperties == true,
        /// то любое простое число НЕ считается обладающим свойством
        /// (кроме проверок на «нечётность/чётность»? — по правилам простые полностью
        /// теряют свойства, поэтому исключаем их из всех свойств).
        /// </summary>
        public static Predicate<int> Build(NumberPropertyType type, int param, bool primesLoseProperties)
        {
            Predicate<int> baseP = NumberProperties.BuildPredicate(type, param);
            if (!primesLoseProperties)
                return baseP;

            // Простые теряют свойства: если число простое — оно не обладает никаким свойством.
            return n => !NumberProperties.IsPrime(n) && baseP(n);
        }

        /// <summary>
        /// Проверка «обладает ли число свойством» с учётом модификатора простых.
        /// </summary>
        public static bool Has(int n, NumberPropertyType type, int param, bool primesLoseProperties)
        {
            return Build(type, param, primesLoseProperties)(n);
        }
    }
}
