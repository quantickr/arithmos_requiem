namespace ArithmosRequiem.Data
{
    /// <summary>
    /// Свойства чисел, используемые операциями «Мощь → Свойство» / «Мощь ← Свойство»
    /// и условиями карт.
    /// </summary>
    public enum NumberPropertyType
    {
        Prime,          // простое
        Even,           // чётное
        Odd,            // нечётное
        MultipleOfN,    // кратное N (param = N)
        Square,         // точный квадрат k^2
        Palindrome,     // палиндром
        Cube,           // куб k^3
        PrimeSquared,   // квадрат простого (p^2)
        DigitSumEquals, // сумма цифр равна param
        ContainsDigit,  // содержит цифру param
        NegativeSquare, // отрицательный квадрат: -(k^2)
        SquareOrNegativeSquare // квадрат ЛИБО отрицательный квадрат (карта №50)
    }
}
