using ArithmosRequiem.Core;
using ArithmosRequiem.Data;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    public class PowerManagerTests
    {
        private PowerManager _pm;

        [SetUp]
        public void SetUp() => _pm = new PowerManager();

        [Test]
        public void Add_Subtract_Set_Work()
        {
            Assert.AreEqual(15, _pm.Apply(10, PowerOp.Add(5), null));
            Assert.AreEqual(4, _pm.Apply(10, PowerOp.Sub(6), null));
            Assert.AreEqual(7, _pm.Apply(10, PowerOp.Set(7), null));
        }

        [Test]
        public void Multiply_RoundsHalfUp()
        {
            // 5 * 1.5 = 7.5 → 8 (AwayFromZero).
            Assert.AreEqual(8, _pm.Apply(5, PowerOp.Mul(1.5), null));
        }

        [Test]
        public void Divide_RoundsHalfUp()
        {
            // 5 : 2 = 2.5 → 3.
            Assert.AreEqual(3, _pm.Apply(5, PowerOp.Div(2), null));
        }

        [Test]
        public void Divide_ByZero_IsIgnored()
        {
            Assert.AreEqual(10, _pm.Apply(10, PowerOp.Div(0), null));
        }

        [Test]
        public void Fractional_Flag_SetForFractionalResult()
        {
            _pm.Apply(5, PowerOp.Div(2), null); // 2.5
            Assert.IsTrue(_pm.LastOpWasFractional);
        }

        [Test]
        public void Fractional_Flag_ClearForIntegerResult()
        {
            _pm.Apply(6, PowerOp.Div(2), null); // 3.0
            Assert.IsFalse(_pm.LastOpWasFractional);
        }

        [Test]
        public void ToProperty_NextPrime()
        {
            // Мощь 7 → простое строго больше → 11.
            int r = _pm.Apply(7, PowerOp.ToProperty(NumberPropertyType.Prime), null);
            Assert.AreEqual(11, r);
        }

        [Test]
        public void FromProperty_PrevPrime()
        {
            // Мощь 8 ← простое строго меньше → 7.
            int r = _pm.Apply(8, PowerOp.FromProperty(NumberPropertyType.Prime), null);
            Assert.AreEqual(7, r);
        }

        [Test]
        public void ApplyPowerToLimit_NegativePower_RaisesLimit()
        {
            // Мощь -4 → Лимит растёт на 4.
            int newLimit = _pm.ApplyPowerToLimit(currentLimit: 12, power: -4, initialLimit: 12, bossMods: null);
            Assert.AreEqual(16, newLimit);
        }

        [Test]
        public void ApplyPowerToLimit_PositivePower_ReducesLimit()
        {
            int newLimit = _pm.ApplyPowerToLimit(currentLimit: 12, power: 5, initialLimit: 12, bossMods: null);
            Assert.AreEqual(7, newLimit);
        }

        [Test]
        public void ApplyPowerToLimit_NeverBelowZero()
        {
            int newLimit = _pm.ApplyPowerToLimit(currentLimit: 3, power: 10, initialLimit: 12, bossMods: null);
            Assert.AreEqual(0, newLimit);
        }

        [Test]
        public void ApplyPowerToLimit_BossBlocksReduction()
        {
            // Босс запрещает понижение любой Мощью → Лимит не меняется.
            var boss = new StubBossMods(allowReduction: false);
            int newLimit = _pm.ApplyPowerToLimit(12, 5, 12, boss);
            Assert.AreEqual(12, newLimit);
        }

        [Test]
        public void ApplyPowerToLimit_BossCapsEffectivePower()
        {
            // Босс ограничивает эффективную Мощь до 3.
            var boss = new StubBossMods(effectiveCap: 3);
            int newLimit = _pm.ApplyPowerToLimit(12, 10, 12, boss);
            Assert.AreEqual(9, newLimit); // 12 - 3
        }

        /// <summary>Управляемая заглушка IBossModifiers для тестов ApplyPowerToLimit.</summary>
        private sealed class StubBossMods : IBossModifiers
        {
            private readonly bool _allow;
            private readonly int? _cap;

            public StubBossMods(bool allowReduction = true, int? effectiveCap = null)
            {
                _allow = allowReduction;
                _cap = effectiveCap;
            }

            public bool PrimesLoseProperties => false;
            public bool AllowLimitReduction(int effectivePower) => _allow;
            public int ModifyEffectivePower(int power, int initialLimit) => _cap ?? power;
            public int ModifyLimitOnSet(int limit) => limit;
        }
    }
}
