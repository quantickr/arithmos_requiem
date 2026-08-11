namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Абстракция генератора случайных чисел. Инъекция сида делает бой детерминированным
    /// (нужно для «золотых» тестов целого боя).
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>Случайное целое в диапазоне [minInclusive, maxInclusive].</summary>
        int Range(int minInclusive, int maxInclusive);

        /// <summary>Бросок d6: значение 1..6.</summary>
        int RollD6();
    }
}
