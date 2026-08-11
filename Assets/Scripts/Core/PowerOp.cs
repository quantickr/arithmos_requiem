using ArithmosRequiem.Data;

namespace ArithmosRequiem.Core
{
    public enum PowerOpType
    {
        Add,          // +X
        Subtract,     // -X
        Set,          // =X
        Multiply,     // xX (может быть дробным множителем, напр. 1.5)
        Divide,       // :X
        ToProperty,   // Мощь → Свойство (строго больше)
        FromProperty  // Мощь ← Свойство (строго меньше)
    }

    /// <summary>
    /// Операция над Мощью. Множитель хранится как double (для x1.5, x1.2, x1.1, x1.25).
    /// Результат всегда приводится к int математическим округлением в PowerManager.
    /// </summary>
    public readonly struct PowerOp
    {
        public readonly PowerOpType Type;
        public readonly int IntAmount;              // для Add/Subtract/Set/Divide
        public readonly double Factor;              // для Multiply
        public readonly NumberPropertyType Property;// для To/FromProperty
        public readonly int PropertyParam;          // параметр свойства (N кратности, цифра, сумма)

        private PowerOp(PowerOpType type, int intAmount, double factor,
                        NumberPropertyType property, int propertyParam)
        {
            Type = type;
            IntAmount = intAmount;
            Factor = factor;
            Property = property;
            PropertyParam = propertyParam;
        }

        public static PowerOp Add(int x) =>
            new PowerOp(PowerOpType.Add, x, 0, default, 0);

        public static PowerOp Sub(int x) =>
            new PowerOp(PowerOpType.Subtract, x, 0, default, 0);

        public static PowerOp Set(int x) =>
            new PowerOp(PowerOpType.Set, x, 0, default, 0);

        public static PowerOp Mul(double factor) =>
            new PowerOp(PowerOpType.Multiply, 0, factor, default, 0);

        public static PowerOp Div(int x) =>
            new PowerOp(PowerOpType.Divide, x, 0, default, 0);

        public static PowerOp ToProperty(NumberPropertyType property, int param = 0) =>
            new PowerOp(PowerOpType.ToProperty, 0, 0, property, param);

        public static PowerOp FromProperty(NumberPropertyType property, int param = 0) =>
            new PowerOp(PowerOpType.FromProperty, 0, 0, property, param);
    }
}
