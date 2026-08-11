namespace ArithmosRequiem.Core
{
    public enum BattleOutcome
    {
        InProgress,
        Win,   // Лимит понижен до 0
        Lose   // ходы кончились по Скорости
    }

    public enum TurnPhase
    {
        BattleStart,
        TurnStart,
        Draw,
        Roll,
        PlayLoop,
        TurnEnd,
        Resolve
    }

    public readonly struct BattleResult
    {
        public readonly BattleOutcome Outcome;
        public readonly int TurnsTaken;
        public readonly int FinalLimit;

        public BattleResult(BattleOutcome outcome, int turnsTaken, int finalLimit)
        {
            Outcome = outcome;
            TurnsTaken = turnsTaken;
            FinalLimit = finalLimit;
        }
    }
}
