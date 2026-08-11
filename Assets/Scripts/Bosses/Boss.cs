using ArithmosRequiem.Core;

namespace ArithmosRequiem.Bosses
{
    /// <summary>
    /// Базовый класс босса: реализует и хуки жизненного цикла (IBossHooks),
    /// и перехваты значений (IBossModifiers). Наследники переопределяют 1-3 метода.
    /// По умолчанию — ничего не меняет (нейтральный босс).
    /// </summary>
    public abstract class Boss : IBossHooks, IBossModifiers
    {
        public abstract string BossId { get; }
        public abstract string DisplayName { get; }
        public abstract string AbilityText { get; }

        // --- IBossHooks ---
        public virtual void OnBattleStart(BattleContext ctx) { }
        public virtual void OnTurnStart(BattleContext ctx) { }
        public virtual void OnTurnEnd(BattleContext ctx) { }
        public virtual void OnCardPlayed(BattleContext ctx) { }
        public virtual void OnDiceChanged(BattleContext ctx) { }
        public virtual bool IsDieValueDisabled(int value) => false;

        // --- IBossModifiers ---
        public virtual bool PrimesLoseProperties => false;
        public virtual bool AllowLimitReduction(int effectivePower) => true;
        public virtual int ModifyEffectivePower(int power, int initialLimit) => power;
        public virtual int ModifyLimitOnSet(int limit) => limit;
    }
}
