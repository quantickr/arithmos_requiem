using System.Linq;
using ArithmosRequiem.Core;
using NUnit.Framework;

namespace ArithmosRequiem.Tests
{
    public class DiceManagerTests
    {
        [Test]
        public void RollAll_CreatesRequestedCount_WithSequence()
        {
            var rng = new FakeRandomProvider(d6Sequence: new[] { 3, 5 });
            var dice = new DiceManager(rng);

            var rolled = dice.RollAll(2);

            Assert.AreEqual(2, rolled.Count);
            Assert.AreEqual(3, rolled[0].Value);
            Assert.AreEqual(5, rolled[1].Value);
        }

        [Test]
        public void Apply_Add_ClampsToSix()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            var die = dice.AddDie(5);

            var result = dice.Apply(die, DieOp.Add(4)); // 5+4=9 → клампится до 6

            Assert.AreEqual(6, result.NewValue);
            Assert.AreEqual(5, result.OldValue);
        }

        [Test]
        public void Apply_Subtract_ClampsToOne()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            var die = dice.AddDie(2);

            var result = dice.Apply(die, DieOp.Sub(5)); // 2-5=-3 → 1

            Assert.AreEqual(1, result.NewValue);
        }

        [Test]
        public void Apply_Flip_IsSevenMinusValue()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            var die = dice.AddDie(2);

            var result = dice.Apply(die, DieOp.Flip()); // 7-2=5

            Assert.AreEqual(5, result.NewValue);
        }

        [Test]
        public void HasDouble_TrueWhenTwoSame()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            dice.AddDie(4);
            dice.AddDie(4);
            dice.AddDie(2);

            Assert.IsTrue(dice.HasDouble());
        }

        [Test]
        public void HasDouble_FalseWhenAllDifferent()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            dice.AddDie(1);
            dice.AddDie(2);
            dice.AddDie(3);

            Assert.IsFalse(dice.HasDouble());
        }

        [Test]
        public void HasValue_ReflectsPresence()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            dice.AddDie(3);

            Assert.IsTrue(dice.HasValue(3));
            Assert.IsFalse(dice.HasValue(6));
        }

        [Test]
        public void DisabledDie_ExcludedFromActiveQueries()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            var d1 = dice.AddDie(3);
            dice.AddDie(3);
            d1.IsDisabled = true;

            // Один активный кубик 3 → дубля нет, сумма только активного.
            Assert.IsFalse(dice.HasDouble());
            Assert.AreEqual(3, dice.SumOfActive());
            Assert.AreEqual(1, dice.ActiveCount());
            Assert.AreEqual(2, dice.TotalCount());
        }

        [Test]
        public void RemoveTemporary_KeepsPermanent()
        {
            var dice = new DiceManager(new FakeRandomProvider());
            dice.AddDie(3, temporary: false);
            dice.AddDie(4, temporary: true);

            dice.RemoveTemporary();

            Assert.AreEqual(1, dice.TotalCount());
            Assert.AreEqual(3, dice.Dice.Single().Value);
        }
    }
}
