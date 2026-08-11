namespace ArithmosRequiem.Core
{
    /// <summary>
    /// Хуки жизненного цикла босса, вызываемые движком/фасадами в фиксированных точках.
    /// Отделены от IBossModifiers (перехваты значений), но обычно реализуются одним
    /// классом Boss.
    /// </summary>
    public interface IBossHooks
    {
        void OnBattleStart(BattleContext ctx);
        void OnTurnStart(BattleContext ctx);
        void OnTurnEnd(BattleContext ctx);

        /// <summary>Реакция на розыгрыш карты (штраф Мощи Форос/Колосс, счётчик Зенон и т.п.).</summary>
        void OnCardPlayed(BattleContext ctx);

        /// <summary>Реакция на изменение значения кубика (штраф Мойра).</summary>
        void OnDiceChanged(BattleContext ctx);

        /// <summary>Отключён ли кубик с данным значением (Катабасис/Месос/Акрос).</summary>
        bool IsDieValueDisabled(int value);
    }
}
