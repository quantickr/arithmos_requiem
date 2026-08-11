using ArithmosRequiem.Core;
using ArithmosRequiem.Utils;

namespace ArithmosRequiem.Bosses.Definitions
{
    /// <summary>№2 Артиос: Лимит не может понижаться нечётной Мощью.</summary>
    public sealed class Boss_02_Artios : Boss
    {
        public override string BossId => "boss_02_artios";
        public override string DisplayName => "Артиос";
        public override string AbilityText => "Лимит не может понижаться нечётной Мощью.";

        public override bool AllowLimitReduction(int effectivePower) =>
            NumberProperties.IsEven(effectivePower);
    }

    /// <summary>№1 Периссос: Лимит не может понижаться чётной Мощью.</summary>
    public sealed class Boss_01_Perissos : Boss
    {
        public override string BossId => "boss_01_perissos";
        public override string DisplayName => "Периссос";
        public override string AbilityText => "Лимит не может понижаться чётной Мощью.";

        public override bool AllowLimitReduction(int effectivePower) =>
            NumberProperties.IsOdd(effectivePower);
    }

    /// <summary>№3 Катабасис: кубики 1, 2 и 3 не работают и не дают Мощь.</summary>
    public sealed class Boss_03_Katabasis : Boss
    {
        public override string BossId => "boss_03_katabasis";
        public override string DisplayName => "Катабасис";
        public override string AbilityText => "Кубики 1, 2 и 3 не работают для эффектов и не дают Мощь.";

        public override bool IsDieValueDisabled(int value) => value == 1 || value == 2 || value == 3;
    }

    /// <summary>№6 Лета: -2 Добора во время боя.</summary>
    public sealed class Boss_06_Lethe : Boss
    {
        public override string BossId => "boss_06_lethe";
        public override string DisplayName => "Лета";
        public override string AbilityText => "-2 Добора во время этого боя.";

        public override void OnBattleStart(BattleContext ctx) => ctx.Stats.ModifyDraw(-2);
    }

    /// <summary>№11 Форос: -1 при розыгрыше карты.</summary>
    public sealed class Boss_11_Foros : Boss
    {
        public override string BossId => "boss_11_foros";
        public override string DisplayName => "Форос";
        public override string AbilityText => "-1 при розыгрыше карты.";

        public override void OnCardPlayed(BattleContext ctx) => ctx.ModifyPower(PowerOp.Sub(1));
    }

    /// <summary>№12 Мойра: -2 при изменении значения кубика.</summary>
    public sealed class Boss_12_Moira : Boss
    {
        public override string BossId => "boss_12_moira";
        public override string DisplayName => "Мойра";
        public override string AbilityText => "-2 при изменении значения кубика.";

        public override void OnDiceChanged(BattleContext ctx) => ctx.ModifyPower(PowerOp.Sub(2));
    }

    /// <summary>№13 Диплос: Лимит увеличен в 2 раза.</summary>
    public sealed class Boss_13_Diplos : Boss
    {
        public override string BossId => "boss_13_diplos";
        public override string DisplayName => "Диплос";
        public override string AbilityText => "Лимит увеличен в 2 раза.";

        public override void OnBattleStart(BattleContext ctx) => ctx.ScaleLimit(2.0);
    }

    /// <summary>№14 Метрон: за ход Мощь не может превысить треть изначального Лимита.</summary>
    public sealed class Boss_14_Metron : Boss
    {
        public override string BossId => "boss_14_metron";
        public override string DisplayName => "Метрон";
        public override string AbilityText => "За ход Мощь не может стать выше трети изначального Лимита.";

        public override int ModifyEffectivePower(int power, int initialLimit)
        {
            int cap = initialLimit / 3;
            return power > cap ? cap : power;
        }
    }

    /// <summary>№22 Тетрагон: +1 Добор; Лимит понижается только Мощью-квадратом.</summary>
    public sealed class Boss_22_Tetragon : Boss
    {
        public override string BossId => "boss_22_tetragon";
        public override string DisplayName => "Тетрагон";
        public override string AbilityText => "+1 Добор; Лимит понижается только Мощью — числом-квадратом.";

        public override void OnBattleStart(BattleContext ctx) => ctx.Stats.ModifyDraw(+1);
        public override bool AllowLimitReduction(int effectivePower) =>
            NumberProperties.IsSquare(effectivePower);
    }

    /// <summary>№23 Эратосфен: простые числа не считаются свойством; +12 Лимита.</summary>
    public sealed class Boss_23_Eratosthenes : Boss
    {
        public override string BossId => "boss_23_eratosthenes";
        public override string DisplayName => "Эратосфен";
        public override string AbilityText => "Простые числа не считаются свойством. +12 Лимита.";

        public override bool PrimesLoseProperties => true;
        public override void OnBattleStart(BattleContext ctx) => ctx.AdjustLimit(+12);
    }

    /// <summary>№25 Герас: в начале каждого хода -1 Кость до конца боя.</summary>
    public sealed class Boss_25_Geras : Boss
    {
        public override string BossId => "boss_25_geras";
        public override string DisplayName => "Герас";
        public override string AbilityText => "В начале каждого хода -1 Кость до конца боя.";

        public override void OnTurnStart(BattleContext ctx) => ctx.Stats.ModifyDice(-1);
    }
}
